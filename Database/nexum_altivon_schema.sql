-- ============================================================
-- NEXUM ALTIVON COMMERCE PLATFORM
-- BANCO DE DADOS: nexum_altivon
-- SERVIDOR: 192.168.1.72:3309
-- VERSÃƒO: 1.0.0
-- DATA: 2026-05-23
-- AUTOR: Grupo Nexum Altivon ME
-- ============================================================

CREATE DATABASE IF NOT EXISTS nexum_altivon
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE nexum_altivon;

-- ============================================================
-- TABELA: usuarios (UsuÃ¡rios do Sistema / Admin)
-- ============================================================
CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    senha_hash VARCHAR(255) NOT NULL,
    perfil ENUM('SuperAdmin','Admin','Gerente','Vendedor','Suporte','Financeiro') DEFAULT 'Vendedor',
    avatar VARCHAR(255),
    telefone VARCHAR(20),
    ativo TINYINT(1) DEFAULT 1,
    ultimo_login DATETIME,
    token_refresh VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_email (email),
    INDEX idx_perfil (perfil),
    INDEX idx_ativo (ativo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: lojas (6 Lojas do Grupo SocietÃ¡rio)
-- ============================================================
CREATE TABLE IF NOT EXISTS lojas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    slug VARCHAR(50) NOT NULL UNIQUE,
    segmento VARCHAR(100) NOT NULL,
    descricao TEXT,
    logo VARCHAR(255),
    banner VARCHAR(255),
    cor_primaria VARCHAR(7) DEFAULT '#C9A227',
    cor_secundaria VARCHAR(7) DEFAULT '#1E3A5F',
    dominio VARCHAR(100),
    ativa TINYINT(1) DEFAULT 1,
    ordem_exibicao INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_slug (slug),
    INDEX idx_ativa (ativa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- InserÃ§Ã£o das 6 lojas do Grupo Nexum Altivon
INSERT INTO lojas (nome, slug, segmento, descricao, cor_primaria, cor_secundaria, dominio, ordem_exibicao) VALUES
('Grann-Tur', 'grann-tur', 'Viagens & Turismo', 'Mochilas, malas, acessÃ³rios de viagem e tudo para explorar o mundo com estilo.', '#C9A227', '#1E3A5F', 'grann-tur.nexumaltivon.com', 1),
('Chronos', 'chronos', 'RelÃ³gios & AcessÃ³rios', 'RelÃ³gios que marcam mais que horas â€” marcam estilo. Do clÃ¡ssico ao moderno.', '#C9A227', '#2E5A8F', 'chronos.nexumaltivon.com', 2),
('Moda Mim', 'moda-mim', 'Moda & VestuÃ¡rio', 'TendÃªncias que vestem a sua personalidade. Roupas, calÃ§ados e acessÃ³rios.', '#C9A227', '#8B1E3F', 'moda-mim.nexumaltivon.com', 3),
('GeraÃ§Ã£o Top+', 'geracao-top', 'Tecnologia & Gadgets', 'Tecnologia de ponta ao alcance de todos. Smartphones, gadgets e eletrÃ´nicos.', '#C9A227', '#0F4C3A', 'geracao-top.nexumaltivon.com', 4),
('Estruturaline', 'estruturaline', 'ConstruÃ§Ã£o & Estruturas', 'Ferramentas, materiais de construÃ§Ã£o e equipamentos profissionais.', '#C9A227', '#4A3728', 'estruturaline.nexumaltivon.com', 5),
('Gran-fest-festas', 'gran-fest', 'Festas & Eventos', 'DecoraÃ§Ãµes, utensÃ­lios e tudo para tornar sua festa inesquecÃ­vel.', '#C9A227', '#6B2D5C', 'gran-fest.nexumaltivon.com', 6);

-- ============================================================
-- TABELA: categorias
-- ============================================================
CREATE TABLE IF NOT EXISTS categorias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    loja_id INT NOT NULL,
    nome VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    descricao TEXT,
    imagem VARCHAR(255),
    categoria_pai_id INT DEFAULT NULL,
    ordem INT DEFAULT 0,
    ativa TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (loja_id) REFERENCES lojas(id) ON DELETE CASCADE,
    FOREIGN KEY (categoria_pai_id) REFERENCES categorias(id) ON DELETE SET NULL,
    UNIQUE KEY uk_categoria_loja (loja_id, slug),
    INDEX idx_loja (loja_id),
    INDEX idx_ativa (ativa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: produtos
-- ============================================================
CREATE TABLE IF NOT EXISTS produtos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    loja_id INT NOT NULL,
    categoria_id INT,
    sku VARCHAR(50) NOT NULL UNIQUE,
    nome VARCHAR(200) NOT NULL,
    slug VARCHAR(200) NOT NULL,
    descricao_curta VARCHAR(500),
    descricao_longa LONGTEXT,
    preco DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    preco_promocional DECIMAL(10,2) DEFAULT NULL,
    custo DECIMAL(10,2) DEFAULT 0.00,
    peso DECIMAL(8,3) DEFAULT 0.000,
    altura DECIMAL(8,2) DEFAULT 0.00,
    largura DECIMAL(8,2) DEFAULT 0.00,
    comprimento DECIMAL(8,2) DEFAULT 0.00,
    imagem_principal VARCHAR(255),
    imagens_galeria JSON,
    estoque_minimo INT DEFAULT 5,
    estoque_atual INT DEFAULT 0,
    estoque_reservado INT DEFAULT 0,
    tipo_produto ENUM('Proprio','Dropshipping','Marketplace','Afiliado') DEFAULT 'Proprio',
    fornecedor_id INT,
    marca VARCHAR(100),
    tags VARCHAR(255),
    seo_titulo VARCHAR(200),
    seo_descricao VARCHAR(500),
    seo_keywords VARCHAR(255),
    destaque TINYINT(1) DEFAULT 0,
    ativo TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (loja_id) REFERENCES lojas(id) ON DELETE CASCADE,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id) ON DELETE SET NULL,
    INDEX idx_loja (loja_id),
    INDEX idx_categoria (categoria_id),
    INDEX idx_sku (sku),
    INDEX idx_slug (slug),
    INDEX idx_ativo (ativo),
    INDEX idx_destaque (destaque),
    INDEX idx_tipo (tipo_produto),
    FULLTEXT INDEX ft_nome_desc (nome, descricao_curta, descricao_longa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: fornecedores
-- ============================================================
CREATE TABLE IF NOT EXISTS fornecedores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    razao_social VARCHAR(200) NOT NULL,
    nome_fantasia VARCHAR(200),
    cnpj VARCHAR(18) UNIQUE,
    ie VARCHAR(20),
    email VARCHAR(150),
    telefone VARCHAR(20),
    whatsapp VARCHAR(20),
    endereco TEXT,
    cidade VARCHAR(100),
    estado VARCHAR(2),
    cep VARCHAR(10),
    segmento VARCHAR(100),
    loja_vinculada_id INT,
    comissao_percentual DECIMAL(5,2) DEFAULT 0.00,
    prazo_entrega_dias INT DEFAULT 7,
    status ENUM('Ativo','Inativo','Pendente','Bloqueado') DEFAULT 'Pendente',
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (loja_vinculada_id) REFERENCES lojas(id) ON DELETE SET NULL,
    INDEX idx_cnpj (cnpj),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Adicionar FK em produtos apÃ³s criar fornecedores
ALTER TABLE produtos ADD CONSTRAINT fk_prod_fornecedor
    FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id) ON DELETE SET NULL;

-- ============================================================
-- TABELA: clientes
-- ============================================================
CREATE TABLE IF NOT EXISTS clientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo ENUM('PF','PJ') DEFAULT 'PF',
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    senha_hash VARCHAR(255),
    cpf_cnpj VARCHAR(18) UNIQUE,
    rg_ie VARCHAR(20),
    data_nascimento DATE,
    telefone VARCHAR(20),
    whatsapp VARCHAR(20),
    avatar VARCHAR(255),
    newsletter TINYINT(1) DEFAULT 1,
    vip TINYINT(1) DEFAULT 0,
    pontos_fidelidade INT DEFAULT 0,
    status ENUM('Ativo','Inativo','Bloqueado','Pendente') DEFAULT 'Ativo',
    ultimo_acesso DATETIME,
    token_reset_senha VARCHAR(255),
    token_expira_em DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_email (email),
    INDEX idx_cpf_cnpj (cpf_cnpj),
    INDEX idx_status (status),
    INDEX idx_vip (vip)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: enderecos
-- ============================================================
CREATE TABLE IF NOT EXISTS enderecos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    tipo ENUM('Entrega','Cobranca','Ambos') DEFAULT 'Entrega',
    apelido VARCHAR(50) DEFAULT 'Principal',
    cep VARCHAR(10) NOT NULL,
    logradouro VARCHAR(200) NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    padrao TINYINT(1) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
    INDEX idx_cliente (cliente_id),
    INDEX idx_cep (cep)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: pedidos
-- ============================================================
CREATE TABLE IF NOT EXISTS pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero_pedido VARCHAR(20) NOT NULL UNIQUE,
    cliente_id INT NOT NULL,
    endereco_entrega_id INT,
    loja_id INT,
    status ENUM('Pendente','Pago','EmSeparacao','Enviado','Entregue','Cancelado','Devolvido','Reembolsado') DEFAULT 'Pendente',
    status_pagamento ENUM('Aguardando','Aprovado','Recusado','Estornado','Cancelado') DEFAULT 'Aguardando',
    meio_pagamento VARCHAR(50),
    gateway_pagamento VARCHAR(50),
    gateway_transacao_id VARCHAR(100),
    subtotal DECIMAL(10,2) DEFAULT 0.00,
    desconto DECIMAL(10,2) DEFAULT 0.00,
    frete_valor DECIMAL(10,2) DEFAULT 0.00,
    frete_metodo VARCHAR(50),
    frete_codigo_rastreio VARCHAR(50),
    frete_transportadora VARCHAR(50),
    frete_prazo_dias INT DEFAULT 0,
    total DECIMAL(10,2) DEFAULT 0.00,
    parcelas INT DEFAULT 1,
    juros DECIMAL(10,2) DEFAULT 0.00,
    cupom_codigo VARCHAR(50),
    cupom_desconto DECIMAL(10,2) DEFAULT 0.00,
    observacoes_cliente TEXT,
    observacoes_internas TEXT,
    ip_cliente VARCHAR(45),
    user_agent VARCHAR(255),
    origem ENUM('Site','Marketplace','Dropshipping','Mobile','API') DEFAULT 'Site',
    marketplace_origem VARCHAR(50),
    marketplace_pedido_id VARCHAR(100),
    data_pagamento DATETIME,
    data_envio DATETIME,
    data_entrega DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE RESTRICT,
    FOREIGN KEY (endereco_entrega_id) REFERENCES enderecos(id) ON DELETE SET NULL,
    FOREIGN KEY (loja_id) REFERENCES lojas(id) ON DELETE SET NULL,
    INDEX idx_numero (numero_pedido),
    INDEX idx_cliente (cliente_id),
    INDEX idx_status (status),
    INDEX idx_status_pagamento (status_pagamento),
    INDEX idx_gateway_transacao (gateway_transacao_id),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: pedido_itens
-- ============================================================
CREATE TABLE IF NOT EXISTS pedido_itens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    produto_id INT,
    nome_produto VARCHAR(200) NOT NULL,
    sku_produto VARCHAR(50),
    imagem_produto VARCHAR(255),
    quantidade INT NOT NULL DEFAULT 1,
    preco_unitario DECIMAL(10,2) NOT NULL,
    preco_total DECIMAL(10,2) NOT NULL,
    desconto_item DECIMAL(10,2) DEFAULT 0.00,
    fornecedor_id INT,
    comissao_fornecedor DECIMAL(10,2) DEFAULT 0.00,
    tipo_fulfillment ENUM('Proprio','Dropshipping','Marketplace') DEFAULT 'Proprio',
    status_item ENUM('Pendente','Separado','Enviado','Entregue','Cancelado') DEFAULT 'Pendente',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
    FOREIGN KEY (produto_id) REFERENCES produtos(id) ON DELETE SET NULL,
    FOREIGN KEY (fornecedor_id) REFERENCES fornecedores(id) ON DELETE SET NULL,
    INDEX idx_pedido (pedido_id),
    INDEX idx_produto (produto_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: carrinho
-- ============================================================
CREATE TABLE IF NOT EXISTS carrinho (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT,
    sessao_id VARCHAR(100),
    produto_id INT NOT NULL,
    quantidade INT NOT NULL DEFAULT 1,
    preco_unitario DECIMAL(10,2) NOT NULL,
    variacao VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
    FOREIGN KEY (produto_id) REFERENCES produtos(id) ON DELETE CASCADE,
    INDEX idx_cliente (cliente_id),
    INDEX idx_sessao (sessao_id),
    INDEX idx_updated (updated_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: cupons
-- ============================================================
CREATE TABLE IF NOT EXISTS cupons (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,
    tipo ENUM('Percentual','ValorFixo','FreteGratis') DEFAULT 'Percentual',
    valor DECIMAL(10,2) NOT NULL,
    valor_minimo_pedido DECIMAL(10,2) DEFAULT 0.00,
    valor_maximo_desconto DECIMAL(10,2) DEFAULT NULL,
    quantidade_usos INT DEFAULT NULL,
    usos_atuais INT DEFAULT 0,
    quantidade_por_cliente INT DEFAULT 1,
    valido_de DATE,
    valido_ate DATE,
    lojas_aplicaveis JSON,
    categorias_aplicaveis JSON,
    produtos_aplicaveis JSON,
    clientes_aplicaveis JSON,
    primeiro_compra_only TINYINT(1) DEFAULT 0,
    ativo TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_codigo (codigo),
    INDEX idx_ativo (ativo),
    INDEX idx_valido_ate (valido_ate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: pagamentos
-- ============================================================
CREATE TABLE IF NOT EXISTS pagamentos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    gateway VARCHAR(50) NOT NULL,
    gateway_transacao_id VARCHAR(100),
    metodo ENUM('PIX','CartaoCredito','CartaoDebito','Boleto','Transferencia','Wallet','Outro') NOT NULL,
    status ENUM('Pendente','Processando','Aprovado','Recusado','Estornado','Cancelado','Chargeback') DEFAULT 'Pendente',
    valor DECIMAL(10,2) NOT NULL,
    valor_liquido DECIMAL(10,2),
    taxa_gateway DECIMAL(10,2) DEFAULT 0.00,
    parcelas INT DEFAULT 1,
    bandeira VARCHAR(20),
    ultimos_digitos VARCHAR(4),
    nsu VARCHAR(50),
    autorizacao_codigo VARCHAR(50),
    pix_qrcode TEXT,
    pix_expiracao DATETIME,
    boleto_url VARCHAR(255),
    boleto_codigo_barras VARCHAR(50),
    boleto_vencimento DATE,
    webhook_payload JSON,
    data_processamento DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
    INDEX idx_pedido (pedido_id),
    INDEX idx_gateway_transacao (gateway_transacao_id),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: transportadoras / envios
-- ============================================================
CREATE TABLE IF NOT EXISTS transportadoras (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    slug VARCHAR(50) NOT NULL UNIQUE,
    tipo ENUM('Correios','Transportadora','Logistica','Hub') DEFAULT 'Transportadora',
    api_endpoint VARCHAR(255),
    api_token VARCHAR(255),
    api_sandbox TINYINT(1) DEFAULT 1,
    ativa TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_slug (slug),
    INDEX idx_ativa (ativa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO transportadoras (nome, slug, tipo, ativa) VALUES
('Correios', 'correios', 'Correios', 1),
('Melhor Envio', 'melhor-envio', 'Hub', 1),
('Jadlog', 'jadlog', 'Transportadora', 1),
('Loggi', 'loggi', 'Logistica', 1),
('Kangu', 'kangu', 'Hub', 1);

-- ============================================================
-- TABELA: envios
-- ============================================================
CREATE TABLE IF NOT EXISTS envios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    transportadora_id INT,
    codigo_rastreio VARCHAR(50),
    etiqueta_url VARCHAR(255),
    etiqueta_pdf BLOB,
    status_envio ENUM('Pendente','EtiquetaGerada','Coletado','EmTransito','SaiuEntrega','Entregue','Devolvido','Extraviado') DEFAULT 'Pendente',
    preco DECIMAL(10,2) DEFAULT 0.00,
    prazo_dias INT DEFAULT 0,
    data_postagem DATETIME,
    data_entrega_estimada DATE,
    data_entrega_real DATETIME,
    eventos_rastreamento JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
    FOREIGN KEY (transportadora_id) REFERENCES transportadoras(id) ON DELETE SET NULL,
    INDEX idx_pedido (pedido_id),
    INDEX idx_rastreio (codigo_rastreio),
    INDEX idx_status (status_envio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: marketplaces (ConfiguraÃ§Ãµes de IntegraÃ§Ã£o)
-- ============================================================
CREATE TABLE IF NOT EXISTS marketplaces (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    slug VARCHAR(50) NOT NULL UNIQUE,
    tipo ENUM('B2W','Magalu','MercadoLivre','Shopee','Amazon','AliExpress','Outro') NOT NULL,
    app_id VARCHAR(100),
    app_secret VARCHAR(255),
    access_token TEXT,
    refresh_token TEXT,
    token_expira_em DATETIME,
    loja_vinculada_id INT,
    seller_id VARCHAR(100),
    sandbox TINYINT(1) DEFAULT 1,
    ativo TINYINT(1) DEFAULT 0,
    ultima_sincronizacao DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (loja_vinculada_id) REFERENCES lojas(id) ON DELETE SET NULL,
    INDEX idx_slug (slug),
    INDEX idx_ativo (ativo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO marketplaces (nome, slug, tipo, ativo) VALUES
('Mercado Livre', 'mercado-livre', 'MercadoLivre', 0),
('Shopee', 'shopee', 'Shopee', 0),
('Amazon', 'amazon', 'Amazon', 0),
('Magalu', 'magalu', 'Magalu', 0),
('Americanas', 'americanas', 'B2W', 0),
('Via Varejo', 'via-varejo', 'B2W', 0);

-- ============================================================
-- TABELA: dropshipping_config
-- ============================================================
CREATE TABLE IF NOT EXISTS dropshipping_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    slug VARCHAR(50) NOT NULL UNIQUE,
    tipo ENUM('AliExpress','CJDropshipping','Dropi','Cartpanda','Nuvemshop','Outro') NOT NULL,
    api_endpoint VARCHAR(255),
    api_key VARCHAR(255),
    api_secret VARCHAR(255),
    ativo TINYINT(1) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_slug (slug),
    INDEX idx_ativo (ativo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO dropshipping_config (nome, slug, tipo, ativo) VALUES
('AliExpress', 'aliexpress', 'AliExpress', 0),
('CJ Dropshipping', 'cj-dropshipping', 'CJDropshipping', 0),
('Dropi', 'dropi', 'Dropi', 0),
('Cartpanda HUB', 'cartpanda', 'Cartpanda', 0),
('Nuvemshop HUB', 'nuvemshop', 'Nuvemshop', 0);

-- ============================================================
-- TABELA: crm_leads
-- ============================================================
CREATE TABLE IF NOT EXISTS crm_leads (
    id INT AUTO_INCREMENT PRIMARY KEY,
    origem ENUM('Site','WhatsApp','Email','Telefone','Marketplace','Indicacao','Campanha','Outro') DEFAULT 'Site',
    tipo ENUM('ClienteVIP','Dropshipping','Fornecedor','Parceiro','Afiliado','Outro') DEFAULT 'ClienteVIP',
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150),
    telefone VARCHAR(20),
    whatsapp VARCHAR(20),
    empresa VARCHAR(200),
    cnpj VARCHAR(18),
    segmento VARCHAR(100),
    proposta TEXT,
    experiencia VARCHAR(50),
    status ENUM('Novo','EmAtendimento','Qualificado','Convertido','Perdido','Arquivado') DEFAULT 'Novo',
    responsavel_id INT,
    prioridade ENUM('Baixa','Media','Alta','Urgente') DEFAULT 'Media',
    anotacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (responsavel_id) REFERENCES usuarios(id) ON DELETE SET NULL,
    INDEX idx_status (status),
    INDEX idx_tipo (tipo),
    INDEX idx_origem (origem),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: crm_atendimentos
-- ============================================================
CREATE TABLE IF NOT EXISTS crm_atendimentos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    lead_id INT,
    cliente_id INT,
    tipo ENUM('Ligacao','Email','WhatsApp','Chat','Reuniao','Visita','Outro') DEFAULT 'WhatsApp',
    assunto VARCHAR(200),
    descricao TEXT,
    status ENUM('Aberto','EmAndamento','Aguardando','Resolvido','Cancelado') DEFAULT 'Aberto',
    responsavel_id INT,
    data_agendamento DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (lead_id) REFERENCES crm_leads(id) ON DELETE SET NULL,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE SET NULL,
    FOREIGN KEY (responsavel_id) REFERENCES usuarios(id) ON DELETE SET NULL,
    INDEX idx_lead (lead_id),
    INDEX idx_cliente (cliente_id),
    INDEX idx_status (status),
    INDEX idx_responsavel (responsavel_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: financeiro (LanÃ§amentos)
-- ============================================================
CREATE TABLE IF NOT EXISTS financeiro (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT,
    tipo ENUM('Receita','Despesa','Transferencia','Estorno','Taxa') DEFAULT 'Receita',
    categoria VARCHAR(100),
    descricao VARCHAR(255),
    valor DECIMAL(10,2) NOT NULL,
    data_vencimento DATE,
    data_pagamento DATE,
    status ENUM('Pendente','Pago','Atrasado','Cancelado','Estornado') DEFAULT 'Pendente',
    meio_pagamento VARCHAR(50),
    conta_bancaria VARCHAR(100),
    comprovante_url VARCHAR(255),
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE SET NULL,
    INDEX idx_pedido (pedido_id),
    INDEX idx_tipo (tipo),
    INDEX idx_status (status),
    INDEX idx_vencimento (data_vencimento)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: fiscal (Notas Fiscais)
-- ============================================================
CREATE TABLE IF NOT EXISTS fiscal (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    numero_nfe VARCHAR(20),
    serie VARCHAR(5),
    chave_acesso VARCHAR(44),
    xml_url VARCHAR(255),
    danfe_url VARCHAR(255),
    status_nfe ENUM('Pendente','Emitida','Autorizada','Cancelada','Denegada','Inutilizada') DEFAULT 'Pendente',
    valor_total DECIMAL(10,2),
    cfop VARCHAR(10),
    natureza_operacao VARCHAR(100),
    data_emissao DATETIME,
    data_autorizacao DATETIME,
    protocolo VARCHAR(50),
    motivo_cancelamento TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
    INDEX idx_pedido (pedido_id),
    INDEX idx_chave (chave_acesso),
    INDEX idx_status (status_nfe)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: notificacoes
-- ============================================================
CREATE TABLE IF NOT EXISTS notificacoes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo ENUM('Sistema','Pedido','Pagamento','Envio','CRM','Marketing','Seguranca') DEFAULT 'Sistema',
    titulo VARCHAR(200) NOT NULL,
    mensagem TEXT,
    destinatario_tipo ENUM('Cliente','Usuario','Todos') DEFAULT 'Todos',
    destinatario_id INT,
    lida TINYINT(1) DEFAULT 0,
    data_leitura DATETIME,
    link_acao VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_destinatario (destinatario_tipo, destinatario_id),
    INDEX idx_lida (lida),
    INDEX idx_tipo (tipo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: logs_auditoria
-- ============================================================
CREATE TABLE IF NOT EXISTS logs_auditoria (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    tabela VARCHAR(50) NOT NULL,
    registro_id INT NOT NULL,
    acao ENUM('INSERT','UPDATE','DELETE','LOGIN','LOGOUT','ERRO','API') NOT NULL,
    usuario_id INT,
    usuario_tipo ENUM('Cliente','Usuario','Sistema','API') DEFAULT 'Sistema',
    ip_address VARCHAR(45),
    user_agent VARCHAR(255),
    dados_anteriores JSON,
    dados_novos JSON,
    endpoint VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_tabela (tabela),
    INDEX idx_registro (registro_id),
    INDEX idx_acao (acao),
    INDEX idx_usuario (usuario_id),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABELA: configuracoes_sistema
-- ============================================================
CREATE TABLE IF NOT EXISTS configuracoes_sistema (
    id INT AUTO_INCREMENT PRIMARY KEY,
    chave VARCHAR(100) NOT NULL UNIQUE,
    valor TEXT,
    tipo ENUM('Texto','Numero','Booleano','JSON','Senha') DEFAULT 'Texto',
    descricao VARCHAR(255),
    grupo VARCHAR(50),
    editavel TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_chave (chave),
    INDEX idx_grupo (grupo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ConfiguraÃ§Ãµes iniciais
INSERT INTO configuracoes_sistema (chave, valor, tipo, descricao, grupo) VALUES
('site_nome', 'Grupo Nexum Altivon', 'Texto', 'Nome do site/empresa', 'Geral'),
('site_url', 'https://www.nexumaltivon.com', 'Texto', 'URL oficial', 'Geral'),
('site_logo', '/assets/logo-2.jpg', 'Texto', 'Logo principal', 'Geral'),
('site_email_contato', 'corporativo.gna@gmail.com', 'Texto', 'E-mail de contato', 'Geral'),
('site_telefone', '(14) 99673-1879', 'Texto', 'Telefone principal', 'Geral'),
('site_whatsapp', '5514996731879', 'Texto', 'WhatsApp principal', 'Geral'),
('frete_gratis_valor_minimo', '299.00', 'Numero', 'Valor mÃ­nimo para frete grÃ¡tis', 'Frete'),
('frete_padrao_metodo', 'Melhor Envio', 'Texto', 'MÃ©todo de frete padrÃ£o', 'Frete'),
('pagamento_pix_ativo', '1', 'Booleano', 'PIX habilitado', 'Pagamento'),
('pagamento_cartao_ativo', '1', 'Booleano', 'CartÃ£o habilitado', 'Pagamento'),
('pagamento_boleto_ativo', '1', 'Booleano', 'Boleto habilitado', 'Pagamento'),
('pagamento_gateway_principal', 'MercadoPago', 'Texto', 'Gateway principal', 'Pagamento'),
('parcelamento_maximo', '12', 'Numero', 'MÃ¡ximo de parcelas', 'Pagamento'),
('parcelamento_sem_juros', '6', 'Numero', 'Parcelas sem juros', 'Pagamento'),
('taxa_juros_parcelamento', '1.99', 'Numero', 'Taxa de juros ao mÃªs', 'Pagamento'),
('dropshipping_ativo', '1', 'Booleano', 'Dropshipping habilitado', 'Dropshipping'),
('marketplace_hub_ativo', '1', 'Booleano', 'Marketplace HUB habilitado', 'Marketplace'),
('recuperacao_carrinho_ativo', '1', 'Booleano', 'RecuperaÃ§Ã£o de carrinho', 'Marketing'),
('afiliados_ativo', '0', 'Booleano', 'Programa de afiliados', 'Marketing'),
('cashback_ativo', '0', 'Booleano', 'Sistema de cashback', 'Marketing'),
('fidelidade_ponto_reais', '1.00', 'Numero', 'Valor do ponto de fidelidade', 'Marketing'),
('manutencao_modo', '0', 'Booleano', 'Modo manutenÃ§Ã£o', 'Sistema'),
('api_rate_limit', '100', 'Numero', 'Limite de requisiÃ§Ãµes por minuto', 'API'),
('jwt_expiracao_horas', '24', 'Numero', 'ExpiraÃ§Ã£o do token JWT', 'Seguranca'),
('jwt_refresh_dias', '7', 'Numero', 'ExpiraÃ§Ã£o do refresh token', 'Seguranca');

-- ============================================================
-- VIEWS
-- ============================================================

-- View: Resumo de Pedidos por Status
CREATE OR REPLACE VIEW v_resumo_pedidos_status AS
SELECT
    status,
    COUNT(*) as total,
    SUM(total) as valor_total,
    DATE(created_at) as data
FROM pedidos
GROUP BY status, DATE(created_at);

-- View: Produtos com Estoque Baixo
CREATE OR REPLACE VIEW v_estoque_baixo AS
SELECT
    p.id,
    p.sku,
    p.nome,
    l.nome as loja,
    p.estoque_atual,
    p.estoque_minimo,
    p.estoque_atual - p.estoque_minimo as deficit,
    p.preco,
    f.nome_fantasia as fornecedor
FROM produtos p
LEFT JOIN lojas l ON p.loja_id = l.id
LEFT JOIN fornecedores f ON p.fornecedor_id = f.id
WHERE p.estoque_atual <= p.estoque_minimo AND p.ativo = 1;

-- View: Vendas por Loja (Dashboard)
CREATE OR REPLACE VIEW v_vendas_loja AS
SELECT
    l.nome as loja,
    l.slug,
    COUNT(p.id) as total_pedidos,
    SUM(p.total) as faturamento,
    AVG(p.total) as ticket_medio,
    DATE(p.created_at) as data
FROM pedidos p
LEFT JOIN lojas l ON p.loja_id = l.id
WHERE p.status IN ('Pago', 'EmSeparacao', 'Enviado', 'Entregue')
GROUP BY l.nome, l.slug, DATE(p.created_at);

-- View: Clientes VIP
CREATE OR REPLACE VIEW v_clientes_vip AS
SELECT
    c.*,
    COUNT(p.id) as total_pedidos,
    SUM(p.total) as total_gasto,
    MAX(p.created_at) as ultima_compra
FROM clientes c
LEFT JOIN pedidos p ON c.id = p.cliente_id AND p.status NOT IN ('Cancelado', 'Reembolsado')
WHERE c.vip = 1 OR c.pontos_fidelidade >= 1000
GROUP BY c.id;

-- View: Leads por Status CRM
CREATE OR REPLACE VIEW v_crm_leads_status AS
SELECT
    tipo,
    status,
    COUNT(*) as total,
    DATE(created_at) as data
FROM crm_leads
GROUP BY tipo, status, DATE(created_at);

-- ============================================================
-- PROCEDURES
-- ============================================================

DELIMITER //

-- Procedure: Gerar nÃºmero de pedido Ãºnico
CREATE PROCEDURE sp_gerar_numero_pedido(
    OUT numero_pedido VARCHAR(20)
)
BEGIN
    DECLARE ano CHAR(2);
    DECLARE mes CHAR(2);
    DECLARE dia CHAR(2);
    DECLARE sequencia INT;
    DECLARE prefixo VARCHAR(8);

    SET ano = RIGHT(YEAR(CURDATE()), 2);
    SET mes = LPAD(MONTH(CURDATE()), 2, '0');
    SET dia = LPAD(DAY(CURDATE()), 2, '0');
    SET prefixo = CONCAT('NX', ano, mes, dia);

    SELECT COALESCE(MAX(CAST(SUBSTRING(numero_pedido, 9) AS UNSIGNED)), 0) + 1
    INTO sequencia
    FROM pedidos
    WHERE numero_pedido LIKE CONCAT(prefixo, '%')
    FOR UPDATE;

    SET numero_pedido = CONCAT(prefixo, LPAD(sequencia, 4, '0'));
END //

-- Procedure: Atualizar estoque apÃ³s pedido
CREATE PROCEDURE sp_atualizar_estoque_pedido(
    IN p_pedido_id INT,
    IN p_operacao ENUM('Reservar','Liberar','Confirmar')
)
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_produto_id INT;
    DECLARE v_quantidade INT;
    DECLARE v_tipo ENUM('Proprio','Dropshipping','Marketplace','Afiliado');

    DECLARE cur CURSOR FOR
        SELECT produto_id, quantidade, p.tipo_produto
        FROM pedido_itens pi
        JOIN produtos p ON pi.produto_id = p.id
        WHERE pi.pedido_id = p_pedido_id;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO v_produto_id, v_quantidade, v_tipo;
        IF done THEN
            LEAVE read_loop;
        END IF;

        IF v_tipo = 'Proprio' THEN
            IF p_operacao = 'Reservar' THEN
                UPDATE produtos
                SET estoque_reservado = estoque_reservado + v_quantidade,
                    estoque_atual = estoque_atual - v_quantidade
                WHERE id = v_produto_id;
            ELSEIF p_operacao = 'Liberar' THEN
                UPDATE produtos
                SET estoque_reservado = estoque_reservado - v_quantidade,
                    estoque_atual = estoque_atual + v_quantidade
                WHERE id = v_produto_id;
            ELSEIF p_operacao = 'Confirmar' THEN
                UPDATE produtos
                SET estoque_reservado = estoque_reservado - v_quantidade
                WHERE id = v_produto_id;
            END IF;
        END IF;
    END LOOP;

    CLOSE cur;
END //

-- Procedure: Calcular totais do pedido
CREATE PROCEDURE sp_calcular_total_pedido(
    IN p_pedido_id INT
)
BEGIN
    DECLARE v_subtotal DECIMAL(10,2);
    DECLARE v_total DECIMAL(10,2);

    SELECT SUM(preco_total) INTO v_subtotal
    FROM pedido_itens
    WHERE pedido_id = p_pedido_id;

    SET v_subtotal = COALESCE(v_subtotal, 0);

    UPDATE pedidos
    SET subtotal = v_subtotal,
        total = v_subtotal + frete_valor - desconto - cupom_desconto + juros
    WHERE id = p_pedido_id;
END //

-- Procedure: Registrar log de auditoria
CREATE PROCEDURE sp_registrar_auditoria(
    IN p_tabela VARCHAR(50),
    IN p_registro_id INT,
    IN p_acao VARCHAR(20),
    IN p_usuario_id INT,
    IN p_usuario_tipo VARCHAR(20),
    IN p_ip VARCHAR(45),
    IN p_user_agent VARCHAR(255),
    IN p_dados_anteriores JSON,
    IN p_dados_novos JSON,
    IN p_endpoint VARCHAR(255)
)
BEGIN
    INSERT INTO logs_auditoria (
        tabela, registro_id, acao, usuario_id, usuario_tipo,
        ip_address, user_agent, dados_anteriores, dados_novos, endpoint
    ) VALUES (
        p_tabela, p_registro_id, p_acao, p_usuario_id, p_usuario_tipo,
        p_ip, p_user_agent, p_dados_anteriores, p_dados_novos, p_endpoint
    );
END //

-- Procedure: Limpar carrinhos abandonados (mais de 7 dias)
CREATE PROCEDURE sp_limpar_carrinho_abandonado()
BEGIN
    DELETE FROM carrinho
    WHERE updated_at < DATE_SUB(NOW(), INTERVAL 7 DAY)
    AND (cliente_id IS NULL OR cliente_id NOT IN (SELECT id FROM clientes WHERE vip = 1));

    SELECT ROW_COUNT() as registros_removidos;
END //

-- Procedure: RelatÃ³rio diÃ¡rio de vendas
CREATE PROCEDURE sp_relatorio_vendas_dia(
    IN p_data DATE
)
BEGIN
    SELECT
        l.nome as loja,
        COUNT(p.id) as pedidos,
        SUM(p.total) as faturamento,
        AVG(p.total) as ticket_medio,
        SUM(p.desconto + p.cupom_desconto) as descontos,
        COUNT(DISTINCT p.cliente_id) as clientes_unicos
    FROM pedidos p
    LEFT JOIN lojas l ON p.loja_id = l.id
    WHERE DATE(p.created_at) = p_data
    AND p.status NOT IN ('Cancelado', 'Reembolsado')
    GROUP BY l.nome
    WITH ROLLUP;
END //

DELIMITER ;

-- ============================================================
-- TRIGGERS
-- ============================================================

DELIMITER //

-- Trigger: Atualizar updated_at em pedidos automaticamente
CREATE TRIGGER trg_pedido_before_update
BEFORE UPDATE ON pedidos
FOR EACH ROW
BEGIN
    IF NEW.status = 'Pago' AND OLD.status != 'Pago' THEN
        SET NEW.data_pagamento = NOW();
    END IF;
    IF NEW.status = 'Enviado' AND OLD.status != 'Enviado' THEN
        SET NEW.data_envio = NOW();
    END IF;
    IF NEW.status = 'Entregue' AND OLD.status != 'Entregue' THEN
        SET NEW.data_entrega = NOW();
    END IF;
END //

-- Trigger: Log de auditoria em clientes
CREATE TRIGGER trg_cliente_after_update
AFTER UPDATE ON clientes
FOR EACH ROW
BEGIN
    INSERT INTO logs_auditoria (
        tabela, registro_id, acao, usuario_id, usuario_tipo,
        dados_anteriores, dados_novos
    ) VALUES (
        'clientes', NEW.id, 'UPDATE', NULL, 'Sistema',
        JSON_OBJECT(
            'nome', OLD.nome, 'email', OLD.email, 'status', OLD.status,
            'vip', OLD.vip, 'pontos_fidelidade', OLD.pontos_fidelidade
        ),
        JSON_OBJECT(
            'nome', NEW.nome, 'email', NEW.email, 'status', NEW.status,
            'vip', NEW.vip, 'pontos_fidelidade', NEW.pontos_fidelidade
        )
    );
END //

DELIMITER ;

-- ============================================================
-- USUÃRIO ADMIN INICIAL (senha: Admin@Nexum2026!)
-- Hash BCrypt: $2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy
-- ============================================================
INSERT INTO usuarios (nome, email, senha_hash, perfil, ativo) VALUES
('Administrador Master', 'admin@nexumaltivon.com', '$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'SuperAdmin', 1),
('Rodrigo Costa', 'corporativo.gna@gmail.com', '$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Admin', 1),
('Vinicius', 'corporativo.gna@gmail.com', '$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Gerente', 1);

-- ============================================================
-- FIM DO SCRIPT
-- ============================================================

