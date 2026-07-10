#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$ApiBaseUrl = "https://api.nexumaltivon.com.br",
    [string]$SiteOrigin = "https://www.nexumaltivon.com.br",
    [int]$TimeoutSec = 30
)

$ErrorActionPreference = "Stop"

function Normalize-BaseUrl {
    param([string]$Value)
    $raw = if ($null -eq $Value) { "" } else { $Value }
    $normalized = $raw.Trim().TrimEnd("/")
    if ($normalized -notmatch "^https?://") {
        throw "URL invalida: '$Value'. Informe uma URL absoluta iniciando com http:// ou https://."
    }
    return $normalized
}

function Invoke-PublishedEndpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [int[]]$AllowedStatusCodes,
        [hashtable]$Headers = @{},
        [string]$Body = "",
        [string]$ContentType = "application/json"
    )

    try {
        $request = @{
            Method = $Method
            Uri = $Url
            Headers = $Headers
            TimeoutSec = $TimeoutSec
            UseBasicParsing = $true
        }

        if (-not [string]::IsNullOrWhiteSpace($Body)) {
            $request.Body = $Body
            $request.ContentType = $ContentType
        }

        $response = Invoke-WebRequest @request
        $statusCode = [int]$response.StatusCode
        $accepted = $AllowedStatusCodes -contains $statusCode
        [pscustomobject]@{
            Item = $Name
            Metodo = $Method
            Url = $Url
            Status = $statusCode
            Resultado = if ($accepted) { "OK" } else { "FALHA" }
            Detalhe = if ($accepted) { "" } else { "Status esperado: $($AllowedStatusCodes -join ', ')" }
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        if ($AllowedStatusCodes -contains $statusCode) {
            return [pscustomobject]@{
                Item = $Name
                Metodo = $Method
                Url = $Url
                Status = $statusCode
                Resultado = "OK"
                Detalhe = ""
            }
        }

        [pscustomobject]@{
            Item = $Name
            Metodo = $Method
            Url = $Url
            Status = $statusCode
            Resultado = "FALHA"
            Detalhe = $_.Exception.Message
        }
    }
}

$api = Normalize-BaseUrl $ApiBaseUrl
$origin = Normalize-BaseUrl $SiteOrigin
$lojaId = 0

try {
    $lojasResponse = Invoke-WebRequest -Method GET -Uri "$api/api/lojas" -TimeoutSec $TimeoutSec -UseBasicParsing
    $lojasPayload = $lojasResponse.Content | ConvertFrom-Json
    $firstLoja = @($lojasPayload.data)[0]
    if ($firstLoja -and $firstLoja.id) {
        $lojaId = [int]$firstLoja.id
    }
}
catch {
    $lojaId = 0
}

$checks = @(
    @{
        Name = "API health"
        Method = "GET"
        Url = "$api/health"
        Allowed = @(200)
    },
    @{
        Name = "Configuracao publica do site"
        Method = "GET"
        Url = "$api/api/site/configuracoes/publico"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Produtos publicos"
        Method = "GET"
        Url = "$api/api/produtos?limit=1"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Lojas"
        Method = "GET"
        Url = "$api/api/lojas"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "OpenAPI Swagger JSON"
        Method = "GET"
        Url = "$api/swagger/v1/swagger.json"
        Allowed = @(200)
    },
    @{
        Name = "Relatorios vendas"
        Method = "GET"
        Url = "$api/api/relatorios/vendas"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Financeiro faturamento"
        Method = "GET"
        Url = "$api/api/financeiro/faturamento"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Pedido por id protegido"
        Method = "GET"
        Url = "$api/api/pedidos/1"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Auth refresh"
        Method = "POST"
        Url = "$api/api/auth/refresh"
        Allowed = @(200, 400, 401, 403)
        Body = '{"token":"invalid","refreshToken":"invalid"}'
    },
    @{
        Name = "Financeiro contabil razao"
        Method = "GET"
        Url = "$api/api/financeiro/contabil/razao"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Financeiro contabil conciliacao"
        Method = "GET"
        Url = "$api/api/financeiro/contabil/conciliacao"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Financeiro contabil DRE"
        Method = "GET"
        Url = "$api/api/financeiro/contabil/dre"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "Financeiro contabil fechamento"
        Method = "GET"
        Url = "$api/api/financeiro/contabil/fechamento"
        Allowed = @(200, 401, 403)
    },
    @{
        Name = "CORS frontend publicado"
        Method = "OPTIONS"
        Url = "$api/api/produtos"
        Allowed = @(200, 204)
        Headers = @{
            Origin = $origin
            "Access-Control-Request-Method" = "GET"
            "Access-Control-Request-Headers" = "content-type,authorization"
        }
    }
)

if ($lojaId -gt 0) {
    $checks += @{
        Name = "Loja por id"
        Method = "GET"
        Url = "$api/api/lojas/$lojaId"
        Allowed = @(200, 401, 403)
    }
}
else {
    $checks += @{
        Name = "Loja por id"
        Method = "GET"
        Url = "$api/api/lojas/1"
        Allowed = @(200, 401, 403)
    }
}

$results = foreach ($check in $checks) {
    Invoke-PublishedEndpoint `
        -Name $check.Name `
        -Method $check.Method `
        -Url $check.Url `
        -AllowedStatusCodes $check.Allowed `
        -Headers $(if ($check.ContainsKey("Headers")) { $check.Headers } else { @{} }) `
        -Body $(if ($check.ContainsKey("Body")) { $check.Body } else { "" })
}

$results | Format-Table -AutoSize

$failed = @($results | Where-Object { $_.Resultado -ne "OK" })
if ($failed.Count -gt 0) {
    throw "Publicacao sem backend operacional: $($failed.Count) verificacao(oes) falharam. Corrija DNS/Cloudflare/API/banco/CORS e execute novamente."
}

Write-Host "Publicacao com backend respondendo nos pontos obrigatorios verificados." -ForegroundColor Green
