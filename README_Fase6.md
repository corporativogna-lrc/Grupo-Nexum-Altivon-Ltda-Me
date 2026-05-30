# FASE 6 — Estrutura GitHub + CI/CD + Docker + Documentação de Deploy
## Grupo Nexum Altivon ME | www.nexumaltivon.com

---

## 📋 Sumário

Esta fase final entrega a infraestrutura completa de DevOps: repositórios GitHub organizados, pipelines CI/CD automatizadas, containers Docker, orquestração com Docker Compose, scripts de backup/restore e documentação de deploy em produção.

---

## 🗂️ Estrutura de Repositórios GitHub

```
Organização: github.com/nexumaltivon
│
├── NexumAltivon.API          → Back-end ASP.NET Core 8 (Fases 1-5)
├── NexumAltivon.Front        → Front-end Next.js (futuro)
├── NexumAltivon.Database     → Scripts SQL e migrations
├── NexumAltivon.Docs         → Documentação técnica e de negócio
└── NexumAltivon.Infra        → Docker, CI/CD, scripts de deploy
```

---

## 🔄 Pipeline CI/CD (GitHub Actions)

### Arquivo: `.github/workflows/ci-cd.yml`

| Stage | Descrição | Gatilho |
|-------|-----------|---------|
| **Build & Test** | Restore, build e testes xUnit com MySQL em container | Push em `main` ou `develop` |
| **Docker Build** | Build de imagem e push para GitHub Container Registry | Após build/test OK |
| **Deploy Staging** | SSH no servidor staging + `docker-compose up` | Branch `develop` |
| **Deploy Production** | SSH no servidor produção + `docker-compose up` | Branch `main` |

### Secrets Necessários (GitHub)

| Secret | Descrição |
|--------|-----------|
| `STAGING_HOST` | IP do servidor staging |
| `STAGING_USER` | Usuário SSH staging |
| `STAGING_SSH_KEY` | Chave privada SSH staging |
| `PROD_HOST` | IP do servidor produção |
| `PROD_USER` | Usuário SSH produção |
| `PROD_SSH_KEY` | Chave privada SSH produção |

---

## 🐳 Docker

### Dockerfile.api
- Base: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Multi-stage build (build → publish → runtime)
- Usuário não-root (`nexum`) para segurança
- Health check em `/health`
- Exposição nas portas 8080/8081

### Docker Compose — Desenvolvimento
```bash
cd docker
docker-compose up -d
```
Serviços:
- **mysql**: Banco MySQL 8.0 com seed automático
- **api**: API ASP.NET Core 8
- **redis**: Cache distribuído (opcional)
- **nginx**: Reverse proxy (opcional)

### Docker Compose — Produção
```bash
cd docker
docker-compose -f docker-compose.prod.yml up -d
```
Serviços:
- **mysql**: MySQL 8.0 com volumes persistentes
- **api**: Imagem do GitHub Container Registry
- **watchtower**: Atualização automática de imagens

---

## 🔐 Nginx Reverse Proxy

Arquivo: `docker/nginx/nginx.conf`

- Proxy reverso para API
- Suporte a WebSocket (Upgrade/Connection)
- Headers de segurança (X-Real-IP, X-Forwarded-For)
- Health check otimizado
- Pronto para SSL/TLS (configurar certificados)

---

## 💾 Backup e Restore

### Backup Automático
```bash
# Agendar no crontab (todo dia às 2h)
0 2 * * * /opt/nexumaltivon/scripts/backup-mysql.sh
```

- Backup completo com `mysqldump`
- Compactação com `gzip`
- Retenção de 30 dias
- Log de execução

### Restore
```bash
./restore-mysql.sh /backups/mysql/nexum_altivon_20260527_020000.sql.gz
```

- Confirmação interativa
- Descompactação automática
- Restore direto no MySQL

---

## 🚀 Guia de Deploy Passo a Passo

### 1. Preparação do Servidor

```bash
# Instalar Docker e Docker Compose
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Criar estrutura de diretórios
sudo mkdir -p /opt/nexumaltivon/{production,staging,backups,logs}
sudo chown -R $USER:$USER /opt/nexumaltivon
```

### 2. Configurar Variáveis de Ambiente

Criar `/opt/nexumaltivon/production/.env`:
```env
MYSQL_ROOT_PASSWORD=sua_senha_forte
MYSQL_USER=nexum_prod
MYSQL_PASSWORD=sua_senha_forte
CONNECTION_STRING=Server=mysql;Port=3306;Database=nexum_altivon;Uid=nexum_prod;Pwd=sua_senha_forte;
JWT_SECRET=sua_chave_jwt_256bits_producao
MP_ACCESS_TOKEN=APP_USR-...
SENDGRID_API_KEY=SG.xxxxx
```

### 3. Primeiro Deploy

```bash
cd /opt/nexumaltivon/production

# Clonar repositório
git clone https://github.com/nexumaltivon/NexumAltivon.API.git .

# Copiar docker-compose.prod.yml
cp docker/docker-compose.prod.yml .

# Iniciar serviços
docker-compose -f docker-compose.prod.yml up -d

# Verificar status
docker-compose -f docker-compose.prod.yml ps
docker logs -f nexum-api-prod
```

### 4. Configurar SSL (Let's Encrypt)

```bash
# Instalar certbot
docker run -it --rm   -v /opt/nexumaltivon/ssl:/etc/letsencrypt   -v /opt/nexumaltivon/ssl-data:/data/letsencrypt   certbot/certbot certonly   --standalone -d api.nexumaltivon.com

# Atualizar nginx.conf com caminhos dos certificados
# Reiniciar nginx
```

### 5. Agendar Backups

```bash
# Adicionar ao crontab
crontab -e
# Adicionar linha:
0 2 * * * /opt/nexumaltivon/scripts/backup-mysql.sh >> /var/log/nexum-backup.log 2>&1
```

---

## 📊 Monitoramento

### Health Checks
- `/health` — Status geral da API + banco
- `/hangfire` — Dashboard de jobs (protegido)
- Nginx status page (opcional)

### Logs
```bash
# API
docker logs -f nexum-api-prod

# MySQL
docker logs -f nexum-mysql-prod

# Nginx
docker logs -f nexum-nginx
```

### Métricas Recomendadas (futuro: Prometheus + Grafana)
- Taxa de requests/segundo
- Tempo de resposta médio
- Taxa de erro 5xx
- Uso de CPU/Memória
- Conexões ativas MySQL

---

## 🔧 Comandos Úteis

```bash
# Ver logs em tempo real
docker-compose -f docker-compose.prod.yml logs -f api

# Reiniciar serviço específico
docker-compose -f docker-compose.prod.yml restart api

# Escalar API (se usar Docker Swarm/K8s futuramente)
docker-compose -f docker-compose.prod.yml up -d --scale api=3

# Acessar container MySQL
docker exec -it nexum-mysql-prod mysql -u root -p

# Acessar container API
docker exec -it nexum-api-prod /bin/sh

# Atualizar imagem manualmente
docker-compose -f docker-compose.prod.yml pull api
docker-compose -f docker-compose.prod.yml up -d api
```

---

## ✅ Checklist de Produção

- [ ] Variáveis de ambiente configuradas
- [ ] SSL/TLS ativo
- [ ] Firewall configurado (apenas 80/443/3309 expostos)
- [ ] Backups automáticos agendados
- [ ] Monitoramento ativo
- [ ] Documentação de rollback preparada
- [ ] Testes de carga realizados
- [ ] Política de senhas forte no banco
- [ ] JWT secret diferente do ambiente de dev
- [ ] Tokens de integração (MP, SendGrid) são de produção

---

## 📞 Suporte

**Rodrigo Costa** — (14) 99673-1879  
**Vinicius** — (14) 99634-8409  
**E-mail**: contato@nexumaltivon.com  
**Site**: www.nexumaltivon.com

---

© 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.
