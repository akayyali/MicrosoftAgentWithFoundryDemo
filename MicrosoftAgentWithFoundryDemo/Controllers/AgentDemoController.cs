using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Runtime.Versioning;

namespace MicrosoftAgentWithFoundryDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentDemoController(ILogger<AgentDemoController> logger,IConfiguration configuration) : ControllerBase
    {
        const string agentName = "MyPOCAgent";

        [RequiresPreviewFeatures("AIProjectClient is a preview feature")]
        [HttpGet(Name = "Chat")]
        public async Task<ActionResult<string>> Chat(string chatMessage)
        {
            try
            {
                string projectEndpoint = configuration["FoundryProjectEndpoint"]
                    ?? throw new InvalidOperationException("Configuration key 'FoundryProjectEndpoint' is not set.");

                

                AIProjectClient projectClient = new(endpoint: new Uri(projectEndpoint), tokenProvider: new DefaultAzureCredential());


                AgentRecord agentRecord = projectClient.Agents.GetAgent(agentName);
                Console.WriteLine($"Agent retrieved (name: {agentRecord.Name}, id: {agentRecord.Id})");

                ProjectResponsesClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentRecord);
                // Use the agent to generate a response
                var response = responseClient.CreateResponse(chatMessage);

                var result = response.Value.GetOutputText(); // Get the output text from the response

                return result;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while chatting with the agent.");
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }



        }

    }
}
