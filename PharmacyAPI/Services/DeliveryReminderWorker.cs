namespace PharmacyAPI.Services
{
    public sealed class DeliveryReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeliveryReminderWorker> _logger;

        public DeliveryReminderWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DeliveryReminderWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var workflow = scope.ServiceProvider
                        .GetRequiredService<IOrderWorkflowService>();
                    await workflow.DispatchDueDeliveryPromptsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to dispatch delivery confirmation prompts.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
