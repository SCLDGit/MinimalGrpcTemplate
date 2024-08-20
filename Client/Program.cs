// See https://aka.ms/new-console-template for more information

using MinimalGrpcTemplate.Api.Protos.V1.Server;
using MinimalGrpcTemplate.Client.Services.ServerInteraction;

var input = string.Empty;

while ( string.IsNullOrWhiteSpace(input) )
{
    Console.Clear();
    Console.WriteLine("Enter numbers separated by spaces: ");
    input = Console.ReadLine();
}

var inputs = input.Split(' ');

var numbers = new List<int>();

foreach ( var number in inputs )
{
    if ( int.TryParse(number, out var parsedNumber) )
    {
        numbers.Add(parsedNumber);
    }
    else
    {
        Console.WriteLine($"'{number}' is not a valid number.");
        return 1;
    }
}

var serverConnectionInfoService = new ServerConnectionInfoService();

serverConnectionInfoService.SetUnauthenticatedChannel("localhost", 5000);

var client = new Server.ServerClient(serverConnectionInfoService.Channel);

var response = client.AddNumbers(new G_AddNumbersRequest
                                 {
                                     Numbers = { numbers }
                                 });

Console.WriteLine($"The sum of the numbers is: {response.Result}");

return 0;
