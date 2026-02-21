using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using AspNetCore.Clients;
using AspNetCore.Settings;
using OpenAI;
using OpenAI.Chat;

namespace AspNetCore;

public static class Bootstraps
{
    public static IServiceCollection RegisterInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<ILlmClient, OpenRouterClient>();
        services.AddOpenAiChatClient(configuration, env);

        return services;
    }

    private static IServiceCollection AddOpenAiChatClient(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.Configure<OpenRouterSettings>(configuration.GetSection(OpenRouterSettings.SectionName));

        var settings = configuration
                           .GetRequiredSection(OpenRouterSettings.SectionName)
                           .Get<OpenRouterSettings>()
                       ?? throw new InvalidOperationException($"Missing {OpenRouterSettings.SectionName} section.");

        const string clientName = "OpenRouter";

        // Register HttpClient with proxy
        var clientBuilder = services.AddHttpClient(clientName)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (env.IsDevelopment() && !string.IsNullOrWhiteSpace(settings.ProxyUrl?.ToString()))
                {
                    handler.Proxy = new WebProxy(settings.ProxyUrl);
                    handler.UseProxy = true;
                }
                else
                {
                    handler.UseProxy = false;
                }

                return handler;
            });

        if (env.IsDevelopment())
        {
            // Register the logging handler only in dev environment for debug purposes ONLY (!)
            services.AddTransient<LoggingHttpHandler>();
            clientBuilder.AddHttpMessageHandler<LoggingHttpHandler>();
        }

        // Register ChatClient
        services.AddSingleton<ChatClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(clientName);
            var transport = new HttpClientPipelineTransport(httpClient);

            var options = new OpenAIClientOptions
            {
                Transport = transport,
                Endpoint = settings.BaseUrl // Set custom endpoint (!)
            };

            return new ChatClient(
                model: settings.Model,
                credential: new ApiKeyCredential(settings.ApiKey),
                options: options);
        });

        return services;
    }
}