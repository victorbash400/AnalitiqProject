using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace PowerBIEmbedFunction
{
    public class GetEmbedToken
    {
        private readonly ILogger _logger;

        public GetEmbedToken(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetEmbedToken>();
        }

        [Function("GetEmbedToken")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Starting GetEmbedToken function.");

            // Extract query parameters
            string reportIdString = req.Query["reportId"];
            string tenantId = req.Query["tenantId"]; // May be used for RLS if supported
            string useRlsString = req.Query["useRls"] ?? "false"; // Default to false
            bool useRls = Boolean.Parse(useRlsString);

            string groupIdString = Environment.GetEnvironmentVariable("POWERBI_GROUP_ID");
            string username = Environment.GetEnvironmentVariable("POWERBI_USERNAME");
            string password = Environment.GetEnvironmentVariable("POWERBI_PASSWORD");

            _logger.LogInformation($"Query params: reportId={reportIdString}, tenantId={tenantId}, groupId={groupIdString}, useRls={useRls}");

            // Validate inputs
            if (string.IsNullOrEmpty(reportIdString))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Missing required query parameter: reportId");
                return badRequest;
            }

            // Validate tenantId if RLS is enabled
            if (useRls && string.IsNullOrEmpty(tenantId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Missing required query parameter: tenantId (required when useRls=true)");
                return badRequest;
            }

            try
            {
                // Parse reportId as GUID
                if (!Guid.TryParse(reportIdString, out Guid reportId))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid reportId format. Must be a valid GUID.");
                    return errorResponse;
                }

                // Handle groupId: "me" for My Workspace or a GUID for shared workspace
                bool isMyWorkspace = groupIdString == "me";
                Guid groupId = Guid.Empty; // Default value, only used if not "me"
                if (!isMyWorkspace && !Guid.TryParse(groupIdString, out groupId))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid groupId format. Must be a valid GUID or 'me'.");
                    return errorResponse;
                }

                _logger.LogInformation($"Parsed: reportId={reportId}, tenantId={tenantId}, isMyWorkspace={isMyWorkspace}");

                // Step 1: Get access token using username/password
                _logger.LogInformation("Attempting to get access token.");
                var tokenCredentials = await GetAccessTokenAsync(username, password);
                _logger.LogInformation("Access token obtained successfully.");

                using (var client = new PowerBIClient(new Uri("https://api.powerbi.com"), tokenCredentials))
                {
                    // Step 2: Retrieve report details to get dataset ID
                    _logger.LogInformation("Fetching report details.");
                    var report = isMyWorkspace
                        ? await client.Reports.GetReportAsync(reportId)
                        : await client.Reports.GetReportInGroupAsync(groupId, reportId);
                    string datasetId = report.DatasetId;
                    _logger.LogInformation($"Report fetched: datasetId={datasetId}");

                    // Step 3: Generate embed token with or without RLS based on useRls parameter
                    if (isMyWorkspace)
                    {
                        // For "My Workspace", use the newer API with GenerateTokenRequestV2
                        var tokenRequest = new GenerateTokenRequestV2
                        {
                            Reports = new List<GenerateTokenRequestV2Report> { new GenerateTokenRequestV2Report(reportId) },
                            Datasets = new List<GenerateTokenRequestV2Dataset> { new GenerateTokenRequestV2Dataset(datasetId) }
                        };

                        // Only add identities if RLS is enabled
                        if (useRls)
                        {
                            _logger.LogInformation("Generating embed token with RLS.");
                            tokenRequest.Identities = new List<EffectiveIdentity>
                            {
                                new EffectiveIdentity(
                                    username: tenantId, // Use tenantId as the username for RLS
                                    roles: new List<string> { "TenantFilter" }, // Replace with your RLS role name
                                    datasets: new List<string> { datasetId })
                            };
                        }
                        else
                        {
                            _logger.LogInformation("Generating embed token without RLS.");
                        }

                        var tokenResponse = await client.EmbedToken.GenerateTokenAsync(tokenRequest);
                        _logger.LogInformation($"Embed token generated successfully.");

                        // Step 4: Construct embed URL
                        var embedUrl = $"https://app.powerbi.com/reportEmbed?reportId={reportId}";

                        // Step 5: Prepare JSON response
                        var responseData = new
                        {
                            EmbedUrl = embedUrl,
                            EmbedToken = tokenResponse.Token,
                            TokenExpiration = tokenResponse.Expiration
                        };

                        // Step 6: Return JSON response
                        var response = req.CreateResponse(HttpStatusCode.OK);
                        response.Headers.Add("Content-Type", "application/json");
                        await response.WriteStringAsync(JsonConvert.SerializeObject(responseData));
                        return response;
                    }
                    else
                    {
                        // For group workspace
                        GenerateTokenRequest tokenRequest;

                        if (useRls)
                        {
                            _logger.LogInformation("Generating embed token with RLS for group workspace.");
                            tokenRequest = new GenerateTokenRequest(
                                accessLevel: "View",
                                identities: new List<EffectiveIdentity>
                                {
                                    new EffectiveIdentity(
                                        username: tenantId,
                                        roles: new List<string> { "TenantFilter" },
                                        datasets: new List<string> { datasetId })
                                }
                            );
                        }
                        else
                        {
                            _logger.LogInformation("Generating embed token without RLS for group workspace.");
                            tokenRequest = new GenerateTokenRequest(accessLevel: "View");
                        }

                        var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(groupId, reportId, tokenRequest);
                        _logger.LogInformation($"Embed token generated successfully.");

                        // Step 4: Construct embed URL
                        var embedUrl = $"https://app.powerbi.com/reportEmbed?reportId={reportId}&groupId={groupId}";

                        // Step 5: Prepare JSON response
                        var responseData = new
                        {
                            EmbedUrl = embedUrl,
                            EmbedToken = tokenResponse.Token,
                            TokenExpiration = tokenResponse.Expiration
                        };

                        // Step 6: Return JSON response
                        var response = req.CreateResponse(HttpStatusCode.OK);
                        response.Headers.Add("Content-Type", "application/json");
                        await response.WriteStringAsync(JsonConvert.SerializeObject(responseData));
                        return response;
                    }
                }
            }
            catch (HttpOperationException ex)
            {
                _logger.LogError($"Power BI API error: {ex.Response.Content}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Power BI API error: {ex.Message}");
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating embed token: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        private async Task<TokenCredentials> GetAccessTokenAsync(string username, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "resource", "https://analysis.windows.net/powerbi/api" },
                    { "username", username },
                    { "password", password },
                    { "client_id", "1950a258-227b-4e31-a9cf-717495945fc2" }
                });

                var httpResponse = await httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/token", content);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to obtain access token: {errorContent}");
                }

                var tokenResponse = await httpResponse.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(tokenResponse);
                var accessToken = json["access_token"].ToString();
                return new TokenCredentials(accessToken, "Bearer");
            }
        }
    }
}