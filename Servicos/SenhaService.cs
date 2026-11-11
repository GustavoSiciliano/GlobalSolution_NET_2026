using System.Security.Cryptography;

namespace Binova.Api.Servicos;

public static class SenhaService
{
    public static string CriarHash(string senha)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 100_000,
            HashAlgorithmName.SHA256, 32);
        return $"PBKDF2$100000${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }
}
