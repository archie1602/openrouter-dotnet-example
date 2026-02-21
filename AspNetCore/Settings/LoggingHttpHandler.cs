using System.Diagnostics;

namespace AspNetCore.Settings;

public class LoggingHttpHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHttpHandler> _logger;

    public LoggingHttpHandler(ILogger<LoggingHttpHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequestAsync(requestId, request);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            // Log response
            await LogResponseAsync(requestId, response, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[{RequestId}] Request failed after {ElapsedMs}ms",
                requestId,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task LogRequestAsync(string requestId, HttpRequestMessage request)
    {
        const LogLevel logLevel = LogLevel.Information;

        _logger.Log(logLevel,
            "{Method} {Uri}",
            request.Method,
            request.RequestUri);

        // Log headers (excluding sensitive ones)
        foreach (var header in request.Headers)
        {
            var value = header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                ? "[REDACTED]"
                : string.Join(", ", header.Value);

            _logger.Log(logLevel,
                "{Header}: {Value}", header.Key, value);
        }

        // Log body
        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync();

            _logger.Log(logLevel,
                "[{RequestId}] <-- Body: {@Body}",
                requestId,
                body);
        }
    }

    private async Task LogResponseAsync(
        string requestId,
        HttpResponseMessage response,
        long elapsedMs)
    {
        var logLevel = response.IsSuccessStatusCode ? LogLevel.Debug : LogLevel.Warning;

        _logger.Log(logLevel,
            "[{RequestId}] <-- {StatusCode} {ReasonPhrase} ({ElapsedMs}ms)",
            requestId,
            (int)response.StatusCode,
            response.ReasonPhrase,
            elapsedMs);

        // Log response body
        var body = await response.Content.ReadAsStringAsync();

        var truncatedBody = body.Length > 10000
            ? body[..10000] + $"... [truncated, total {body.Length} chars]"
            : body;

        _logger.Log(logLevel,
            "[{RequestId}] <-- Body: {@Body}",
            requestId, truncatedBody);
    }
}