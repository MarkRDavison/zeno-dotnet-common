using mark.davison.common.client.web.abstractions.Repository;
using mark.davison.example.shared.models.dto.Scenarios.Commands;
using System.Text.Json;

namespace mark.davison.example.console.app;

public static class TestMethod
{

    public static async Task CallApi(IClientHttpRepository clientHttpRepository, string currentAccessToken)
    {
        //_apiClient.SetBearerToken(currentAccessToken);


        var response = await clientHttpRepository.Post<ExampleCommandResponse, ExampleCommandRequest>(new()
        {
            Payload = "This is from the client"
        }, CancellationToken.None);

        if (response.SuccessWithValue)
        {
            Console.WriteLine(JsonSerializer.Serialize(response.Value, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            Console.WriteLine($"Error: {string.Join(", ", response.Errors)}");
        }
    }

}
