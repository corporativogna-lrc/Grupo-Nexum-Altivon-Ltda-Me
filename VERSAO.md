# Nexum Altivon — Versão 1.1.4

Marco: `api-24h-servidor-e-integracoes-base-2026-06-04`

Data de registro: 2026-06-04

Status:
- API preparada para instalação 24h no servidor, sem depender do Codex ou desta máquina de trabalho.
- API local fixada para autoarranque pelo usuário do Windows em `http://localhost:5010`.
- API local passa a rodar por publicação própria em `.nexum-runtime\api-local`, reduzindo dependência do projeto aberto.
- Serviço local do Cloudflared confirmado como automático nesta máquina.
- Pacote do servidor corrigido para usar a unidade compartilhada `Y:\NexumAltivon_API_24H_Y_FIX`, evitando instalação indevida em `C:`.
- Versão operacional congelada antes da etapa crítica de integrações comerciais.
- Checkout, frete, gateway escolhido, upload de imagem e painel de integrações seguem como base funcional.
- Uploads reais de produtos ficam fora do Git para evitar versionar arquivos operacionais.
- Próxima prioridade travada: corrigir publicação pública do hostname `api.nexumaltivon.com`, validar túnel/DNS público e avançar integrações de dropshipping, logística, gateways, e-commerce/marketplaces e financeiro.

Observação operacional:
- Esta versão é o ponto de estabilidade para apresentar evolução à diretoria e atacar operação 24h.
- Toda próxima melhoria deve validar site, painel, login, API e checkout antes da publicação.
- Se `https://api.nexumaltivon.com/health` falhar enquanto `http://localhost:5010/health` estiver saudável, o problema é DNS/túnel público, não a API local.
