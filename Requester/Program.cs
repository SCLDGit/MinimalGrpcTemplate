using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.Data.Extensions;
using Grpc.Core;
using OrchestratorService;
using RequestHandlerService;
using PlatformService;

namespace Requester
{
    class Program
    {
        static async Task Main()
        {
            var channel = new Channel("localhost:30052", ChannelCredentials.Insecure);
            var client = new Orchestrator.OrchestratorClient(channel);

            var clientId = Guid.NewGuid().ToString();
            
            Console.WriteLine("Attempting to register client with server...");

            var responseStream = client.RegisterClient(new G_RegisterClientRequest()
            {
                ClientId = clientId
            });

            Task.Run(async () => MonitorMessages(responseStream));

            ConsoleKey key;

            Console.WriteLine("Press a key to send a server request. Press 'QUIT' to quit...");
            string command;
            do
            {
                command = Console.ReadLine();

                var commands = command.Split(" ");
                var endpointRequest = new G_OrchestrationRequest()
                {
                    OriginId = clientId,
                };
                switch (commands[0].ToUpper())
                {
                    case "SCAN":
                        endpointRequest.ScanRequest = ProcessScanCommand(commands, clientId);
                        break;
                    case "REMEDIATE":
                        break;
                }

                Console.WriteLine();

                Console.WriteLine("Sending client request to server...");

                try
                {
                    var reply = client.EnqueueRequest(endpointRequest);
                    
                    Console.WriteLine(reply.Response);
                }
                catch (RpcException exception)
                {
                    Console.WriteLine($"The server could not be reached - Status Code: {exception.StatusCode}");
                }
                
            } while (command != null && command.ToUpper() != "EXIT" && command.ToUpper() != "QUIT");
        }
        
        private static async void MonitorMessages(AsyncServerStreamingCall<G_Response> p_responseStream)
        {
            while (true)
            {
                while (await p_responseStream.ResponseStream.MoveNext())
                {
                    var response = p_responseStream.ResponseStream.Current;
                    Console.WriteLine(response.Response);
                }

                await Task.Delay(100);
            }
        }

        private static void ProcessCommand(string p_command, string p_clientId)
        {
            var commands = p_command.Split(" ");
            switch (commands[0].ToUpper())
            {
                case "SCAN":
                    ProcessScanCommand(commands, p_clientId);
                    break;

                case "REMEDIATE":
                    break;
            }

            Console.WriteLine();
        }

        private static G_ScanRequest ProcessScanCommand(IList<string> p_commands, string p_clientId)
        {
            var endpointAddressIndex = p_commands.FindIndex(p_command => p_command.ToUpper().Equals("-A")) + 1;
            var endpointPlatformIndex = p_commands.FindIndex(p_command => p_command.ToUpper().Equals("-T")) + 1;
            var policyVersionItemNameIndex = p_commands.FindIndex(p_command => p_command.ToUpper().Equals("-P")) + 1;
            var policyVersionItemVersionIndex = p_commands.FindIndex(p_command => p_command.ToUpper().Equals("-PV")) + 1;
            var endpointPlatformName = endpointPlatformIndex == 0 ? string.Empty : p_commands[endpointPlatformIndex];
            var policyVersionItemName = policyVersionItemNameIndex == 0 ? string.Empty : p_commands[policyVersionItemNameIndex];
            var policyVersionItemVersion = policyVersionItemVersionIndex == 0 ? string.Empty : p_commands[policyVersionItemVersionIndex];


            var RequestScan = new G_ScanRequest()
            {
                OriginId = p_clientId,
                EndPointAddress = p_commands[endpointAddressIndex],
                EndPointPlatform = endpointPlatformName,
                PolicyItemName = policyVersionItemName,
                PolicyItenVersion = policyVersionItemVersion
            };
            return RequestScan;
        }
    }
}