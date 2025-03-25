using System.Net;
using System.Net.Sockets;

namespace TcpServer.Services
{
    public class TcpServerService : IHostedService, IDisposable
    {
        private readonly ILogger<TcpServerService> _logger;
        private TcpListener _tcpListener;
        private readonly int _port = 8080;
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

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        private async Task RunServerAsync(CancellationToken cancellationToken)
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();

            _logger.LogInformation($"Server is listening on port {_port}");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                    _ = HandleClientAsync(client, cancellationToken);
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

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Client connected");

            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream))
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var data = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(data)) break;

                        if (int.TryParse(data, out var numberFromClient))
                        {
                            _logger.LogInformation($"Received number from client: {numberFromClient}");

                            var smallestNumber = Math.Min(numberFromClient, _numberOnBackend);
                            await writer.WriteLineAsync(smallestNumber.ToString());
                            await writer.FlushAsync();

                            _logger.LogInformation($"Sent smallest number to client: {smallestNumber}");
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
                _logger.LogInformation("Client disconnected");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null) return;

            _cts.Cancel();
            _tcpListener.Stop();

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));
        }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _cts?.Cancel();
        }
    }
}
