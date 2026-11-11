using System.Data;
using Binova.Api.Dados;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Servicos;

public static class SequenciaService
{
    public static async Task<int> ProximoIdAsync(BinovaDbContext banco, string entidade)
    {
        var sequencia = entidade switch
        {
            "USUARIO" => "BINOVA_USUARIO_SEQ",
            "PONTO_COLETA" => "BINOVA_PONTO_SEQ",
            "LEITURA_SENSOR" => "BINOVA_LEITURA_SEQ",
            "OCORRENCIA" => "BINOVA_OCORRENCIA_SEQ",
            _ => throw new ArgumentException("Entidade invalida.")
        };

        var conexao = banco.Database.GetDbConnection();
        var fechar = conexao.State != ConnectionState.Open;
        if (fechar) await conexao.OpenAsync();
        try
        {
            using var comando = conexao.CreateCommand();
            comando.CommandText = $"SELECT {sequencia}.NEXTVAL FROM DUAL";
            var resultado = await comando.ExecuteScalarAsync();
            return Convert.ToInt32(resultado);
        }
        finally
        {
            if (fechar) await conexao.CloseAsync();
        }
    }
}
