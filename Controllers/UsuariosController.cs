
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
    // Monta a resposta do usuário sem expor a senha
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

    // Verifica os papéis permitidos pelo banco de dados
    private static bool PapelValido(string papel)
    {
        return papel is "CIDADAO" or "ADMIN";
    }

    // Lista todos os usuários cadastrados
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await banco.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Id)
            .ToListAsync();

        return Ok(usuarios.Select(MontarResposta));
    }

    // Consulta um usuário pelo identificador.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Buscar(int id)
    {
        var usuario = await banco.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id);

        if (usuario is null)
        {
            return NotFound(new { erro = "Usuário não encontrado." });
        }

        return Ok(MontarResposta(usuario));
    }

    // Cadastra um usuário com senha protegida por hash
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] UsuarioEntrada entrada)
    {
        string nome = entrada.Nome.Trim();
        string email = entrada.Email.Trim().ToLowerInvariant();
        string papel = entrada.Papel.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) || !PapelValido(papel))
        {
            return BadRequest(new { erro = "Nome ou papel inválido." });
        }

        bool emailCadastrado = await banco.Usuarios
            .AnyAsync(usuario => usuario.Email == email);

        if (emailCadastrado)
        {
            return BadRequest(new { erro = "E-mail já cadastrado." });
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

    // Atualiza os dados de um usuário existente
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] UsuarioAtualizacao entrada)
    {
        var usuario = await banco.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound(new { erro = "Usuário não encontrado." });
        }

        string nome = entrada.Nome.Trim();
        string email = entrada.Email.Trim().ToLowerInvariant();
        string papel = entrada.Papel.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) || !PapelValido(papel))
        {
            return BadRequest(new { erro = "Nome ou papel inválido." });
        }

        bool emailUtilizado = await banco.Usuarios
            .AnyAsync(outro => outro.Email == email && outro.Id != id);

        if (emailUtilizado)
        {
            return BadRequest(new { erro = "E-mail já utilizado." });
        }

        usuario.Nome = nome;
        usuario.Email = email;
        usuario.Papel = papel;

        await banco.SaveChangesAsync();

        return NoContent();
    }

    // Exclui um usuário sem ocorrências vinculadas
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var usuario = await banco.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound(new { erro = "Usuário não encontrado." });
        }

        bool possuiOcorrencias = await banco.Ocorrencias
            .AnyAsync(ocorrencia => ocorrencia.UsuarioId == id);

        if (possuiOcorrencias)
        {
            return BadRequest(new
            {
                erro = "O usuário possui ocorrências vinculadas e não pode ser excluído."
            });
        }

        banco.Usuarios.Remove(usuario);
        await banco.SaveChangesAsync();

        return NoContent();
    }
}
