using OpenAI.Chat;

namespace AspNetCore.Clients;

public class OpenRouterClient : ILlmClient
{
    private readonly ChatClient _chatClient;


    // ReSharper disable once InconsistentNaming
    private const string SYSTEM_PROMPT = """
                                         You are an expert math professor with deep knowledge across all areas of mathematics.
                                         The user will provide a math problem.
                                         Your task is to solve it step by step, showing all work clearly.

                                         Guidelines:
                                         - Break down the solution into clear, numbered steps
                                         - State the final answer explicitly at the end
                                         """;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OpenRouterClient(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string?> Solve(string problemToSolve, CancellationToken ct = default)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(SYSTEM_PROMPT),
            new UserChatMessage(problemToSolve)
        ];

        ChatCompletionOptions options = new()
        {
            Temperature = 0,
            TopP = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
            MaxOutputTokenCount = 5000,
            ResponseFormat = ChatResponseFormat.CreateTextFormat()
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, ct);

        var jsonResponse = completion.Content[0].Text;

        return jsonResponse;
    }
}