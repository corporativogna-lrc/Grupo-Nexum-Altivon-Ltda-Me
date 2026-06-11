using NexumAltivon.API.Models;

namespace NexumAltivon.API.ERP.FiscalRouting;

public interface IFiscalRoutingEngine
{
    FiscalRoutingDecision Evaluate(FiscalRoutingRequest request, IReadOnlyCollection<FiscalCompanySnapshot> empresas);
    FiscalCompanySnapshot ToSnapshot(EmpresaGrupo empresa);
}

public sealed class FiscalRoutingEngine : IFiscalRoutingEngine
{
    public FiscalRoutingDecision Evaluate(FiscalRoutingRequest request, IReadOnlyCollection<FiscalCompanySnapshot> empresas)
    {
        var elegiveis = empresas
            .Where(empresa => empresa.Ativa)
            .Where(empresa => !request.RequerSaidaNfe || empresa.PermiteNfeSaida)
            .Where(empresa => !request.RequerEntradaNfe || empresa.PermiteNfeEntrada)
            .Where(empresa => !request.ExigeDropshipping || empresa.PermiteDropshipping)
            .Where(empresa => !request.ExigeMarketplace || empresa.PermiteMarketplace)
            .ToList();

        if (elegiveis.Count == 0)
        {
            return new FiscalRoutingDecision(
                null,
                [],
                false,
                "Nenhuma empresa ativa atende os requisitos mínimos da operação fiscal.");
        }

        var ranking = elegiveis
            .Select(empresa => AvaliarEmpresa(request, empresa))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.CustoTributarioEstimado)
            .ThenBy(item => item.CustoOperacionalEstimado)
            .ToList();

        var selecionada = ranking.First();

        return new FiscalRoutingDecision(
            selecionada.Empresa,
            ranking,
            true,
            $"Emitente sugerida: {selecionada.Empresa.RazaoSocial} ({selecionada.Empresa.CodigoEmpresa}) com score {selecionada.Score:F2}, margem estimada de {selecionada.MargemEstimadaPercentual:F2}% e custo tributário estimado de R$ {selecionada.CustoTributarioEstimado:F2}.");
    }

    public FiscalCompanySnapshot ToSnapshot(EmpresaGrupo empresa) =>
        new(
            empresa.Id,
            empresa.CodigoEmpresa ?? $"EMP-{empresa.Id}",
            empresa.RazaoSocial,
            empresa.Cnpj,
            empresa.Estado ?? string.Empty,
            empresa.RegimeTributario,
            empresa.CategoriaFiscal,
            empresa.SubcategoriaFiscal,
            empresa.AliquotaIcmsInterna ?? 0m,
            empresa.AliquotaIcmsInterestadual ?? 0m,
            empresa.AliquotaPis ?? 0m,
            empresa.AliquotaCofins ?? 0m,
            empresa.AliquotaIss ?? 0m,
            empresa.AliquotaIpi ?? 0m,
            empresa.CargaTributariaPercentual ?? 0m,
            empresa.CustoOperacionalPercentual ?? 0m,
            empresa.MargemMinimaPercentual ?? 0m,
            empresa.PrioridadeFiscal,
            empresa.PermiteNfeEntrada,
            empresa.PermiteNfeSaida,
            empresa.PermiteDropshipping,
            empresa.PermiteMarketplace,
            empresa.EmitentePreferencial,
            empresa.Ativa);

    private static FiscalRoutingCandidate AvaliarEmpresa(FiscalRoutingRequest request, FiscalCompanySnapshot empresa)
    {
        var justificativas = new List<string>();
        var receitaBruta = request.ValorProdutos + request.ValorFrete;

        var mesmaUf = string.Equals(request.EstadoOrigem, request.EstadoDestino, StringComparison.OrdinalIgnoreCase);
        var icms = mesmaUf ? empresa.AliquotaIcmsInterna : empresa.AliquotaIcmsInterestadual;
        var tributosPercentuais = icms + empresa.AliquotaPis + empresa.AliquotaCofins + empresa.AliquotaIss + empresa.AliquotaIpi;

        if (empresa.CargaTributariaPercentual > 0)
        {
            tributosPercentuais = Math.Max(tributosPercentuais, empresa.CargaTributariaPercentual);
            justificativas.Add("Carga tributária histórica considerada no cálculo.");
        }

        var custoTributario = receitaBruta * (tributosPercentuais / 100m);
        var custoOperacional = receitaBruta * (empresa.CustoOperacionalPercentual / 100m);
        var lucroEstimado = receitaBruta - custoTributario - custoOperacional;
        var margem = receitaBruta == 0 ? 0 : lucroEstimado / receitaBruta * 100m;

        var score = 1000m;
        score -= custoTributario;
        score -= custoOperacional * 0.60m;
        score += Math.Max(0, margem) * 12m;
        score += empresa.EmitentePreferencial ? 35m : 0m;
        score += Math.Max(0, 200 - empresa.PrioridadeFiscal);

        if (!string.IsNullOrWhiteSpace(request.CategoriaFiscal) &&
            string.Equals(request.CategoriaFiscal, empresa.CategoriaFiscal, StringComparison.OrdinalIgnoreCase))
        {
            score += 18m;
            justificativas.Add("Categoria fiscal compatível com a operação.");
        }

        if (!string.IsNullOrWhiteSpace(request.SubcategoriaFiscal) &&
            string.Equals(request.SubcategoriaFiscal, empresa.SubcategoriaFiscal, StringComparison.OrdinalIgnoreCase))
        {
            score += 10m;
            justificativas.Add("Subcategoria fiscal alinhada.");
        }

        if (margem < empresa.MargemMinimaPercentual)
        {
            score -= 120m;
            justificativas.Add("Margem estimada abaixo do mínimo definido.");
        }
        else
        {
            justificativas.Add("Margem estimada dentro da meta.");
        }

        if (mesmaUf)
        {
            justificativas.Add("Operação interna: ICMS interno aplicado.");
        }
        else
        {
            justificativas.Add("Operação interestadual: ICMS interestadual aplicado.");
        }

        if (request.ExigeMarketplace && empresa.PermiteMarketplace)
        {
            justificativas.Add("Empresa habilitada para marketplace.");
        }

        if (request.ExigeDropshipping && empresa.PermiteDropshipping)
        {
            justificativas.Add("Empresa habilitada para dropshipping.");
        }

        return new FiscalRoutingCandidate(
            empresa,
            decimal.Round(receitaBruta, 2),
            decimal.Round(custoTributario, 2),
            decimal.Round(custoOperacional, 2),
            decimal.Round(lucroEstimado, 2),
            decimal.Round(margem, 2),
            decimal.Round(score, 2),
            justificativas);
    }
}
