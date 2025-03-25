using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UdpServer.Services
{
    public class UdpServerService : IHostedService, IDisposable
    {
        private readonly ILogger<UdpServerService> _logger;
        private UdpClient _udpListener;
        private readonly Random _random = new Random();
        private Task _executingTask;
        private CancellationTokenSource _cts;

        public UdpServerService(ILogger<UdpServerService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = RunServerAsync(_cts.Token);
            return Task.CompletedTask;
        }

        private async Task RunServerAsync(CancellationToken cancellationToken)
        {
            _udpListener = new UdpClient(8080);
            _logger.LogInformation($"UDP Server is listening on port 8080");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _udpListener.ReceiveAsync(cancellationToken);
                    _ = HandleMessageAsync(result);
                }
            }
            catch (OperationCanceledException)
            {
                // Server is stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UDP server");
            }
        }

        private async Task HandleMessageAsync(UdpReceiveResult result)
        {
            var remoteEndpoint = result.RemoteEndPoint;
            _logger.LogInformation($"Received message from {remoteEndpoint}");

            try
            {
                var message = System.Text.Encoding.ASCII.GetString(result.Buffer).Trim();

                if (int.TryParse(message, out var clientNumber))
                {
                    _logger.LogInformation($"Received number from client: {clientNumber}");

                    // Generate random number between 1-100 for this request
                    var randomNumber = _random.Next(1, 101);
                    _logger.LogInformation($"Generated random number: {randomNumber}");

                    var smallestNumber = Math.Min(clientNumber, randomNumber);
                    var responseBytes = System.Text.Encoding.ASCII.GetBytes(smallestNumber.ToString());

                    await _udpListener.SendAsync(responseBytes, responseBytes.Length, remoteEndpoint);
                    _logger.LogInformation($"Sent smallest number to client: {smallestNumber}");
                }
                else
                {
                    _logger.LogInformation("Received data from client is not a number");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling message from {remoteEndpoint}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _udpListener?.Close();
            _cts?.Cancel();
            await (_executingTask ?? Task.CompletedTask);
        }

        public void Dispose()
        {
            _udpListener?.Close();
            _cts?.Cancel();
        }
    }
}