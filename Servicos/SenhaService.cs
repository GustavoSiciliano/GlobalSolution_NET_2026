
using System;
using System.Security.Cryptography;

namespace Binova.Api.Servicos;

public static class SenhaService
{
    private const int TamanhoSal = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    // Gera um hash seguro para armazenar a senha do usuário
    public static string CriarHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException(
                "A senha não pode estar vazia.",
                nameof(senha)
            );
        }

        byte[] sal = RandomNumberGenerator.GetBytes(TamanhoSal);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            senha,
            sal,
            Iteracoes,
            HashAlgorithmName.SHA256,
            TamanhoHash
        );

        return $"PBKDF2${Iteracoes}$" +
               $"{Convert.ToBase64String(sal)}$" +
               $"{Convert.ToBase64String(hash)}";
    }
}
