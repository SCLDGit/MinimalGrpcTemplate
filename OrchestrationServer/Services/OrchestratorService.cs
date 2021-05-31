using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using OrchestratorService;
using RequestHandlerService;
using PlatformService;
using ScanWinService;

namespace OrchestrationServer.Services
{
    public class OrchestratorService : Orchestrator.OrchestratorBase
    {
        public OrchestratorService()
        {
            HandlerClient = new Handler.HandlerClient(new Channel("localhost:30053", ChannelCredentials.Insecure));
            PlatformHandler = new Platform.PlatformClient(new Channel("localhost:30054", ChannelCredentials.Insecure));
            Task.Factory.StartNew(MonitorQueue, TaskCreationOptions.LongRunning);
        }

        private Dictionary<string, IServerStreamWriter<G_Response>> RegisteredClients { get; } = new();
        private Queue<G_OrchestrationRequest> RequestQueue { get; } = new();
        private Handler.HandlerClient HandlerClient { get; }
        private Platform.PlatformClient PlatformHandler { get; }
        

        public override Task<G_Response> EnqueueRequest(G_OrchestrationRequest request, ServerCallContext context)
        {
            RequestQueue.Enqueue(request);
            
            return Task.FromResult(new G_Response()
            {
                WasSuccessful = true,
                Response = "Request dispatched to queue..."
            });
        }

        public override async Task RegisterClient(G_RegisterClientRequest request, IServerStreamWriter<G_Response> responseStream, ServerCallContext context)
        {
            if (RegisteredClients.ContainsKey(request.ClientId))
            {
                await responseStream.WriteAsync(new G_Response()
                {
                    WasSuccessful = false,
                    Response = "ERROR: Could not register client - client ID already exists in server registry..."
                });

                return;
            }
            
            RegisteredClients.Add(request.ClientId, responseStream);

            await responseStream.WriteAsync(new G_Response()
            {
                WasSuccessful = true,
                Response = "Client registered successfully!"
            });

            // Leave the stream open to ensure that we can write to it as needed. - Comment by Matt Heimlich on 04/09/2021 @ 14:42:12
            while (RegisteredClients.ContainsKey(request.ClientId))
            {
                await Task.Delay(100);
            }
        }

        private async Task MonitorQueue()
        {
            while (true)
            {
                while (RequestQueue.Any())
                {
                    ProcessRequest(RequestQueue.Dequeue());
                }

                await Task.Delay(100);
            }
        }

        private void ProcessRequest(G_OrchestrationRequest p_request)
        {
            switch (p_request.RequestTypeCase)
            {
                case G_OrchestrationRequest.RequestTypeOneofCase.None:
                    break;
                case G_OrchestrationRequest.RequestTypeOneofCase.ScanRequest:
                    try
                    {
                        Console.WriteLine($"Sending Scan request...{p_request.OriginId}");
                        var platform = PlatformHandler.GetPlatform(p_request.ScanRequest);
                        Console.WriteLine($"Platform: {platform}");
                        Console.WriteLine($"Scan request {p_request.OriginId} finished successfully");
                    }
                    catch (RpcException exception)
                    {
                        Console.WriteLine(exception.InnerException);
                        throw;
                    }

                    break;
                case G_OrchestrationRequest.RequestTypeOneofCase.RequestType1:
                    try
                    {
                        HandlerClient.AddItem1(p_request.RequestType1);
                    }
                    catch (RpcException exception)
                    {
                        var client = RegisteredClients[p_request.OriginId];

                        Console.WriteLine($"The request handler server could not be reached - Status Code: {exception.StatusCode}");
                        client.WriteAsync(new G_Response()
                        {
                            WasSuccessful = false,
                            Response = $"The request handler server could not be reached - Status Code: {exception.StatusCode}"
                        });
                    }
                    break;
                case G_OrchestrationRequest.RequestTypeOneofCase.RequestType2:
                    try
                    {
                        HandlerClient.AddItem2(p_request.RequestType2);
                    }
                    catch (RpcException exception)
                    {
                        var client = RegisteredClients[p_request.OriginId];

                        Console.WriteLine($"The request handler server could not be reached - Status Code: {exception.StatusCode}");
                        client.WriteAsync(new G_Response()
                        {
                            WasSuccessful = false,
                            Response = $"The request handler server could not be reached - Status Code: {exception.StatusCode}"
                        });
                    }                    
                    break;
                case G_OrchestrationRequest.RequestTypeOneofCase.Response:
                    try
                    {
                        var client = RegisteredClients[p_request.OriginId];
                        client.WriteAsync(p_request.Response);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}