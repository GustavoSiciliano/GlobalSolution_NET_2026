
using Binova.Api.Contratos;
using Binova.Api.Dados;
using Binova.Api.Modelos;
using Binova.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Binova.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(BinovaDbContext banco) : ControllerBase
{
    // Monta a resposta do usuÃ¡rio sem expor a senha
    private static object MontarResposta(Usuario usuario)
    {
        return new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Papel,
            usuario.CriadoEm
        };
    }

    // Verifica os papÃ©is permitidos pelo banco de dados
    private static bool PapelValido(string papel)
    {
        return papel is "CIDADAO" or "ADMIN";
    }

    // Lista todos os usuÃ¡rios cadastrados
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await banco.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Id)
            .ToListAsync();

        return Ok(usuarios.Select(MontarResposta));
    }

    // Consulta um usuÃ¡rio pelo identificador.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var usuario = await banco.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id);

        if (usuario is null)
        {
            return NotFound(new { erro = "UsuÃ¡rio nÃ£o encontrado." });
        }

        return Ok(MontarResposta(usuario));
    }

    // Cadastra um usuÃ¡rio com senha protegida por hash
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] UsuarioEntrada entrada)
    {
        string nome = entrada.Nome.Trim();
        string email = entrada.Email.Trim().ToLowerInvariant();
        string papel = entrada.Papel.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) || !PapelValido(papel))
        {
            return BadRequest(new { erro = "Nome ou papel invÃ¡lido." });
        }

        int quantidadeEmails = await banco.Usuarios
            .CountAsync(usuario => usuario.Email == email);

        if (quantidadeEmails > 0)
        {
            return BadRequest(new { erro = "E-mail jÃ¡ cadastrado." });
        }

        var usuario = new Usuario
        {
            Id = await SequenciaService.ProximoIdAsync(banco, "USUARIO"),
            Nome = nome,
            Email = email,
            Papel = papel,
            SenhaHash = SenhaService.CriarHash(entrada.Senha)
        };

        banco.Usuarios.Add(usuario);
        await banco.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Buscar),
            new { id = usuario.Id },
            MontarResposta(usuario)
        );
    }

    // Atualiza os dados de um usuÃ¡rio existente
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] UsuarioAtualizacao entrada)
    {
        var usuario = await banco.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound(new { erro = "UsuÃ¡rio nÃ£o encontrado." });
        }

        string nome = entrada.Nome.Trim();
        string email = entrada.Email.Trim().ToLowerInvariant();
        string papel = entrada.Papel.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) || !PapelValido(papel))
        {
            return BadRequest(new { erro = "Nome ou papel invÃ¡lido." });
        }

        int quantidadeEmails = await banco.Usuarios
            .CountAsync(outro => outro.Email == email && outro.Id != id);

        if (quantidadeEmails > 0)
        {
            return BadRequest(new { erro = "E-mail jÃ¡ utilizado." });
        }

        usuario.Nome = nome;
        usuario.Email = email;
        usuario.Papel = papel;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Exclui um usuÃ¡rio sem ocorrÃªncias vinculadas
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var usuario = await banco.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound(new { erro = "UsuÃ¡rio nÃ£o encontrado." });
        }

        int quantidadeOcorrencias = await banco.Ocorrencias
            .CountAsync(ocorrencia => ocorrencia.UsuarioId == id);

        if (quantidadeOcorrencias > 0)
        {
            return BadRequest(new
            {
                erro = "O usuÃ¡rio possui ocorrÃªncias vinculadas e nÃ£o pode ser excluÃ­do."
            });
        }

        banco.Usuarios.Remove(usuario);
        await banco.SaveChangesAsync();

        return NoContent();
    }
}

