using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Leave.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Starting request {RequestName}", requestName);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                stopwatch.Stop();
                _logger.LogInformation("Completed request {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestName} failed", requestName);
                throw;
            }
        }
    }
}
