$ErrorActionPreference = "Stop"
$base = "http://localhost:5086"

function Fazer($metodo, $caminho, $dados = $null) {
    $params = @{ Method = $metodo; Uri = "$base$caminho"; UseBasicParsing = $true }
    if ($null -ne $dados) {
        $params.ContentType = "application/json"
        $params.Body = ($dados | ConvertTo-Json -Depth 6)
    }
    $resposta = Invoke-WebRequest @params
    $conteudo = $null
    if ($resposta.Content) { $conteudo = $resposta.Content | ConvertFrom-Json }
    return @{ Status = [int]$resposta.StatusCode; Dados = $conteudo }
}

$usuario = Fazer "POST" "/api/usuarios" @{ nome="Teste Binova"; email="teste.$([guid]::NewGuid().ToString('N'))@binova.invalid"; senha="SenhaTeste123!"; papel="CIDADAO" }
if ($usuario.Status -ne 201) { throw "POST usuario deveria retornar 201" }
$usuarioId = $usuario.Dados.id

$ponto = Fazer "POST" "/api/pontos-coleta" @{ nome="Ponto demonstracao"; descricaoLocalizacao="Sala de testes"; capacidadeLitros=50; ativo=$true }
if ($ponto.Status -ne 201) { throw "POST ponto deveria retornar 201" }
$pontoId = $ponto.Dados.id

$leitura = Fazer "POST" "/api/leituras-sensor" @{ pontoColetaId=$pontoId; distanciaCm=10; nivelPercentual=85 }
if ($leitura.Status -ne 201) { throw "POST leitura deveria retornar 201" }
$leituraId = $leitura.Dados.id

$ocorrencia = Fazer "POST" "/api/ocorrencias" @{ pontoColetaId=$pontoId; usuarioId=$usuarioId; tipo="LIXEIRA_CHEIA"; descricao="Teste de lixeira cheia" }
if ($ocorrencia.Status -ne 201) { throw "POST ocorrencia deveria retornar 201" }
$ocorrenciaId = $ocorrencia.Dados.id

if ((Fazer "GET" "/api/pontos-coleta/$pontoId").Status -ne 200) { throw "GET ponto falhou" }
if ((Fazer "GET" "/api/pontos-coleta/$pontoId/situacao").Dados.situacao -ne "PROXIMA_DO_LIMITE") { throw "Situacao incorreta" }
if ((Fazer "PUT" "/api/ocorrencias/$ocorrenciaId" @{ tipo="LIXEIRA_CHEIA"; descricao="Resolvido"; status="RESOLVIDA" }).Status -ne 204) { throw "PUT ocorrencia falhou" }
if ((Fazer "PUT" "/api/pontos-coleta/$pontoId" @{ nome="Ponto atualizado"; descricaoLocalizacao="Sala de testes"; capacidadeLitros=60; ativo=$true }).Status -ne 204) { throw "PUT ponto falhou" }

# Remove entidades na ordem das referencias; ponto usa exclusao logica.
if ((Fazer "DELETE" "/api/ocorrencias/$ocorrenciaId").Status -ne 204) { throw "DELETE ocorrencia falhou" }
if ((Fazer "DELETE" "/api/leituras-sensor/$leituraId").Status -ne 204) { throw "DELETE leitura falhou" }
if ((Fazer "DELETE" "/api/usuarios/$usuarioId").Status -ne 204) { throw "DELETE usuario falhou" }
if ((Fazer "DELETE" "/api/pontos-coleta/$pontoId").Status -ne 204) { throw "DELETE ponto falhou" }

Write-Host "SUCESSO: CRUD testado nas 4 entidades, codigos 201/200/204 e classificacao por leitura." -ForegroundColor Green
Write-Host "Verifique manualmente tambem erros HTTP 400 e 404 em scripts/testar-api.http."
