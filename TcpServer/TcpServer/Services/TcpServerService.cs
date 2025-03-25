using System.Net;
using System.Net.Sockets;

namespace TcpServer.Services
{
    public class TcpServerService : IHostedService, IDisposable
    {
        private readonly ILogger<TcpServerService> _logger;
        private TcpListener _tcpListener;
        private readonly int _numberOnBackend = 100;
        private Task _executingTask;
        private CancellationTokenSource _cts;

        public TcpServerService(ILogger<TcpServerService> logger)
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
            _tcpListener = new TcpListener(IPAddress.Any, 8080);
            _tcpListener.Start();

            _logger.LogInformation($"Server is listening on port 8080");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException)
            {
                // Server is stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TCP server");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
            _logger.LogInformation($"Client connected from {remoteEndpoint}");

            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];

                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer);
                        if (bytesRead == 0) break;

                        var data = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                        _logger.LogInformation($"Received: {data}");

                        if (int.TryParse(data, out var numberFromClient))
                        {
                            var response = Math.Min(numberFromClient, _numberOnBackend).ToString();
                            var responseBytes = System.Text.Encoding.ASCII.GetBytes(response);

                            await stream.WriteAsync(responseBytes);
                            _logger.LogInformation($"Sent: {response}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client");
            }
            finally
            {
                _logger.LogInformation($"Client disconnected: {remoteEndpoint}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _tcpListener?.Stop();
            _cts?.Cancel();
            await (_executingTask ?? Task.CompletedTask);
        }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _cts?.Cancel();
        }
    }
}