# Handoff privado de produção

Este repositório **não deve receber segredos reais**.  
Preencha credenciais reais **somente no servidor**.

## Arquivo-base

Use como modelo:

- `NexumAltivon_Back-End/API/appsettings.PrivateProduction.template.json`

Crie no servidor um arquivo privado fora do versionamento, por exemplo:

- `appsettings.Production.json`

## Campos mínimos antes de liberar a equipe

### Base operacional

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`
- `AdminUser:Password`
- `EmailSettings:Password`

### Shopify

- `Shopify:StoreDomain`
- `Shopify:ApiVersion`
- `Shopify:AdminApiAccessToken`
- `Shopify:WebhookSecret`

### CJ Dropshipping

- `CJDropshipping:ApiEndpoint`
- `CJDropshipping:AccessToken`
- `CJDropshipping:WebhookSecret`

## Validação segura

Depois de preencher no servidor:

1. subir a API;
2. entrar no painel administrativo;
3. abrir `Integrações`;
4. testar `Shopify`;
5. testar `CJ Dropshipping`;
6. só então vincular produtos reais aos canais.

## Regras de segurança

- não commitar `appsettings.Production.json`;
- não enviar tokens por chat;
- não salvar senha em arquivo público do front;
- usar somente backend/servidor para tokens e segredos;
- revisar logs antes de liberar a operação.
