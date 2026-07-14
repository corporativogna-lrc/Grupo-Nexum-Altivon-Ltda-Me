# FASE 3 — Carrinho, Checkout e Gateway de Pagamento

## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/CarrinhoDtos.cs` | DTOs do carrinho e itens |
| 2 | `DTOs/CheckoutDtos.cs` | DTOs de checkout, endereço, frete e finalização |
| 3 | `DTOs/PagamentoDtos.cs` | DTOs de pagamento, webhook e reembolso |
| 4 | `Services/CarrinhoService.cs` | Carrinho com sessão + cliente, cupom, migração |
| 5 | `Services/CheckoutService.cs` | Fluxo completo de checkout |
| 6 | `Services/MercadoPagoService.cs` | Gateway PIX, Cartão e Boleto + webhooks |
| 7 | `Services/FreteService.cs` | Cálculo de frete (Melhor Envio + tabela própria) |
| 8 | `Services/NotificacaoService.cs` | E-mail (SendGrid) + WhatsApp |
| 9 | `Services/PedidoService.cs` | Geração de pedido e número NXYYMMDDXNNN |
| 10 | `Controllers/CarrinhoController.cs` | API pública de carrinho (anônima) |
| 11 | `Controllers/CheckoutController.cs` | API protegida de checkout |
| 12 | `Controllers/PagamentoController.cs` | Gestão de pagamentos e reembolsos |
| 13 | `Controllers/WebhookController.cs` | Recepção de webhooks Mercado Pago |
| 14 | `Models/CarrinhoCheckoutPagamento.cs` | Entidades EF Core complementares |
| 15 | `Configurations/ServiceExtensions.cs` | Registro de DI dos novos serviços |

### Configurações necessárias em `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoPago": {
      "AccessToken": "TEST-xxxxxxxxxxxxxxxx",
      "WebhookSecret": "seu_secret",
      "Sandbox": true
    },
    "MelhorEnvio": {
      "Ativo": false,
      "Token": "seu_token",
      "CepOrigem": "01001000"
    },
    "SendGrid": {
      "ApiKey": "SG.xxxxx",
      "FromEmail": "naoresponder@nexumaltivon.com",
      "FromName": "Grupo Nexum Altivon"
    },
    "WhatsApp": {
      "Ativo": false,
      "ApiUrl": "http://sua-api-whatsapp:8080/message/sendText",
      "ApiKey": "sua_chave"
    }
  },
  "Alertas": {
    "EstoqueEmailAdmin": "corporativo.gna@gmail.com"
  }
}
```

### Fluxo de Uso

1. **Cliente anônimo** adiciona itens ao carrinho (cookie `nx_session_id`)
2. Ao **logar**, chama `POST /api/carrinho/migrar` para unir carrinhos
3. Inicia checkout: `POST /api/checkout/iniciar` (endereço + cupom)
4. Seleciona frete: `POST /api/checkout/{id}/frete`
5. Finaliza: `POST /api/checkout/finalizar` (PIX, Cartão ou Boleto)
6. Recebe QR Code / link / boleto na resposta
7. Webhook MP atualiza status automaticamente para "PAGO"

### Próxima Fase
- **FASE 4**: Integrações com Marketplaces (Mercado Livre, Shopee), Dropshipping e Logística completa.
