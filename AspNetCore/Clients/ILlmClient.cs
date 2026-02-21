namespace AspNetCore.Clients;

public interface ILlmClient
{
    Task<string?> Solve(string problemToSolve, CancellationToken ct = default);
}