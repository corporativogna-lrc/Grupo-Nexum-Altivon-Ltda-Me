-- ============================================================
-- SCRIPT DE ATUALIZAÇÃO DO BANCO NEXUM_ALTIVON
-- FASE 5 — ERP/CRM GenesisGest.Net
-- Adiciona 15+ tabelas ao schema existente da Fase 1
-- Execute após o schema base (Fase 1) já estar aplicado
-- Servidor: 192.168.1.72:3309 | Database: nexum_altivon
-- ============================================================

USE nexum_altivon;

-- ==================== FINANCEIRO ====================

CREATE TABLE IF NOT EXISTS erp_centros_custo (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(500),
    pai_id INT,
    tipo VARCHAR(20) NOT NULL DEFAULT 'Sintetico',
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pai_id) REFERENCES erp_centros_custo(id)
);

CREATE TABLE IF NOT EXISTS erp_contas_bancarias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    banco VARCHAR(100) NOT NULL,
    agencia VARCHAR(10),
    conta VARCHAR(20),
    tipo_conta VARCHAR(20),
    saldo_atual DECIMAL(18,2) DEFAULT 0.00,
    saldo_inicial DECIMAL(18,2) DEFAULT 0.00,
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS erp_contas_pagar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero_documento VARCHAR(20) NOT NULL,
    fornecedor_id INT NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    valor_original DECIMAL(18,2) NOT NULL,
    valor_pago DECIMAL(18,2) DEFAULT 0.00,
    valor_multa DECIMAL(18,2) DEFAULT 0.00,
    valor_juros DECIMAL(18,2) DEFAULT 0.00,
    valor_desconto DECIMAL(18,2) DEFAULT 0.00,
    data_emissao DATETIME NOT NULL,
    data_vencimento DATETIME NOT NULL,
    data_pagamento DATETIME,
    status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
    forma_pagamento VARCHAR(50),
    numero_boleto VARCHAR(100),
    observacoes TEXT,
    loja_id INT,
    centro_custo_id INT NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100),
    FOREIGN KEY (fornecedor_id) REFERENCES erp_fornecedores(id),
    FOREIGN KEY (centro_custo_id) REFERENCES erp_centros_custo(id),
    INDEX idx_cp_status_vencimento (status, data_vencimento)
);

CREATE TABLE IF NOT EXISTS erp_contas_receber (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero_documento VARCHAR(20) NOT NULL,
    cliente_id INT NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    valor_original DECIMAL(18,2) NOT NULL,
    valor_recebido DECIMAL(18,2) DEFAULT 0.00,
    valor_multa DECIMAL(18,2) DEFAULT 0.00,
    valor_juros DECIMAL(18,2) DEFAULT 0.00,
    valor_desconto DECIMAL(18,2) DEFAULT 0.00,
    data_emissao DATETIME NOT NULL,
    data_vencimento DATETIME NOT NULL,
    data_recebimento DATETIME,
    status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
    forma_recebimento VARCHAR(50),
    numero_pedido_referencia VARCHAR(100),
    observacoes TEXT,
    loja_id INT,
    centro_custo_id INT NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100),
    FOREIGN KEY (centro_custo_id) REFERENCES erp_centros_custo(id),
    INDEX idx_cr_status_vencimento (status, data_vencimento)
);

CREATE TABLE IF NOT EXISTS erp_fluxo_caixa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    data DATETIME NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    categoria VARCHAR(50),
    conta_pagar_id INT,
    conta_receber_id INT,
    pedido_id INT,
    forma_pagamento VARCHAR(50),
    conta_bancaria VARCHAR(100),
    observacoes TEXT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_por VARCHAR(100),
    INDEX idx_fc_data_tipo (data, tipo)
);

CREATE TABLE IF NOT EXISTS erp_boletos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    conta_receber_id INT NOT NULL,
    nosso_numero VARCHAR(100),
    linha_digitavel VARCHAR(255),
    codigo_barras VARCHAR(255),
    banco VARCHAR(100),
    vencimento DATETIME NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'EM_ABERTO',
    url_boleto VARCHAR(500),
    pdf_url VARCHAR(500),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_boleto_conta_receber (conta_receber_id),
    INDEX idx_boleto_status_vencimento (status, vencimento)
);

CREATE TABLE IF NOT EXISTS erp_financeiro_referencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo VARCHAR(40) NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    descricao VARCHAR(150) NOT NULL,
    ordem INT NOT NULL DEFAULT 0,
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_erp_fin_ref_tipo_codigo (tipo, codigo),
    INDEX idx_erp_fin_ref_tipo_ordem (tipo, ordem, descricao)
);

-- ==================== FISCAL ====================

CREATE TABLE IF NOT EXISTS erp_notas_fiscais (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero VARCHAR(20) NOT NULL,
    serie VARCHAR(10) NOT NULL DEFAULT '1',
    tipo VARCHAR(10) NOT NULL,
    natureza_operacao VARCHAR(20) NOT NULL,
    emitente_id INT NOT NULL,
    destinatario_id INT NOT NULL,
    valor_total DECIMAL(18,2) NOT NULL,
    valor_icms DECIMAL(18,2) DEFAULT 0.00,
    valor_ipi DECIMAL(18,2) DEFAULT 0.00,
    valor_pis DECIMAL(18,2) DEFAULT 0.00,
    valor_cofins DECIMAL(18,2) DEFAULT 0.00,
    valor_frete DECIMAL(18,2) DEFAULT 0.00,
    valor_seguro DECIMAL(18,2) DEFAULT 0.00,
    valor_desconto DECIMAL(18,2) DEFAULT 0.00,
    data_emissao DATETIME NOT NULL,
    data_saida_entrada DATETIME,
    status VARCHAR(50) NOT NULL DEFAULT 'Emitida',
    chave_acesso VARCHAR(44),
    xml_autorizacao TEXT,
    protocolo_autorizacao VARCHAR(500),
    pedido_id INT,
    loja_id INT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS erp_itens_nota_fiscal (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nota_fiscal_id INT NOT NULL,
    produto_id INT NOT NULL,
    descricao VARCHAR(120) NOT NULL,
    cfop VARCHAR(20) NOT NULL,
    ncm VARCHAR(10) NOT NULL,
    cst_icms VARCHAR(10) NOT NULL,
    cst_pis VARCHAR(10) NOT NULL,
    cst_cofins VARCHAR(10) NOT NULL,
    quantidade DECIMAL(18,3) NOT NULL,
    valor_unitario DECIMAL(18,2) NOT NULL,
    valor_total DECIMAL(18,2) NOT NULL,
    valor_icms DECIMAL(18,2) DEFAULT 0.00,
    aliquota_icms DECIMAL(18,2) DEFAULT 0.00,
    base_calculo_icms DECIMAL(18,2) DEFAULT 0.00,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (nota_fiscal_id) REFERENCES erp_notas_fiscais(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS erp_impostos_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descricao VARCHAR(100) NOT NULL,
    ncm VARCHAR(10) NOT NULL,
    cfop VARCHAR(10) NOT NULL,
    aliquota_icms DECIMAL(18,2) DEFAULT 0.00,
    aliquota_ipi DECIMAL(18,2) DEFAULT 0.00,
    aliquota_pis DECIMAL(18,2) DEFAULT 0.00,
    aliquota_cofins DECIMAL(18,2) DEFAULT 0.00,
    cst_icms VARCHAR(10),
    cst_pis VARCHAR(10),
    cst_cofins VARCHAR(10),
    uf_origem VARCHAR(2),
    uf_destino VARCHAR(2),
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ==================== ESTOQUE AVANÇADO ====================

CREATE TABLE IF NOT EXISTS erp_movimentacoes_estoque (
    id INT AUTO_INCREMENT PRIMARY KEY,
    produto_id INT NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    quantidade DECIMAL(18,3) NOT NULL,
    custo_unitario DECIMAL(18,2),
    custo_total DECIMAL(18,2),
    motivo VARCHAR(50) NOT NULL,
    observacoes TEXT,
    origem_loja_id INT,
    destino_loja_id INT,
    pedido_id INT,
    nota_fiscal_id INT,
    fornecedor_id INT,
    documento_referencia VARCHAR(100) NOT NULL,
    data_movimentacao DATETIME NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_por VARCHAR(100),
    INDEX idx_me_produto_data (produto_id, data_movimentacao)
);

CREATE TABLE IF NOT EXISTS erp_inventarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,
    descricao VARCHAR(200) NOT NULL,
    loja_id INT,
    status VARCHAR(20) NOT NULL DEFAULT 'Aberto',
    data_inicio DATETIME NOT NULL,
    data_fim DATETIME,
    observacoes TEXT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_por VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS erp_itens_inventario (
    id INT AUTO_INCREMENT PRIMARY KEY,
    inventario_id INT NOT NULL,
    produto_id INT NOT NULL,
    quantidade_sistema DECIMAL(18,3) NOT NULL,
    quantidade_contada DECIMAL(18,3) NOT NULL,
    diferenca DECIMAL(18,3) AS (quantidade_contada - quantidade_sistema),
    custo_unitario DECIMAL(18,2),
    valor_diferenca DECIMAL(18,2),
    observacoes VARCHAR(200),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (inventario_id) REFERENCES erp_inventarios(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS erp_kardex (
    id INT AUTO_INCREMENT PRIMARY KEY,
    produto_id INT NOT NULL,
    data DATETIME NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    quantidade DECIMAL(18,3) NOT NULL,
    saldo DECIMAL(18,3) NOT NULL,
    custo_unitario DECIMAL(18,2),
    custo_medio DECIMAL(18,2),
    documento VARCHAR(100) NOT NULL,
    observacoes TEXT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_kardex_produto_data (produto_id, data)
);

CREATE TABLE IF NOT EXISTS erp_locais_estoque (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(500),
    loja_id INT,
    setor VARCHAR(100),
    prateleira VARCHAR(100),
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ==================== CRM ====================

CREATE TABLE IF NOT EXISTS erp_leads_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(200),
    telefone VARCHAR(20),
    whatsapp VARCHAR(20),
    origem VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Novo',
    tipo VARCHAR(50),
    observacoes TEXT,
    empresa VARCHAR(200),
    cargo VARCHAR(100),
    cnpj VARCHAR(20),
    cpf VARCHAR(20),
    responsavel_id INT,
    responsavel_nome VARCHAR(100),
    valor_estimado DECIMAL(18,2),
    probabilidade INT,
    data_previsao_fechamento DATETIME,
    data_ultimo_contato DATETIME,
    data_conversao DATETIME,
    cliente_convertido_id INT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100),
    INDEX idx_leads_status_criado (status, criado_em)
);

CREATE TABLE IF NOT EXISTS erp_interacoes_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    lead_id INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    descricao VARCHAR(1000) NOT NULL,
    data_interacao DATETIME NOT NULL,
    responsavel VARCHAR(100),
    anotacoes TEXT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (lead_id) REFERENCES erp_leads_crm(id) ON DELETE CASCADE,
    INDEX idx_interacoes_lead (lead_id)
);

CREATE TABLE IF NOT EXISTS erp_tarefas_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    descricao TEXT,
    tipo VARCHAR(50) NOT NULL,
    prioridade VARCHAR(20) NOT NULL DEFAULT 'Media',
    status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
    lead_id INT,
    cliente_id INT,
    data_vencimento DATETIME NOT NULL,
    data_conclusao DATETIME,
    responsavel VARCHAR(100),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME
);

-- ==================== FORNECEDORES ====================

CREATE TABLE IF NOT EXISTS erp_fornecedores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    razao_social VARCHAR(200) NOT NULL,
    nome_fantasia VARCHAR(200),
    cnpj VARCHAR(20) NOT NULL UNIQUE,
    inscricao_estadual VARCHAR(20),
    inscricao_municipal VARCHAR(20),
    email VARCHAR(200),
    telefone VARCHAR(20),
    celular VARCHAR(20),
    endereco VARCHAR(200),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    uf VARCHAR(2),
    cep VARCHAR(10),
    segmento VARCHAR(50),
    status VARCHAR(20) DEFAULT 'Ativo',
    limite_credito DECIMAL(18,2),
    prazo_pagamento_dias INT,
    forma_pagamento_preferida VARCHAR(50),
    observacoes TEXT,
    dropshipping TINYINT(1) DEFAULT 0,
    comissao_dropshipping DECIMAL(18,2),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS erp_rh_colaboradores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    cargo VARCHAR(120),
    departamento VARCHAR(120),
    status VARCHAR(50) NOT NULL DEFAULT 'ATIVO',
    data_admissao DATETIME,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    INDEX idx_rh_status_departamento (status, departamento),
    INDEX idx_rh_nome (nome)
);

CREATE TABLE IF NOT EXISTS erp_rh_referencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo VARCHAR(40) NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    descricao VARCHAR(120) NOT NULL,
    ordem INT NOT NULL DEFAULT 0,
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_erp_rh_ref_tipo_codigo (tipo, codigo),
    INDEX idx_erp_rh_ref_tipo_ordem (tipo, ordem, descricao)
);

CREATE TABLE IF NOT EXISTS erp_avaliacoes_fornecedor (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fornecedor_id INT NOT NULL,
    nota INT NOT NULL,
    comentario VARCHAR(500),
    categoria_avaliacao VARCHAR(50),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_por VARCHAR(100),
    FOREIGN KEY (fornecedor_id) REFERENCES erp_fornecedores(id) ON DELETE CASCADE
);

-- ==================== SEED INICIAL ====================

INSERT INTO erp_centros_custo (codigo, nome, descricao, tipo) VALUES
('1', 'Despesas Operacionais', 'Centro sintético de despesas', 'Sintetico'),
('1.01', 'Marketing', 'Campanhas e publicidade', 'Analitico'),
('1.02', 'Logística', 'Frete e armazenagem', 'Analitico'),
('1.03', 'Tecnologia', 'Infraestrutura e software', 'Analitico'),
('2', 'Despesas Administrativas', 'Centro sintético administrativo', 'Sintetico'),
('2.01', 'Pessoal', 'Salários e encargos', 'Analitico'),
('2.02', 'Aluguel', 'Imóveis e condomínio', 'Analitico'),
('3', 'Receitas', 'Centro sintético de receitas', 'Sintetico'),
('3.01', 'Vendas Online', 'E-commerce próprio', 'Analitico'),
('3.02', 'Marketplaces', 'ML, Shopee, Amazon', 'Analitico'),
('3.03', 'Dropshipping', 'Vendas via parceiros', 'Analitico');

INSERT INTO erp_contas_bancarias (nome, banco, agencia, conta, tipo_conta, saldo_inicial) VALUES
('Conta Principal', 'Itaú', '1234', '56789-0', 'Corrente', 0.00),
('Conta Secundária', 'Bradesco', '4321', '98765-0', 'Corrente', 0.00),
('Reserva', 'Nubank', '0001', '12345678-9', 'Poupanca', 0.00);

INSERT INTO erp_financeiro_referencias (tipo, codigo, descricao, ordem, ativo) VALUES
('FORMA_PAGAMENTO', 'PIX', 'PIX', 10, 1),
('FORMA_PAGAMENTO', 'BOLETO', 'Boleto bancario', 20, 1),
('FORMA_PAGAMENTO', 'TED', 'Transferencia TED', 30, 1),
('FORMA_PAGAMENTO', 'DOC', 'Transferencia DOC', 40, 1),
('FORMA_PAGAMENTO', 'DINHEIRO', 'Dinheiro', 50, 1),
('FORMA_PAGAMENTO', 'CARTAO_CREDITO', 'Cartao de credito', 60, 1),
('FORMA_PAGAMENTO', 'CARTAO_DEBITO', 'Cartao de debito', 70, 1),
('BANCO', 'ITAU', 'Itau', 10, 1),
('BANCO', 'BRADESCO', 'Bradesco', 20, 1),
('BANCO', 'SANTANDER', 'Santander', 30, 1),
('BANCO', 'CAIXA', 'Caixa Economica Federal', 40, 1),
('BANCO', 'BANCO_DO_BRASIL', 'Banco do Brasil', 50, 1),
('BANCO', 'NUBANK', 'Nubank', 60, 1),
('STATUS_TITULO', 'PENDENTE', 'Pendente', 10, 1),
('STATUS_TITULO', 'PARCIAL', 'Parcial', 20, 1),
('STATUS_TITULO', 'PAGO', 'Pago', 30, 1),
('STATUS_TITULO', 'RECEBIDO', 'Recebido', 40, 1),
('STATUS_TITULO', 'VENCIDO', 'Vencido', 50, 1)
ON DUPLICATE KEY UPDATE
    descricao = VALUES(descricao),
    ordem = VALUES(ordem),
    ativo = VALUES(ativo);

INSERT INTO erp_rh_referencias (tipo, codigo, descricao, ordem, ativo) VALUES
('DEPARTAMENTO', 'RH', 'Recursos Humanos', 10, 1),
('DEPARTAMENTO', 'DP', 'Departamento Pessoal', 20, 1),
('DEPARTAMENTO', 'FINANCEIRO', 'Financeiro', 30, 1),
('DEPARTAMENTO', 'COMERCIAL', 'Comercial', 40, 1),
('DEPARTAMENTO', 'OPERACOES', 'Operacoes', 50, 1),
('DEPARTAMENTO', 'TI', 'Tecnologia da Informacao', 60, 1),
('CARGO', 'ASSISTENTE_ADMINISTRATIVO', 'Assistente Administrativo', 10, 1),
('CARGO', 'ANALISTA_RH', 'Analista de RH', 20, 1),
('CARGO', 'ASSISTENTE_DP', 'Assistente de DP', 30, 1),
('CARGO', 'ANALISTA_FINANCEIRO', 'Analista Financeiro', 40, 1),
('CARGO', 'VENDEDOR', 'Vendedor', 50, 1),
('STATUS_COLABORADOR', 'ATIVO', 'Ativo', 10, 1),
('STATUS_COLABORADOR', 'AFASTADO', 'Afastado', 20, 1),
('STATUS_COLABORADOR', 'FERIAS_PROGRAMADAS', 'Ferias Programadas', 30, 1),
('STATUS_COLABORADOR', 'FERIAS', 'Ferias', 40, 1),
('STATUS_COLABORADOR', 'DESLIGADO', 'Desligado', 50, 1),
('STATUS_COLABORADOR', 'INATIVO', 'Inativo', 60, 1)
ON DUPLICATE KEY UPDATE
    descricao = VALUES(descricao),
    ordem = VALUES(ordem),
    ativo = VALUES(ativo);

INSERT INTO erp_locais_estoque (codigo, nome, descricao, setor, prateleira) VALUES
('CD-GERAL', 'Centro de Distribuição Geral', 'Armazém principal do grupo', 'Recebimento', 'A-01'),
('LOJA-01', 'Estoque Grann-Tur', 'Estoque da loja Grann-Tur', 'Vendas', 'B-01'),
('LOJA-02', 'Estoque Chronos', 'Estoque da loja Chronos', 'Vendas', 'B-02'),
('LOJA-03', 'Estoque Moda Mim', 'Estoque da loja Moda Mim', 'Vendas', 'B-03'),
('LOJA-04', 'Estoque Geração Top+', 'Estoque da loja Geração Top+', 'Vendas', 'B-04'),
('LOJA-05', 'Estoque Estruturaline', 'Estoque da loja Estruturaline', 'Vendas', 'B-05'),
('LOJA-06', 'Estoque Gran-fest-festas', 'Estoque da loja Gran-fest-festas', 'Vendas', 'B-06');

-- ==================== VIEWS DO ERP ====================

CREATE OR REPLACE VIEW v_resumo_financeiro AS
SELECT
    (SELECT COALESCE(SUM(valor_original - valor_pago - valor_desconto + valor_multa + valor_juros), 0)
     FROM erp_contas_pagar WHERE status = 'Pendente' AND DATE(data_vencimento) = CURDATE()) AS total_pagar_hoje,
    (SELECT COALESCE(SUM(valor_original - valor_pago - valor_desconto + valor_multa + valor_juros), 0)
     FROM erp_contas_pagar WHERE status = 'Pendente' AND DATE(data_vencimento) < CURDATE()) AS total_pagar_atrasado,
    (SELECT COALESCE(SUM(valor_original - valor_recebido - valor_desconto + valor_multa + valor_juros), 0)
     FROM erp_contas_receber WHERE status = 'Pendente' AND DATE(data_vencimento) = CURDATE()) AS total_receber_hoje,
    (SELECT COALESCE(SUM(valor_original - valor_recebido - valor_desconto + valor_multa + valor_juros), 0)
     FROM erp_contas_receber WHERE status = 'Pendente' AND DATE(data_vencimento) < CURDATE()) AS total_receber_atrasado,
    (SELECT COALESCE(SUM(saldo_atual), 0) FROM erp_contas_bancarias WHERE ativo = 1) AS saldo_caixa;

CREATE OR REPLACE VIEW v_pipeline_crm AS
SELECT
    status,
    COUNT(*) AS quantidade,
    COALESCE(SUM(valor_estimado), 0) AS valor_total,
    COALESCE(AVG(probabilidade), 0) AS probabilidade_media
FROM erp_leads_crm
WHERE status NOT IN ('Convertido', 'Perdido')
GROUP BY status;

CREATE OR REPLACE VIEW v_estoque_baixo_erp AS
SELECT
    p.id,
    p.nome,
    p.sku,
    p.estoque_atual,
    p.estoque_minimo,
    (p.estoque_minimo - p.estoque_atual) AS deficit,
    l.nome AS loja_nome
FROM produtos p
LEFT JOIN lojas l ON p.loja_id = l.id
WHERE p.estoque_atual <= p.estoque_minimo
ORDER BY deficit DESC;

-- ==================== STORED PROCEDURES ====================

DELIMITER //

CREATE PROCEDURE IF NOT EXISTS sp_gerar_dre(
    IN p_data_inicio DATE,
    IN p_data_fim DATE
)
BEGIN
    SELECT
        CONCAT(DATE_FORMAT(p_data_inicio, '%m/%Y'), ' a ', DATE_FORMAT(p_data_fim, '%m/%Y')) AS periodo,
        COALESCE(SUM(CASE WHEN tipo = 'Entrada' THEN valor ELSE 0 END), 0) AS receita_bruta,
        COALESCE(SUM(CASE WHEN tipo = 'Entrada' THEN valor ELSE 0 END), 0) * 0.12 AS impostos,
        COALESCE(SUM(CASE WHEN tipo = 'Saida' AND categoria = 'Custo Produtos' THEN valor ELSE 0 END), 0) AS custo_produtos,
        COALESCE(SUM(CASE WHEN tipo = 'Saida' AND categoria IN ('Despesas Operacionais', 'Despesas Administrativas') THEN valor ELSE 0 END), 0) AS despesas_operacionais,
        COALESCE(SUM(CASE WHEN tipo = 'Saida' AND categoria = 'Despesas Financeiras' THEN valor ELSE 0 END), 0) AS despesas_financeiras,
        COALESCE(SUM(CASE WHEN tipo = 'Entrada' AND categoria = 'Receitas Financeiras' THEN valor ELSE 0 END), 0) AS receitas_financeiras
    FROM erp_fluxo_caixa
    WHERE DATE(data) BETWEEN p_data_inicio AND p_data_fim;
END //

CREATE PROCEDURE IF NOT EXISTS sp_atualizar_saldo_conta(
    IN p_conta_id INT,
    IN p_valor DECIMAL(18,2),
    IN p_tipo VARCHAR(20)
)
BEGIN
    IF p_tipo = 'Entrada' THEN
        UPDATE erp_contas_bancarias SET saldo_atual = saldo_atual + p_valor WHERE id = p_conta_id;
    ELSEIF p_tipo = 'Saida' THEN
        UPDATE erp_contas_bancarias SET saldo_atual = saldo_atual - p_valor WHERE id = p_conta_id;
    END IF;
END //

CREATE PROCEDURE IF NOT EXISTS sp_recalcular_custo_medio(
    IN p_produto_id INT
)
BEGIN
    DECLARE v_custo_total DECIMAL(18,2);
    DECLARE v_quantidade_total DECIMAL(18,3);

    SELECT COALESCE(SUM(custo_total), 0), COALESCE(SUM(quantidade), 0)
    INTO v_custo_total, v_quantidade_total
    FROM erp_movimentacoes_estoque
    WHERE produto_id = p_produto_id AND tipo = 'Entrada';

    IF v_quantidade_total > 0 THEN
        UPDATE produtos
        SET custo_medio = v_custo_total / v_quantidade_total
        WHERE id = p_produto_id;
    END IF;
END //

DELIMITER ;

-- ==================== TRIGGERS ====================

DELIMITER //

CREATE TRIGGER IF NOT EXISTS trg_atualiza_status_conta_pagar
BEFORE UPDATE ON erp_contas_pagar
FOR EACH ROW
BEGIN
    IF NEW.valor_pago >= (NEW.valor_original - NEW.valor_desconto + NEW.valor_multa + NEW.valor_juros) THEN
        SET NEW.status = 'Pago';
    ELSEIF NEW.data_vencimento < CURDATE() AND NEW.status != 'Pago' THEN
        SET NEW.status = 'Atrasado';
    END IF;
    SET NEW.atualizado_em = NOW();
END //

CREATE TRIGGER IF NOT EXISTS trg_atualiza_status_conta_receber
BEFORE UPDATE ON erp_contas_receber
FOR EACH ROW
BEGIN
    IF NEW.valor_recebido >= (NEW.valor_original - NEW.valor_desconto + NEW.valor_multa + NEW.valor_juros) THEN
        SET NEW.status = 'Recebido';
    ELSEIF NEW.data_vencimento < CURDATE() AND NEW.status != 'Recebido' THEN
        SET NEW.status = 'Atrasado';
    END IF;
    SET NEW.atualizado_em = NOW();
END //

CREATE TRIGGER IF NOT EXISTS trg_auditoria_movimentacao_estoque
AFTER INSERT ON erp_movimentacoes_estoque
FOR EACH ROW
BEGIN
    INSERT INTO auditoria (tabela, operacao, registro_id, dados_novos, data_operacao, usuario)
    VALUES ('erp_movimentacoes_estoque', 'INSERT', NEW.id,
            CONCAT('Produto:', NEW.produto_id, ' Tipo:', NEW.tipo, ' Qtd:', NEW.quantidade),
            NOW(), NEW.criado_por);
END //

DELIMITER ;

-- ============================================================
-- FIM DO SCRIPT DE ATUALIZAÇÃO FASE 5
-- Execute: mysql -h 192.168.1.72 -P 3309 -u root -p nexum_altivon < erp_schema_update.sql
-- ============================================================
