
using System;
using System.Data;
using System.Threading.Tasks;
using Binova.Api.Dados;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Servicos;

public static class SequenciaService
{
    // Obtém o próximo identificador da sequência Oracle correspondente.
    public static async Task<int> ProximoIdAsync(
        BinovaDbContext banco,
        string entidade)
    {
        string sequencia = entidade switch
        {
            "USUARIO" => "BINOVA_USUARIO_SEQ",
            "PONTO_COLETA" => "BINOVA_PONTO_SEQ",
            "LEITURA_SENSOR" => "BINOVA_LEITURA_SEQ",
            "OCORRENCIA" => "BINOVA_OCORRENCIA_SEQ",

            _ => throw new ArgumentException(
                "A entidade informada é inválida.",
                nameof(entidade))
        };

        var conexao = banco.Database.GetDbConnection();

        bool precisaAbrir = conexao.State != ConnectionState.Open;

        if (precisaAbrir)
        {
            await conexao.OpenAsync();
        }

        try
        {
            using var comando = conexao.CreateCommand();

            comando.CommandText =
                $"SELECT {sequencia}.NEXTVAL FROM DUAL";

            var resultado = await comando.ExecuteScalarAsync();

            if (resultado is null or DBNull)
            {
                throw new InvalidOperationException(
                    "Não foi possível obter o próximo identificador."
                );
            }

            return Convert.ToInt32(resultado);
        }
        finally
        {
            if (precisaAbrir)
            {
                await conexao.CloseAsync();
            }
        }
    }
}
