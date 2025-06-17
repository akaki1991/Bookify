using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Bookify.Application.Abstractions.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = request.GetType().Name;

        try
        {
            logger.LogInformation("Executing request {Request}", name);

            var result = await next(cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Request {Request} processed successfully", name);
            }
            else
            {
                //// @ sign in front of Error argumnet serializes error as json.
                //logger.LogInformation("Request {Request} processed with {@Error}", name, result.Error);

                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogInformation("Request {Request} processed with error", name);
                }
            }

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {Request} processing failed", name);

            throw;
        }
    }
}