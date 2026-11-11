
$ErrorActionPreference = "Stop"
$urlBase = "http://localhost:5086"

# Executa requisições HTTP e retorna o status e os dados
function Fazer($metodo, $caminho, $dados = $null) {
    $parametros = @{
        Method = $metodo
        Uri = "$urlBase$caminho"
        UseBasicParsing = $true
    }

    if ($null -ne $dados) {
        $parametros.ContentType = "application/json"
        $parametros.Body = $dados | ConvertTo-Json -Depth 6
    }

    try {
        $resposta = Invoke-WebRequest @parametros
        $conteudo = $null

        if ($resposta.Content) {
            $conteudo = $resposta.Content | ConvertFrom-Json
        }

        return @{
            Status = [int]$resposta.StatusCode
            Dados = $conteudo
        }
    }
    catch {
        $respostaErro = $_.Exception.Response

        if ($null -eq $respostaErro) {
            throw
        }

        return @{
            Status = [int]$respostaErro.StatusCode
            Dados = $null
        }
    }
}

# Confere se a resposta possui o código HTTP esperado
function VerificarStatus($resposta, $esperado, $operacao) {
    if ($resposta.Status -ne $esperado) {
        throw "$($operacao): esperado HTTP $esperado, recebido HTTP $($resposta.Status)."
    }
}

$usuarioId = $null
$pontoId = $null
$leituraId = $null
$ocorrenciaId = $null

$testesConcluidos = $false
$limpezaPendente = $false

try {
    # Cadastra um usuário para os testes
    $emailTeste = "teste.$([guid]::NewGuid().ToString('N'))@binova.invalid"

    $usuario = Fazer "POST" "/api/usuarios" @{
        nome = "Teste Binova"
        email = $emailTeste
        senha = "SenhaTeste123!"
        papel = "CIDADAO"
    }

    VerificarStatus $usuario 201 "Cadastrar usuário"
    $usuarioId = $usuario.Dados.id

    if (-not $usuarioId) {
        throw "O cadastro do usuário não retornou um ID."
    }

    # Cadastra um ponto de coleta
    $ponto = Fazer "POST" "/api/pontos-coleta" @{
        nome = "Ponto demonstracao"
        descricaoLocalizacao = "Sala de testes"
        capacidadeLitros = 50
        ativo = $true
    }

    VerificarStatus $ponto 201 "Cadastrar ponto"
    $pontoId = $ponto.Dados.id

    if (-not $pontoId) {
        throw "O cadastro do ponto não retornou um ID."
    }

    # Registra uma leitura do sensor
    $leitura = Fazer "POST" "/api/leituras-sensor" @{
        pontoColetaId = $pontoId
        distanciaCm = 10
        nivelPercentual = 85
    }

    VerificarStatus $leitura 201 "Cadastrar leitura"
    $leituraId = $leitura.Dados.id

    if (-not $leituraId) {
        throw "O cadastro da leitura não retornou um ID."
    }

    # Registra uma ocorrência vinculada ao usuário e ao ponto
    $ocorrencia = Fazer "POST" "/api/ocorrencias" @{
        pontoColetaId = $pontoId
        usuarioId = $usuarioId
        tipo = "LIXEIRA_CHEIA"
        descricao = "Teste de lixeira cheia"
    }

    VerificarStatus $ocorrencia 201 "Cadastrar ocorrência"
    $ocorrenciaId = $ocorrencia.Dados.id

    if (-not $ocorrenciaId) {
        throw "O cadastro da ocorrência não retornou um ID."
    }

    # Consulta os quatro registros cadastrados
    VerificarStatus (Fazer "GET" "/api/usuarios/$usuarioId") 200 "Consultar usuário"
    VerificarStatus (Fazer "GET" "/api/pontos-coleta/$pontoId") 200 "Consultar ponto"
    VerificarStatus (Fazer "GET" "/api/leituras-sensor/$leituraId") 200 "Consultar leitura"
    VerificarStatus (Fazer "GET" "/api/ocorrencias/$ocorrenciaId") 200 "Consultar ocorrência"

    # Valida a classificação do nível de preenchimento
    $situacao = Fazer "GET" "/api/pontos-coleta/$pontoId/situacao"

    VerificarStatus $situacao 200 "Consultar situação"

    if ($situacao.Dados.situacao -ne "PROXIMA_DO_LIMITE") {
        throw "A classificação do ponto está incorreta."
    }

    # Atualiza os dados do usuário
    $atualizarUsuario = Fazer "PUT" "/api/usuarios/$usuarioId" @{
        nome = "Teste Binova Atualizado"
        email = $emailTeste
        papel = "CIDADAO"
    }

    VerificarStatus $atualizarUsuario 204 "Atualizar usuário"

    # Atualiza o ponto de coleta
    $atualizarPonto = Fazer "PUT" "/api/pontos-coleta/$pontoId" @{
        nome = "Ponto atualizado"
        descricaoLocalizacao = "Sala de testes"
        capacidadeLitros = 60
        ativo = $true
    }

    VerificarStatus $atualizarPonto 204 "Atualizar ponto"

    # Atualiza a medição do sensor
    $atualizarLeitura = Fazer "PUT" "/api/leituras-sensor/$leituraId" @{
        pontoColetaId = $pontoId
        distanciaCm = 25
        nivelPercentual = 45
    }

    VerificarStatus $atualizarLeitura 204 "Atualizar leitura"

    # Atualiza o atendimento da ocorrência
    $atualizarOcorrencia = Fazer "PUT" "/api/ocorrencias/$ocorrenciaId" @{
        tipo = "LIXEIRA_CHEIA"
        descricao = "Ocorrência resolvida durante o teste"
        status = "RESOLVIDA"
    }

    VerificarStatus $atualizarOcorrencia 204 "Atualizar ocorrência"

    # Confirma a classificação após a atualização da leitura
    $situacaoAtualizada = Fazer "GET" "/api/pontos-coleta/$pontoId/situacao"

    VerificarStatus $situacaoAtualizada 200 "Consultar situação atualizada"

    if ($situacaoAtualizada.Dados.situacao -ne "DISPONIVEL") {
        throw "A classificação não foi atualizada corretamente."
    }

    # Verifica o retorno para um identificador inexistente
    $naoEncontrado = Fazer "GET" "/api/usuarios/2147483647"

    VerificarStatus $naoEncontrado 404 "Validar HTTP 404"

    # Verifica o retorno para dados inválidos
    $dadosInvalidos = Fazer "POST" "/api/pontos-coleta" @{
        nome = ""
        descricaoLocalizacao = ""
        capacidadeLitros = -10
        ativo = $true
    }

    VerificarStatus $dadosInvalidos 400 "Validar HTTP 400"

    $testesConcluidos = $true
}
finally {
    # Remove os registros de teste respeitando os relacionamentos
    $exclusoes = @(
        @{ Nome = "Ocorrência"; Id = $ocorrenciaId; Rota = "/api/ocorrencias" }
        @{ Nome = "Leitura"; Id = $leituraId; Rota = "/api/leituras-sensor" }
        @{ Nome = "Usuário"; Id = $usuarioId; Rota = "/api/usuarios" }
        @{ Nome = "Ponto"; Id = $pontoId; Rota = "/api/pontos-coleta" }
    )

    foreach ($item in $exclusoes) {
        if ($null -eq $item.Id) {
            continue
        }

        try {
            $resultado = Fazer "DELETE" "$($item.Rota)/$($item.Id)"

            if ($resultado.Status -ne 204) {
                $limpezaPendente = $true
                Write-Warning "Falha ao excluir $($item.Nome): HTTP $($resultado.Status)."
            }
        }
        catch {
            $limpezaPendente = $true
            Write-Warning "Erro ao excluir $($item.Nome): $_"
        }
    }
}

if ($testesConcluidos) {
    Write-Host "SUCESSO: CRUD validado nas quatro entidades." -ForegroundColor Green
    Write-Host "Códigos HTTP 200, 201, 204, 400 e 404 verificados." -ForegroundColor Green
    Write-Host "Classificação das leituras validada." -ForegroundColor Green

    if ($limpezaPendente) {
        Write-Warning "Alguns registros de teste podem precisar de limpeza manual."
    }
    else {
        Write-Host "Limpeza das entidades de teste concluída." -ForegroundColor Green
        Write-Host "Observação: o ponto permanece inativo no Oracle." -ForegroundColor Yellow
    }
}
