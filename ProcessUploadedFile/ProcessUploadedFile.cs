using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml.Packaging;
using CsvHelper;
using System.Globalization;
using ExcelDataReader;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace AnalytiQFunctions
{
    public class ProcessUploadedFile
    {
        private readonly ILogger<ProcessUploadedFile> _logger;
        private static readonly HttpClient client = new HttpClient();
        private readonly string blobStorageConnection = Environment.GetEnvironmentVariable("analytiqstorage290Connection");

        public ProcessUploadedFile(ILogger<ProcessUploadedFile> logger)
        {
            _logger = logger;
        }

        [Function("ProcessUploadedFile")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            try
            {
                _logger.LogInformation($"📩 Received Event: {eventGridEvent.EventType}");

                using var jsonDoc = JsonDocument.Parse(eventGridEvent.Data.ToString());
                var root = jsonDoc.RootElement;

                if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
                {
                    string blobUrl = root.GetProperty("url").GetString();
                    _logger.LogInformation($"🟢 New Blob Created: {blobUrl}");

                    string extractedText = await ReadAndExtractTextFromBlobUrl(blobUrl);
                    _logger.LogInformation($"📝 Extracted Text: {extractedText.Substring(0, Math.Min(500, extractedText.Length))}...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error Processing Event: {ex.Message}");
                _logger.LogError($"❌ Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null) _logger.LogError($"❌ Inner Exception: {ex.InnerException.Message}");
                throw;
            }
        }

        private async Task<string> ReadAndExtractTextFromBlobUrl(string blobUrl)
        {
            try
            {
                _logger.LogInformation($"🔍 Processing blob URL: {blobUrl}");

                if (string.IsNullOrEmpty(blobStorageConnection))
                {
                    _logger.LogError("❌ Blob storage connection string 'analytiqstorage290Connection' is missing!");
                    return "⚠️ Error: Azure Blob Storage connection string is missing.";
                }

                Uri uri = new Uri(blobUrl);
                string containerName = uri.Segments[1].TrimEnd('/');
                string blobPath = string.Join("", uri.Segments.Skip(2));
                _logger.LogInformation($"🔍 Container: {containerName}, Blob Path: {blobPath}");

                if (containerName == "processed-data")
                {
                    _logger.LogInformation($"⏭️ Skipping processing of generated batch file in processed-data container");
                    return "ℹ️ Skipped: File is in processed-data container.";
                }

                BlobServiceClient blobServiceClient = new BlobServiceClient(blobStorageConnection);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobPath);
                _logger.LogInformation($"🔍 Created authenticated BlobClient for: {blobClient.Uri}");

                int retryCount = 5, retryDelay = 3000;
                while (retryCount > 0)
                {
                    bool exists = await blobClient.ExistsAsync();
                    _logger.LogInformation($"🔍 Blob exists check: {exists}");
                    if (exists) break;
                    _logger.LogWarning($"⏳ Blob not found, retrying... ({retryCount} attempts left)");
                    await Task.Delay(retryDelay);
                    retryCount--;
                    retryDelay *= 2;
                }

                if (!(await blobClient.ExistsAsync()))
                {
                    _logger.LogError($"❌ Error: Blob still does not exist after retries!");
                    return $"⚠️ Error: Blob not found after {5 - retryCount} attempts.";
                }

                BlobDownloadInfo download = await blobClient.DownloadAsync();
                using MemoryStream ms = new MemoryStream();
                await download.Content.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();

                BlobProperties properties = await blobClient.GetPropertiesAsync();
                string productName = properties.Metadata.TryGetValue("ProductName", out string value) ? value : null;
                _logger.LogInformation($"🏷️ ProductName from metadata: {productName ?? "None"}");

                string fileName = Path.GetFileName(uri.LocalPath);
                string fileExtension = Path.GetExtension(fileName).ToLower();
                _logger.LogInformation($"📂 File Type Detected: {fileExtension}");

                string extractedText = fileExtension switch
                {
                    ".pdf" => await ExtractTextFromPDF(fileBytes),
                    ".docx" => await ExtractTextFromDOCX(fileBytes),
                    ".txt" => Encoding.UTF8.GetString(fileBytes),
                    ".csv" => await ExtractTextFromCSV(fileBytes),
                    ".xlsx" => await ExtractTextFromExcel(fileBytes),
                    _ => "⚠️ Unsupported file format."
                };

                if (extractedText.StartsWith("⚠️"))
                {
                    _logger.LogWarning($"⚠️ Text extraction failed or unsupported format: {extractedText}");
                    return extractedText;
                }

                string[] pathSegments = blobPath.Split('/');
                if (pathSegments.Length < 2)
                {
                    _logger.LogError("❌ Invalid blob path format. Expected TenantId/BatchId structure.");
                    return "⚠️ Error: Invalid blob path format.";
                }

                string tenantId = pathSegments[0];
                string batchId = pathSegments[1];
                _logger.LogInformation($"📤 Preparing to save to database: TenantID={tenantId}, FileName={fileName}, FileType={fileExtension}, ProductName={productName}");

                AnalyzedTextResult aiResult = await AnalyzeTextWithGPT4(extractedText);
                if (string.IsNullOrEmpty(aiResult.Error))
                {
                    await SaveToDatabase(tenantId, fileName, fileExtension, extractedText, aiResult, productName);
                    await GenerateAndUploadBatchCsv(tenantId, batchId, fileName, fileExtension, extractedText, aiResult, productName);
                }
                else
                {
                    _logger.LogError($"❌ Skipping database save and CSV generation due to GPT-4 error: {aiResult.Error}");
                }

                _logger.LogInformation($"📊 Processed {fileExtension} file, extracted {extractedText.Length} characters.");
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ReadAndExtractTextFromBlobUrl: {ex.Message}");
                return $"⚠️ Error accessing blob: {ex.Message}";
            }
        }

        private async Task<AnalyzedTextResult> AnalyzeTextWithGPT4(string text)
        {
            try
            {
                var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "AsrRDxpNJI9vB6lUWEa15ZfvQl1LMOxaAoeeSFxHMqdFzsOO2RigJQQJ99BCACfhMk5XJ3w3AAAAACOGrScJ";
                var endpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT") ?? "https://victo-m8ad3gdl-swedencentral.cognitiveservices.azure.com/";
                var deploymentName = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-analytiq";

                string apiUrl = $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-01";

                var systemMessage = @"You are an expert analyst processing customer feedback. For each input text, analyze it and return a JSON object with these fields:
    - SentimentScore (float, -1 to 1): Overall sentiment, where -1 is very negative and 1 is very positive.
    - SentimentCategory (string): 'Positive', 'Negative', or 'Neutral'.
    - UrgencyLevel (string): 'Low', 'Medium', 'High', or 'Critical' based on how urgent the feedback sounds.
    - KeyPhrases (string): Comma-separated list of important phrases.
    - RecommendationText (string): One actionable suggestion to improve based on the feedback.
    - RelatedIssue (string): The main problem mentioned (e.g., 'Shipping Delay').
    - ImpactScore (float, 0-1): How severe the issue seems, 0 being minor and 1 being major.
    - CustomerSegmentGuess (string): Guess the customer type: 'Premium User', 'New Customer', or 'Regular User'.
    Be precise and avoid wild guesses. If unsure, keep scores conservative.
    Return only the JSON object without any markdown formatting or backticks.";

                var messages = new[]
                {
            new { role = "system", content = systemMessage },
            new { role = "user", content = $"Analyze this text: '{text}'" }
        };

                var requestBody = new { messages = messages, max_tokens = 500 };
                var jsonBody = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("api-key", openAiKey);

                _logger.LogInformation($"🚀 Sending request to GPT API: {apiUrl}");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ GPT API Error: {response.StatusCode} - {responseText}");
                    return new AnalyzedTextResult { Error = $"GPT API failed: {response.StatusCode} - {responseText}" };
                }

                _logger.LogInformation($"✅ GPT Response: {responseText}");

                try
                {
                    // Parse the entire response first
                    using JsonDocument jsonDocument = JsonDocument.Parse(responseText);
                    JsonElement root = jsonDocument.RootElement;

                    // Check if choices array exists and has elements
                    if (root.TryGetProperty("choices", out JsonElement choicesElement) &&
                        choicesElement.ValueKind == JsonValueKind.Array &&
                        choicesElement.GetArrayLength() > 0)
                    {
                        // Get the first choice
                        JsonElement firstChoice = choicesElement[0];

                        // Extract message content if it exists
                        if (firstChoice.TryGetProperty("message", out JsonElement messageElement) &&
                            messageElement.TryGetProperty("content", out JsonElement contentElement))
                        {
                            string jsonContent = contentElement.GetString();
                            _logger.LogInformation($"📄 Extracted content: {jsonContent}");

                            // Parse the content as JSON (which contains our actual result)
                            try
                            {
                                var options = new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                };

                                var result = JsonSerializer.Deserialize<AnalyzedTextResult>(jsonContent, options);
                                if (result != null)
                                {
                                    _logger.LogInformation($"🔍 Analyzed Result: SentimentScore={result.SentimentScore}, Category={result.SentimentCategory}, Urgency={result.UrgencyLevel}");
                                    return result;
                                }
                                else
                                {
                                    _logger.LogError("❌ Deserialized result is null.");
                                    return new AnalyzedTextResult { Error = "Failed to deserialize GPT content to result object." };
                                }
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError($"❌ JSON parsing error in content: {ex.Message}");
                                _logger.LogError($"❌ Problematic content: {jsonContent}");
                                return new AnalyzedTextResult { Error = $"JSON parsing failed: {ex.Message}" };
                            }
                        }
                        else
                        {
                            _logger.LogError("❌ Missing message or content property in choice.");
                            return new AnalyzedTextResult { Error = "Missing message or content in GPT response." };
                        }
                    }
                    else
                    {
                        _logger.LogError("❌ No choices array or empty choices in GPT response.");
                        return new AnalyzedTextResult { Error = "No choices in GPT response." };
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"❌ Error parsing overall GPT response: {ex.Message}");
                    return new AnalyzedTextResult { Error = $"Failed to parse GPT response: {ex.Message}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error calling GPT API: {ex.Message}");
                return new AnalyzedTextResult { Error = $"GPT API failed: {ex.Message}" };
            }
        }

        // Classes for GPT-4 chat completion response structure
        private class ChatCompletionResponse
        {
            public List<Choice> Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; }
        }

        private class Message
        {
            public string Content { get; set; }
        }

        private class AnalyzedTextResult
        {
            public float SentimentScore { get; set; }
            public string SentimentCategory { get; set; }
            public string UrgencyLevel { get; set; }
            public string KeyPhrases { get; set; }
            public string RecommendationText { get; set; }
            public string RelatedIssue { get; set; }
            public float ImpactScore { get; set; }
            public string CustomerSegmentGuess { get; set; }
            public string Error { get; set; }
        }

        private async Task SaveToDatabase(string tenantId, string fileName, string fileType, string extractedText, AnalyzedTextResult aiResult, string productName)
        {
            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("❌ SQL Connection String is missing!");
                return;
            }

            int maxRetries = 3;
            int retryDelayMs = 2000;

            const int maxTextLength = 1000000;
            string truncatedText = extractedText.Length > maxTextLength ? extractedText.Substring(0, maxTextLength) : extractedText;
            if (extractedText.Length > maxTextLength)
            {
                _logger.LogWarning($"⚠️ Truncating text for database storage from {extractedText.Length} to {maxTextLength} characters");
            }

            for (int retryCount = 0; retryCount <= maxRetries; retryCount++)
            {
                try
                {
                    _logger.LogInformation($"💾 Attempting database connection (Attempt {retryCount + 1}/{maxRetries + 1})...");

                    using SqlConnection conn = new SqlConnection(connectionString);
                    await conn.OpenAsync();

                    string query = @"
                INSERT INTO dbo.ProcessedData (
                    TenantID, UploadedFileName, FileType, OriginalText, CleanedText, SentimentScore, SentimentCategory, 
                    UrgencyLevel, KeyPhrases, RecommendationText, RelatedIssue, ImpactScore, CustomerSegmentGuess, 
                    ProductName, ProcessingTime
                )
                VALUES (
                    @TenantID, @UploadedFileName, @FileType, @OriginalText, @CleanedText, @SentimentScore, @SentimentCategory, 
                    @UrgencyLevel, @KeyPhrases, @RecommendationText, @RelatedIssue, @ImpactScore, @CustomerSegmentGuess, 
                    @ProductName, @ProcessingTime
                );";

                    using SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TenantID", tenantId);
                    cmd.Parameters.AddWithValue("@UploadedFileName", fileName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileType", fileType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OriginalText", truncatedText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CleanedText", truncatedText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SentimentScore", aiResult.SentimentScore);
                    cmd.Parameters.AddWithValue("@SentimentCategory", aiResult.SentimentCategory ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UrgencyLevel", aiResult.UrgencyLevel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KeyPhrases", aiResult.KeyPhrases ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RecommendationText", aiResult.RecommendationText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelatedIssue", aiResult.RelatedIssue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImpactScore", aiResult.ImpactScore);
                    cmd.Parameters.AddWithValue("@CustomerSegmentGuess", aiResult.CustomerSegmentGuess ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProductName", productName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProcessingTime", DateTime.UtcNow); // ✅ Added Processing Time

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"✅ Saved {fileName} with ProductName={productName} to ProcessedData. Rows affected: {rowsAffected}");
                    return;
                }
                catch (SqlException ex) when (ex.Number == -2 || ex.Number == 53 || ex.Number == 258 || ex.Number == 4060
                                           || ex.Message.Contains("not currently available")
                                           || ex.Message.Contains("timeout") && retryCount < maxRetries)
                {
                    _logger.LogWarning($"⚠️ Database connection attempt {retryCount + 1} failed: {ex.Message}, SqlError: {ex.Number}. Retrying in {retryDelayMs}ms...");
                    await Task.Delay(retryDelayMs);
                    retryDelayMs *= 2;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Error saving to database: {ex.Message}");
                    if (ex.InnerException != null)
                        _logger.LogError($"❌ Inner Exception: {ex.InnerException.Message}");
                    throw;
                }
            }

            _logger.LogError($"❌ Failed to connect to database after {maxRetries + 1} attempts");
            throw new Exception($"Failed to connect to database after {maxRetries + 1} attempts");
        }


        private async Task GenerateAndUploadBatchCsv(string tenantId, string batchId, string fileName, string fileType, string extractedText, AnalyzedTextResult aiResult, string productName)
        {
            try
            {
                _logger.LogInformation($"📄 Generating Batch Processed Data CSV for TenantID={tenantId}, BatchID={batchId}");

                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("RecordID,TenantID,BatchID,UploadedFileName,OriginalText,SentimentScore,SentimentCategory,UrgencyLevel,KeyPhrases,RecommendationText,RelatedIssue,ImpactScore,CustomerSegmentGuess,ProductName,ProcessingTime");

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string originalTextEscaped = extractedText.Replace(",", " ").Replace("\r\n", " ").Replace("\n", " ");
                string keyPhrasesEscaped = aiResult.KeyPhrases?.Replace(",", ";") ?? "";
                string csvLine = $"1,{tenantId},{batchId},{fileName},{originalTextEscaped},{aiResult.SentimentScore},{aiResult.SentimentCategory},{aiResult.UrgencyLevel},{keyPhrasesEscaped},{aiResult.RecommendationText},{aiResult.RelatedIssue},{aiResult.ImpactScore},{aiResult.CustomerSegmentGuess},{productName},{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}";
                csvContent.AppendLine(csvLine);

                byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

                BlobServiceClient blobServiceClient = new BlobServiceClient(blobStorageConnection);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("processed-data");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                string csvFileName = $"batch_{tenantId}_{batchId}_{timestamp}.csv";
                BlobClient blobClient = containerClient.GetBlobClient(csvFileName);
                using MemoryStream stream = new MemoryStream(csvBytes);
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"📄 Uploaded Batch Processed Data CSV: {csvFileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating/uploading CSV: {ex.Message}");
                _logger.LogError($"❌ Stack Trace: {ex.StackTrace}");
            }
        }

        private async Task<string> ExtractTextFromPDF(byte[] pdfBytes)
        {
            var documentAiEndpoint = Environment.GetEnvironmentVariable("DOCUMENT_AI_ENDPOINT");
            var documentAiKey = Environment.GetEnvironmentVariable("DOCUMENT_AI_KEY");

            if (string.IsNullOrEmpty(documentAiEndpoint) || string.IsNullOrEmpty(documentAiKey))
            {
                _logger.LogError("❌ Document AI credentials are missing!");
                return "⚠️ Error: Document AI credentials missing.";
            }

            try
            {
                client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", documentAiKey);

                string base64File = Convert.ToBase64String(pdfBytes);
                _logger.LogInformation($"📄 Base64 PDF Size: {base64File.Length} characters");

                var requestBody = new { base64Source = base64File };
                string jsonBody = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                string apiUrl = $"{documentAiEndpoint}/formrecognizer/documentModels/prebuilt-layout:analyze?api-version=2023-07-31";
                _logger.LogInformation($"🔍 Calling Document AI API at: {apiUrl}");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                _logger.LogInformation($"📡 API Status: {response.StatusCode}, Headers: {response.Headers}");

                if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Document AI API Error: {error}");
                    return $"⚠️ API Error: {error}";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    string operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("❌ Operation-Location header missing in response!");
                        return "⚠️ Error: Operation-Location header missing.";
                    }

                    _logger.LogInformation($"⏳ Polling Operation-Location: {operationLocation}");
                    return await PollForResult(operationLocation);
                }

                _logger.LogError($"❌ Unexpected status code: {response.StatusCode}");
                return $"⚠️ Error: Unexpected API response status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ExtractTextFromPDF: {ex.Message}");
                return $"⚠️ Error extracting PDF text: {ex.Message}";
            }
        }

        private async Task<string> PollForResult(string operationLocation)
        {
            int maxRetries = 10, delayMs = 3000;
            for (int i = 0; i < maxRetries; i++)
            {
                HttpResponseMessage resultResponse = await client.GetAsync(operationLocation);
                string resultContent = await resultResponse.Content.ReadAsStringAsync();

                if (resultResponse.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(resultContent);
                    JsonElement root = doc.RootElement;
                    string status = root.GetProperty("status").GetString();
                    _logger.LogInformation($"📡 Polling status: {status}");

                    if (status == "succeeded")
                    {
                        _logger.LogInformation("✅ Analysis succeeded!");
                        return ParseDocumentAIResponse(resultContent);
                    }
                    else if (status == "failed")
                    {
                        string error = root.GetProperty("error").ToString();
                        _logger.LogError($"❌ Analysis failed: {error}");
                        return $"⚠️ Error: Analysis failed - {error}";
                    }
                }

                _logger.LogInformation($"⏳ Waiting for analysis to complete... (Attempt {i + 1}/{maxRetries})");
                await Task.Delay(delayMs);
                delayMs += 1000; // Incremental backoff
            }

            _logger.LogError("❌ Polling timed out!");
            return "⚠️ Error: Analysis timed out.";
        }

        private static string ParseDocumentAIResponse(string jsonResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;
            StringBuilder extractedText = new StringBuilder();

            foreach (var page in root.GetProperty("analyzeResult").GetProperty("pages").EnumerateArray())
            {
                foreach (var line in page.GetProperty("lines").EnumerateArray())
                {
                    extractedText.AppendLine(line.GetProperty("content").GetString());
                }
            }

            return extractedText.ToString();
        }

        private async Task<string> ExtractTextFromDOCX(byte[] docxBytes)
        {
            try
            {
                using MemoryStream stream = new MemoryStream(docxBytes);
                using WordprocessingDocument wordDoc = await Task.Run(() => WordprocessingDocument.Open(stream, false));
                StringBuilder text = new StringBuilder();

                foreach (var para in wordDoc.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    text.AppendLine(para.InnerText);
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error extracting text from DOCX: {ex.Message}");
                return $"⚠️ Error extracting DOCX text: {ex.Message}";
            }
        }

        private async Task<string> ExtractTextFromCSV(byte[] csvBytes)
        {
            try
            {
                using MemoryStream stream = new MemoryStream(csvBytes);
                using StreamReader reader = new StreamReader(stream);
                using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                StringBuilder text = new StringBuilder();

                while (csv.Read())
                {
                    text.AppendLine(string.Join(",", csv.Parser.Record));
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error extracting text from CSV: {ex.Message}");
                return $"⚠️ Error extracting CSV text: {ex.Message}";
            }
        }

        private async Task<string> ExtractTextFromExcel(byte[] excelBytes)
        {
            try
            {
                using MemoryStream stream = new MemoryStream(excelBytes);
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                StringBuilder text = new StringBuilder();
                int rowCount = 0;

                do
                {
                    while (reader.Read() && rowCount++ < 1000)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            text.Append(reader.GetValue(i)?.ToString() + ",");
                        }
                        text.Length--; // Remove trailing comma
                        text.AppendLine();
                    }
                } while (reader.NextResult());

                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error extracting text from Excel: {ex.Message}");
                return $"⚠️ Error extracting Excel text: {ex.Message}";
            }
        }
    }
}