using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class BlobEventsController : ControllerBase
{
    // ✅ Step 1: Allow Azure Webhook Validation
    [HttpOptions("handle-event")]
    public IActionResult ValidateWebhook()
    {
        Console.WriteLine("Received Azure Webhook Validation Request.");
        return Ok();
    }

    // ✅ Step 2: Handle Blob Upload Events
    [HttpPost("handle-event")]
    public IActionResult HandleBlobCreated([FromBody] JsonDocument eventData)
    {
        try
        {
            Console.WriteLine("🔹 Received Blob Event: " + eventData.RootElement.ToString());

            // ✅ Extract file URL from event data
            if (eventData.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                dataElement.TryGetProperty("url", out JsonElement urlElement))
            {
                string fileUrl = urlElement.GetString();
                Console.WriteLine($"📂 New file uploaded: {fileUrl}");

                // TODO: Trigger AI processing here
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing blob event: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
