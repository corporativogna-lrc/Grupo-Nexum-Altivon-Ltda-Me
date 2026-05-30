# FASE 4 — Integrações Completas

## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/MarketplaceDtos.cs` | DTOs Mercado Livre, Shopee, Amazon, Hub unificado |
| 2 | `DTOs/DropshippingDtos.cs` | DTOs de roteamento, comissão, fornecedores |
| 3 | `DTOs/LogisticaDtos.cs` | DTOs de etiquetas, rastreamento, dashboard |
| 4 | `DTOs/ErpSyncDtos.cs` | DTOs de sincronização GenesisGest.Net |
| 5 | `Services/MercadoLivreService.cs` | Publicar, atualizar, importar pedidos ML |
| 6 | `Services/MarketplaceHubService.cs` | Hub multi-canal: Shopee, Amazon, sync automático |
| 7 | `Services/DropshippingService.cs` | Roteamento inteligente, comissões, notificações |
| 8 | `Services/LogisticaService.cs` | Etiquetas, rastreamento, status de entrega |
| 9 | `Services/ErpSyncService.cs` | Bridge GenesisGest.Net (produtos, clientes, pedidos, estoque) |
| 10 | `Services/MarketplaceSyncService.cs` | Orquestrador de sync automático e logs |
| 11 | `Controllers/IntegracoesController.cs` | Hub unificado REST (marketplaces, dropshipping, logística, ERP) |
| 12 | `Models/IntegracoesModels.cs` | Entidades: MarketplaceProduto, DropshippingPedido, Fornecedor, Transportadora, Etiqueta, SyncLog |
| 13 | `Configurations/IntegrationExtensions.cs` | Registro de DI das integrações |
| 14 | `README_Fase4.md` | Este documento |

### Endpoints da API de Integrações

#### Marketplaces
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/marketplaces/sync` | POST | Admin/Gerente | Sincroniza produto em canais |
| `/api/integracoes/marketplaces/sync-lote` | POST | Admin/Gerente | Sincroniza lote de produtos |
| `/api/integracoes/marketplaces/relatorio` | GET | Admin/Gerente | Relatório de sync por período |
| `/api/integracoes/marketplaces/status/{id}` | GET | Admin/Gerente | Status de sync de produto |
| `/api/integracoes/mercadolivre/publicar/{id}` | POST | Admin/Gerente | Publica produto no ML |
| `/api/integracoes/mercadolivre/importar-pedidos` | POST | Admin/Gerente | Importa pedidos pendentes do ML |
| `/api/integracoes/mercadolivre/marcar-enviado/{id}` | POST | Admin/Gerente | Marca pedido ML como enviado |

#### Dropshipping
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/dropshipping/roteiar` | POST | Admin/Gerente | Roteia pedido para fornecedor |
| `/api/integracoes/dropshipping/pendentes` | GET | Admin/Gerente | Lista pedidos pendentes |
| `/api/integracoes/dropshipping/{id}/status` | PUT | Admin/Gerente | Atualiza status/envio |
| `/api/integracoes/dropshipping/fornecedores` | GET | Admin/Gerente | Lista fornecedores |
| `/api/integracoes/dropshipping/comissao/{id}` | GET | Admin/Gerente | Relatório de comissão |

#### Logística
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/logistica/etiqueta` | POST | Admin/Gerente | Gera etiqueta de envio |
| `/api/integracoes/logistica/rastrear/{codigo}` | GET | Público | Rastreia envio |
| `/api/integracoes/logistica/status-envio` | PUT | Admin/Gerente | Atualiza status de entrega |
| `/api/integracoes/logistica/dashboard` | GET | Admin/Gerente | Dashboard operacional |
| `/api/integracoes/logistica/transportadoras` | GET | Admin/Gerente | Lista transportadoras |

#### ERP GenesisGest.Net
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/erp/sync` | POST | Admin | Sincroniza produtos/clientes/pedidos/estoque |
| `/api/integracoes/erp/status` | GET | Admin | Testa conexão com ERP |
| `/api/integracoes/erp/configuracao` | GET/PUT | Admin | Gerencia configuração |

#### Sync Automático
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/sync/executar-agendado` | POST | Admin | Executa sync manual agendado |
| `/api/integracoes/sync/logs` | GET | Admin | Logs de sincronização |

### Configurações `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoLivre": {
      "AccessToken": "APP_USR-...",
      "SellerId": "123456789"
    },
    "Shopee": {
      "BaseUrl": "https://partner.shopeemobile.com",
      "PartnerId": "123456",
      "ShopId": "789012"
    },
    "MelhorEnvio": {
      "Ativo": true,
      "Token": "...",
      "CepOrigem": "01001000"
    },
    "GenesisGest": {
      "UrlBase": "http://192.168.1.72:8080",
      "TokenApi": "...",
      "AutoSync": true,
      "IntervaloMinutos": 60,
      "Entidades": ["PRODUTOS", "CLIENTES", "PEDIDOS", "ESTOQUE"]
    }
  }
}
```

### Próximos Passos
- Configurar tokens reais de cada marketplace
- Implementar tokenização PCI-compliant para cartões (MercadoPago.js)
- Ativar webhook de confirmação de envio do Melhor Envio
- Configurar job recorrente (Hangfire/Quartz) para sync automático ERP
