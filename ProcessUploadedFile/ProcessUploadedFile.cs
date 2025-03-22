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
        private readonly string blobStorageConnection;

        // Constructor: Initialize logger and connection string with null check
        public ProcessUploadedFile(ILogger<ProcessUploadedFile> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            blobStorageConnection = Environment.GetEnvironmentVariable("analytiqstorage290Connection");
            if (string.IsNullOrEmpty(blobStorageConnection))
            {
                _logger.LogWarning("Blob storage connection string 'analytiqstorage290Connection' is not set.");
            }
        }

        // Main function entry point: Triggered by Event Grid
        [Function("ProcessUploadedFile")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            try
            {
                _logger.LogInformation("📩 Received Event: {EventType}", eventGridEvent.EventType);

                // Parse event data safely
                using var jsonDoc = JsonDocument.Parse(eventGridEvent.Data?.ToString() ?? "{}");
                var root = jsonDoc.RootElement;

                if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
                {
                    if (!root.TryGetProperty("url", out JsonElement urlElement) || urlElement.ValueKind != JsonValueKind.String)
                    {
                        _logger.LogError("❌ Blob URL missing or invalid in event data.");
                        return;
                    }
                    string blobUrl = urlElement.GetString();
                    _logger.LogInformation("🟢 New Blob Created: {BlobUrl}", blobUrl);

                    string extractedText = await ReadAndExtractTextFromBlobUrl(blobUrl);
                    _logger.LogInformation("📝 Extracted Text: {ExtractedText}...",
                        extractedText.Length > 500 ? extractedText.Substring(0, 500) : extractedText);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Event type {EventType} not handled.", eventGridEvent.EventType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error Processing Event: {Message}\nStack Trace: {StackTrace}", ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.LogError("❌ Inner Exception: {InnerMessage}", ex.InnerException.Message);
                }
                throw; // Re-throw to ensure function failure is recorded
            }
        }

        // Core method to process blob and extract text
        private async Task<string> ReadAndExtractTextFromBlobUrl(string blobUrl)
        {
            try
            {
                _logger.LogInformation("🔍 Processing blob URL: {BlobUrl}", blobUrl);

                if (string.IsNullOrEmpty(blobStorageConnection))
                {
                    _logger.LogError("❌ Blob storage connection string is missing.");
                    return "⚠️ Error: Azure Blob Storage connection string is missing.";
                }

                Uri uri;
                try
                {
                    uri = new Uri(blobUrl);
                }
                catch (UriFormatException ex)
                {
                    _logger.LogError("❌ Invalid blob URL format: {Message}", ex.Message);
                    return "⚠️ Error: Invalid blob URL format.";
                }

                string containerName = uri.Segments.Length > 1 ? uri.Segments[1].TrimEnd('/') : string.Empty;
                string blobPath = uri.Segments.Length > 2
                    ? Uri.UnescapeDataString(string.Join("", uri.Segments.Skip(2)))
                    : string.Empty;

                if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobPath))
                {
                    _logger.LogError("❌ Failed to parse container or blob path from URL: {BlobUrl}", blobUrl);
                    return "⚠️ Error: Invalid blob URL structure.";
                }

                _logger.LogInformation("🔍 Container: {ContainerName}, Blob Path: {BlobPath}", containerName, blobPath);

                if (containerName.Equals("processed-data", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("⏭️ Skipping processing of generated batch file in processed-data container.");
                    return "ℹ️ Skipped: File is in processed-data container.";
                }

                BlobServiceClient blobServiceClient = new BlobServiceClient(blobStorageConnection);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobPath);
                _logger.LogInformation("🔍 Created authenticated BlobClient for: {BlobUri}", blobClient.Uri);

                // Retry logic for blob existence check
                const int maxBlobRetries = 5;
                int retryDelayMs = 3000;
                for (int retry = 0; retry < maxBlobRetries; retry++)
                {
                    bool exists = await blobClient.ExistsAsync();
                    _logger.LogInformation("🔍 Blob exists check: {Exists}", exists);
                    if (exists) break;

                    if (retry == maxBlobRetries - 1)
                    {
                        _logger.LogError("❌ Blob does not exist after {MaxRetries} attempts.", maxBlobRetries);
                        return $"⚠️ Error: Blob not found after {maxBlobRetries} attempts.";
                    }

                    _logger.LogWarning("⏳ Blob not found, retrying... (Attempt {Attempt}/{MaxRetries})", retry + 1, maxBlobRetries);
                    await Task.Delay(retryDelayMs);
                    retryDelayMs *= 2; // Exponential backoff
                }

                BlobDownloadInfo download = await blobClient.DownloadAsync();
                using MemoryStream ms = new MemoryStream();
                await download.Content.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();

                BlobProperties properties = await blobClient.GetPropertiesAsync();
                string productName = properties.Metadata.TryGetValue("ProductName", out string value) ? value : null;
                _logger.LogInformation("🏷️ ProductName from metadata: {ProductName}", productName ?? "None");

                string fileName = Path.GetFileName(uri.LocalPath)?.Trim() ?? "unknown_file";
                string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                _logger.LogInformation("📂 File Type Detected: {FileExtension}", fileExtension);

                // Handle various file types
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
                    _logger.LogWarning("⚠️ Text extraction failed or unsupported format: {ExtractedText}", extractedText);
                    return extractedText;
                }

                string[] pathSegments = blobPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathSegments.Length < 2)
                {
                    _logger.LogError("❌ Invalid blob path format. Expected TenantId/BatchId structure.");
                    return "⚠️ Error: Invalid blob path format.";
                }

                string tenantId = pathSegments[0];
                string batchId = pathSegments[1];
                _logger.LogInformation("📤 Preparing to save to database: TenantID={TenantId}, FileName={FileName}, FileType={FileExtension}, ProductName={ProductName}",
                    tenantId, fileName, fileExtension, productName);

                AnalyzedTextResult aiResult = await AnalyzeTextWithGPT4(extractedText);
                if (string.IsNullOrEmpty(aiResult.Error))
                {
                    await SaveToDatabase(tenantId, fileName, fileExtension, extractedText, aiResult, productName);
                    await GenerateAndUploadBatchCsv(tenantId, batchId, fileName, fileExtension, extractedText, aiResult, productName);
                }
                else
                {
                    _logger.LogError("❌ Skipping database save and CSV generation due to GPT-4 error: {Error}", aiResult.Error);
                }

                _logger.LogInformation("📊 Processed {FileExtension} file, extracted {Length} characters.", fileExtension, extractedText.Length);
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error in ReadAndExtractTextFromBlobUrl: {Message}", ex.Message);
                return $"⚠️ Error accessing blob: {ex.Message}";
            }
        }

        // Extract text from PDF using Document AI with improved polling
        private async Task<string> ExtractTextFromPDF(byte[] pdfBytes)
        {
            string documentAiEndpoint = Environment.GetEnvironmentVariable("DOCUMENT_AI_ENDPOINT");
            string documentAiKey = Environment.GetEnvironmentVariable("DOCUMENT_AI_KEY");

            if (string.IsNullOrEmpty(documentAiEndpoint) || string.IsNullOrEmpty(documentAiKey))
            {
                _logger.LogError("❌ Document AI credentials are missing.");
                return "⚠️ Error: Document AI credentials missing.";
            }

            try
            {
                client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", documentAiKey);

                string base64File = Convert.ToBase64String(pdfBytes);
                _logger.LogInformation("📄 Base64 PDF Size: {Size} characters", base64File.Length);

                var requestBody = new { base64Source = base64File };
                string jsonBody = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                string apiUrl = $"{documentAiEndpoint}/formrecognizer/documentModels/prebuilt-layout:analyze?api-version=2023-07-31";
                _logger.LogInformation("🔍 Calling Document AI API at: {ApiUrl}", apiUrl);

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                _logger.LogInformation("📡 API Status: {StatusCode}, Headers: {Headers}", response.StatusCode, response.Headers);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    string operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("❌ Operation-Location header missing in response.");
                        return "⚠️ Error: Operation-Location header missing.";
                    }

                    _logger.LogInformation("⏳ Polling Operation-Location: {OperationLocation}", operationLocation);
                    return await PollForResult(operationLocation, base64File.Length);
                }

                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Document AI API Error: {StatusCode} - {Error}", response.StatusCode, error);
                return $"⚠️ API Error: {response.StatusCode} - {error}";
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error in ExtractTextFromPDF: {Message}", ex.Message);
                return $"⚠️ Error extracting PDF text: {ex.Message}";
            }
        }

        // Enhanced polling with dynamic retries based on file size
        private async Task<string> PollForResult(string operationLocation, long base64Size)
        {
            const int baseRetries = 30; // Base number of retries
            int maxRetries = baseRetries + (int)(base64Size / 5_000_000); // Increase retries for larger files (1 extra retry per 5MB)
            int delayMs = 3000; // Starting delay
            const int maxDelayMs = 15000; // Cap delay at 15 seconds

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    HttpResponseMessage resultResponse = await client.GetAsync(operationLocation);
                    string resultContent = await resultResponse.Content.ReadAsStringAsync();

                    if (resultResponse.IsSuccessStatusCode)
                    {
                        using JsonDocument doc = JsonDocument.Parse(resultContent);
                        JsonElement root = doc.RootElement;
                        string status = root.GetProperty("status").GetString();
                        _logger.LogInformation("📡 Polling status: {Status} (Attempt {Attempt}/{MaxRetries}, Delay: {Delay}ms)",
                            status, i + 1, maxRetries, delayMs);

                        if (status == "succeeded")
                        {
                            _logger.LogInformation("✅ Analysis succeeded!");
                            return ParseDocumentAIResponse(resultContent);
                        }
                        else if (status == "failed")
                        {
                            string error = root.TryGetProperty("error", out JsonElement errorElement)
                                ? errorElement.ToString() : "Unknown error";
                            _logger.LogError("❌ Analysis failed: {Error}", error);
                            return $"⚠️ Error: Analysis failed - {error}";
                        }
                    }
                    else
                    {
                        _logger.LogError("❌ Polling failed with status code: {StatusCode}", resultResponse.StatusCode);
                        return $"⚠️ Error: Polling failed with status code {resultResponse.StatusCode}";
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning("⚠️ Polling attempt {Attempt}/{MaxRetries} failed: {Message}. Retrying...",
                        i + 1, maxRetries, ex.Message);
                }

                _logger.LogInformation("⏳ Waiting for analysis to complete... (Attempt {Attempt}/{MaxRetries}, Next delay: {Delay}ms)",
                    i + 1, maxRetries, delayMs);
                await Task.Delay(delayMs);
                delayMs = Math.Min(delayMs + 1000, maxDelayMs); // Incremental backoff with cap
            }

            _logger.LogError("❌ Polling timed out after {MaxRetries} attempts!", maxRetries);
            return "⚠️ Error: Analysis timed out.";
        }

        // Parse Document AI response
        private static string ParseDocumentAIResponse(string jsonResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;
            StringBuilder extractedText = new StringBuilder();

            if (root.TryGetProperty("analyzeResult", out JsonElement analyzeResult) &&
                analyzeResult.TryGetProperty("pages", out JsonElement pages))
            {
                foreach (var page in pages.EnumerateArray())
                {
                    if (page.TryGetProperty("lines", out JsonElement lines))
                    {
                        foreach (var line in lines.EnumerateArray())
                        {
                            if (line.TryGetProperty("content", out JsonElement content))
                            {
                                extractedText.AppendLine(content.GetString());
                            }
                        }
                    }
                }
            }

            return extractedText.Length > 0 ? extractedText.ToString() : "⚠️ No text extracted from PDF.";
        }

        // GPT-4 analysis (unchanged for brevity, assumed robust)
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

                var messages = new[] { new { role = "system", content = systemMessage }, new { role = "user", content = $"Analyze this text: '{text}'" } };
                var requestBody = new { messages, max_tokens = 500 };
                string jsonBody = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("api-key", openAiKey);

                _logger.LogInformation("🚀 Sending request to GPT API: {ApiUrl}", apiUrl);
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ GPT API Error: {StatusCode} - {ResponseText}", response.StatusCode, responseText);
                    return new AnalyzedTextResult { Error = $"GPT API failed: {response.StatusCode} - {responseText}" };
                }

                using JsonDocument jsonDocument = JsonDocument.Parse(responseText);
                JsonElement root = jsonDocument.RootElement;
                if (root.TryGetProperty("choices", out JsonElement choices) && choices.EnumerateArray().Any())
                {
                    string jsonContent = choices[0].GetProperty("message").GetProperty("content").GetString();
                    var result = JsonSerializer.Deserialize<AnalyzedTextResult>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _logger.LogInformation("🔍 Analyzed Result: SentimentScore={Score}, Category={Category}, Urgency={Urgency}",
                        result.SentimentScore, result.SentimentCategory, result.UrgencyLevel);
                    return result;
                }

                _logger.LogError("❌ No valid choices in GPT response.");
                return new AnalyzedTextResult { Error = "No valid analysis result from GPT." };
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error calling GPT API: {Message}", ex.Message);
                return new AnalyzedTextResult { Error = $"GPT API failed: {ex.Message}" };
            }
        }

        // Save to database with retry logic
        private async Task SaveToDatabase(string tenantId, string fileName, string fileType, string extractedText, AnalyzedTextResult aiResult, string productName)
        {
            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("❌ SQL Connection String is missing!");
                throw new InvalidOperationException("SQL connection string not configured.");
            }

            const int maxTextLength = 1000000;
            string truncatedText = extractedText.Length > maxTextLength ? extractedText.Substring(0, maxTextLength) : extractedText;
            if (extractedText.Length > maxTextLength)
            {
                _logger.LogWarning("⚠️ Truncating text for database storage from {OriginalLength} to {MaxLength} characters.",
                    extractedText.Length, maxTextLength);
            }

            const int maxRetries = 3;
            int retryDelayMs = 2000;

            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    _logger.LogInformation("💾 Attempting database save (Attempt {Attempt}/{MaxRetries})...", retry + 1, maxRetries + 1);
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
                    cmd.Parameters.AddWithValue("@TenantID", tenantId ?? (object)DBNull.Value);
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
                    cmd.Parameters.AddWithValue("@ProcessingTime", DateTime.UtcNow);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation("✅ Saved {FileName} with ProductName={ProductName} to database. Rows affected: {Rows}",
                        fileName, productName, rowsAffected);
                    return;
                }
                catch (SqlException ex) when (retry < maxRetries && IsTransientSqlError(ex))
                {
                    _logger.LogWarning("⚠️ Database save failed: {Message}, SqlError: {Number}. Retrying in {Delay}ms...",
                        ex.Message, ex.Number, retryDelayMs);
                    await Task.Delay(retryDelayMs);
                    retryDelayMs *= 2;
                }
                catch (Exception ex)
                {
                    _logger.LogError("❌ Error saving to database: {Message}", ex.Message);
                    throw;
                }
            }

            _logger.LogError("❌ Failed to save to database after {MaxRetries} attempts.", maxRetries + 1);
            throw new Exception($"Database save failed after {maxRetries + 1} attempts.");
        }

        // Helper to detect transient SQL errors
        private static bool IsTransientSqlError(SqlException ex)
        {
            int[] transientErrors = { -2, 53, 258, 4060 };
            return transientErrors.Contains(ex.Number) ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("not currently available", StringComparison.OrdinalIgnoreCase);
        }

        // Generate and upload CSV with proper escaping
        private async Task GenerateAndUploadBatchCsv(string tenantId, string batchId, string fileName, string fileType,
            string extractedText, AnalyzedTextResult aiResult, string productName)
        {
            try
            {
                _logger.LogInformation("📄 Generating Batch CSV for TenantID={TenantId}, BatchID={BatchId}", tenantId, batchId);

                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteField("RecordID");
                csv.WriteField("TenantID");
                csv.WriteField("BatchID");
                csv.WriteField("UploadedFileName");
                csv.WriteField("OriginalText");
                csv.WriteField("SentimentScore");
                csv.WriteField("SentimentCategory");
                csv.WriteField("UrgencyLevel");
                csv.WriteField("KeyPhrases");
                csv.WriteField("RecommendationText");
                csv.WriteField("RelatedIssue");
                csv.WriteField("ImpactScore");
                csv.WriteField("CustomerSegmentGuess");
                csv.WriteField("ProductName");
                csv.WriteField("ProcessingTime");
                csv.NextRecord();

                csv.WriteField("1");
                csv.WriteField(tenantId);
                csv.WriteField(batchId);
                csv.WriteField(fileName);
                csv.WriteField(extractedText);
                csv.WriteField(aiResult.SentimentScore.ToString(CultureInfo.InvariantCulture));
                csv.WriteField(aiResult.SentimentCategory);
                csv.WriteField(aiResult.UrgencyLevel);
                csv.WriteField(aiResult.KeyPhrases);
                csv.WriteField(aiResult.RecommendationText);
                csv.WriteField(aiResult.RelatedIssue);
                csv.WriteField(aiResult.ImpactScore.ToString(CultureInfo.InvariantCulture));
                csv.WriteField(aiResult.CustomerSegmentGuess);
                csv.WriteField(productName);
                csv.WriteField(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
                csv.NextRecord();

                byte[] csvBytes = Encoding.UTF8.GetBytes(writer.ToString());
                BlobServiceClient blobServiceClient = new BlobServiceClient(blobStorageConnection);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("processed-data");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                string csvFileName = $"batch_{tenantId}_{batchId}_{timestamp}.csv";
                BlobClient blobClient = containerClient.GetBlobClient(csvFileName);
                using MemoryStream stream = new MemoryStream(csvBytes);
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation("📄 Uploaded Batch CSV: {CsvFileName}", csvFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error generating/uploading CSV: {Message}", ex.Message);
                throw;
            }
        }

        // Other extraction methods (unchanged for brevity, with minor robustness tweaks)
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
                _logger.LogError("❌ Error extracting DOCX: {Message}", ex.Message);
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
                while (await csv.ReadAsync())
                {
                    text.AppendLine(string.Join(",", csv.Parser.Record));
                }
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error extracting CSV: {Message}", ex.Message);
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
                        if (text.Length > 0) text.Length--; // Remove trailing comma
                        text.AppendLine();
                    }
                } while (reader.NextResult());
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error extracting Excel: {Message}", ex.Message);
                return $"⚠️ Error extracting Excel text: {ex.Message}";
            }
        }

        // Data model for GPT-4 results
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
    }
}