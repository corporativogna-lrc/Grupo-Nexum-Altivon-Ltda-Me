-- =====================================================
-- ATUALIZAÇÃO DO BANCO NEXUM_ALTIVON — FASE 5 ERP/CRM
-- GenesisGest.Net — Grupo Nexum Altivon
-- Servidor: 192.168.1.72:3309
-- =====================================================

USE nexum_altivon;

-- =====================================================
-- TABELAS FINANCEIRO
-- =====================================================

CREATE TABLE IF NOT EXISTS erp_centros_custo (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(500),
    pai_id INT,
    tipo VARCHAR(20) DEFAULT 'Sintetico',
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
    saldo_atual DECIMAL(18,2) DEFAULT 0,
    saldo_inicial DECIMAL(18,2) DEFAULT 0,
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS erp_contas_pagar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero_documento VARCHAR(20) NOT NULL,
    fornecedor_id INT NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    valor_original DECIMAL(18,2) NOT NULL,
    valor_pago DECIMAL(18,2) DEFAULT 0,
    valor_multa DECIMAL(18,2) DEFAULT 0,
    valor_juros DECIMAL(18,2) DEFAULT 0,
    valor_desconto DECIMAL(18,2) DEFAULT 0,
    data_emissao DATETIME NOT NULL,
    data_vencimento DATETIME NOT NULL,
    data_pagamento DATETIME,
    status VARCHAR(20) DEFAULT 'Pendente',
    forma_pagamento VARCHAR(50),
    numero_boleto VARCHAR(100),
    observacoes TEXT,
    loja_id INT,
    centro_custo_id INT NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100),
    INDEX idx_status_vencimento (status, data_vencimento),
    INDEX idx_fornecedor (fornecedor_id)
);

CREATE TABLE IF NOT EXISTS erp_contas_receber (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero_documento VARCHAR(20) NOT NULL,
    cliente_id INT NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    valor_original DECIMAL(18,2) NOT NULL,
    valor_recebido DECIMAL(18,2) DEFAULT 0,
    valor_multa DECIMAL(18,2) DEFAULT 0,
    valor_juros DECIMAL(18,2) DEFAULT 0,
    valor_desconto DECIMAL(18,2) DEFAULT 0,
    data_emissao DATETIME NOT NULL,
    data_vencimento DATETIME NOT NULL,
    data_recebimento DATETIME,
    status VARCHAR(20) DEFAULT 'Pendente',
    forma_recebimento VARCHAR(50),
    numero_pedido_referencia VARCHAR(100),
    observacoes TEXT,
    loja_id INT,
    centro_custo_id INT NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    criado_por VARCHAR(100),
    INDEX idx_status_vencimento (status, data_vencimento),
    INDEX idx_cliente (cliente_id)
);

CREATE TABLE IF NOT EXISTS erp_fluxo_caixa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    data DATETIME NOT NULL,
    tipo VARCHAR(50) NOT NULL,
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
    INDEX idx_data_tipo (data, tipo)
);

-- =====================================================
-- TABELAS FISCAL
-- =====================================================

CREATE TABLE IF NOT EXISTS erp_notas_fiscais (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero VARCHAR(20) NOT NULL,
    serie VARCHAR(10) DEFAULT '1',
    tipo VARCHAR(10) NOT NULL,
    natureza_operacao VARCHAR(20) NOT NULL,
    emitente_id INT NOT NULL,
    destinatario_id INT NOT NULL,
    valor_total DECIMAL(18,2) NOT NULL,
    valor_icms DECIMAL(18,2) DEFAULT 0,
    valor_ipi DECIMAL(18,2) DEFAULT 0,
    valor_pis DECIMAL(18,2) DEFAULT 0,
    valor_cofins DECIMAL(18,2) DEFAULT 0,
    valor_frete DECIMAL(18,2) DEFAULT 0,
    valor_seguro DECIMAL(18,2) DEFAULT 0,
    valor_desconto DECIMAL(18,2) DEFAULT 0,
    data_emissao DATETIME NOT NULL,
    data_saida_entrada DATETIME,
    status VARCHAR(50) DEFAULT 'Emitida',
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
    valor_icms DECIMAL(18,2) DEFAULT 0,
    aliquota_icms DECIMAL(18,2) DEFAULT 0,
    base_calculo_icms DECIMAL(18,2) DEFAULT 0,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (nota_fiscal_id) REFERENCES erp_notas_fiscais(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS erp_impostos_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descricao VARCHAR(100) NOT NULL,
    ncm VARCHAR(10) NOT NULL,
    cfop VARCHAR(10) NOT NULL,
    aliquota_icms DECIMAL(18,2) DEFAULT 0,
    aliquota_ipi DECIMAL(18,2) DEFAULT 0,
    aliquota_pis DECIMAL(18,2) DEFAULT 0,
    aliquota_cofins DECIMAL(18,2) DEFAULT 0,
    cst_icms VARCHAR(10),
    cst_pis VARCHAR(10),
    cst_cofins VARCHAR(10),
    uf_origem VARCHAR(2),
    uf_destino VARCHAR(2),
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- TABELAS ESTOQUE AVANÇADO
-- =====================================================

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
    INDEX idx_produto_data (produto_id, data_movimentacao),
    INDEX idx_tipo (tipo)
);

CREATE TABLE IF NOT EXISTS erp_inventarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,
    descricao VARCHAR(200) NOT NULL,
    loja_id INT,
    status VARCHAR(20) DEFAULT 'Aberto',
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
    custo_unitario DECIMAL(18,2),
    valor_diferenca DECIMAL(18,2),
    observacoes TEXT,
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
    INDEX idx_produto_data (produto_id, data)
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

-- =====================================================
-- TABELAS CRM
-- =====================================================

CREATE TABLE IF NOT EXISTS erp_leads_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(200),
    telefone VARCHAR(20),
    whatsapp VARCHAR(20),
    origem VARCHAR(50) NOT NULL,
    status VARCHAR(50) DEFAULT 'Novo',
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
    INDEX idx_status_criado (status, criado_em),
    INDEX idx_origem (origem)
);

CREATE TABLE IF NOT EXISTS erp_interacoes_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    lead_id INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    descricao TEXT NOT NULL,
    data_interacao DATETIME NOT NULL,
    responsavel VARCHAR(100),
    anotacoes TEXT,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_lead (lead_id),
    FOREIGN KEY (lead_id) REFERENCES erp_leads_crm(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS erp_tarefas_crm (
    id INT AUTO_INCREMENT PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    descricao TEXT,
    tipo VARCHAR(50) NOT NULL,
    prioridade VARCHAR(20) DEFAULT 'Media',
    status VARCHAR(20) DEFAULT 'Pendente',
    lead_id INT,
    cliente_id INT,
    data_vencimento DATETIME NOT NULL,
    data_conclusao DATETIME,
    responsavel VARCHAR(100),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME,
    INDEX idx_status_vencimento (status, data_vencimento)
);

-- =====================================================
-- TABELAS FORNECEDORES
-- =====================================================

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

CREATE TABLE IF NOT EXISTS erp_avaliacoes_fornecedor (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fornecedor_id INT NOT NULL,
    nota INT NOT NULL CHECK (nota BETWEEN 1 AND 5),
    comentario TEXT,
    categoria_avaliacao VARCHAR(50),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_por VARCHAR(100),
    FOREIGN KEY (fornecedor_id) REFERENCES erp_fornecedores(id) ON DELETE CASCADE
);

-- =====================================================
-- SEED DE DADOS INICIAIS
-- =====================================================

-- Centros de Custo
INSERT INTO erp_centros_custo (codigo, nome, descricao, tipo) VALUES
('1', 'Receitas', 'Todas as receitas do grupo', 'Sintetico'),
('1.1', 'Vendas Online', 'Receitas de vendas e-commerce', 'Analitico'),
('1.2', 'Vendas Marketplace', 'Receitas de marketplaces', 'Analitico'),
('2', 'Despesas', 'Todas as despesas operacionais', 'Sintetico'),
('2.1', 'Marketing', 'Investimentos em marketing digital', 'Analitico'),
('2.2', 'Logística', 'Frete e armazenagem', 'Analitico'),
('2.3', 'Administrativo', 'Despesas administrativas', 'Analitico'),
('3', 'Custo Mercadoria', 'CMV das lojas', 'Sintetico'),
('3.1', 'CMV Grann-Tur', 'Custo das mercadorias vendidas', 'Analitico'),
('3.2', 'CMV Chronos', 'Custo das mercadorias vendidas', 'Analitico'),
('3.3', 'CMV Moda Mim', 'Custo das mercadorias vendidas', 'Analitico'),
('3.4', 'CMV Geração Top+', 'Custo das mercadorias vendidas', 'Analitico'),
('3.5', 'CMV Estruturaline', 'Custo das mercadorias vendidas', 'Analitico'),
('3.6', 'CMV Gran-fest-festas', 'Custo das mercadorias vendidas', 'Analitico')
ON DUPLICATE KEY UPDATE nome = VALUES(nome);

-- Contas Bancárias
INSERT INTO erp_contas_bancarias (nome, banco, agencia, conta, tipo_conta, saldo_inicial) VALUES
('Conta Principal Bradesco', 'Bradesco', '1234', '56789-0', 'Corrente', 0),
('Conta Secundária Itaú', 'Itaú', '5678', '12345-6', 'Corrente', 0),
('Poupança Reserva', 'Bradesco', '1234', '99999-9', 'Poupanca', 0)
ON DUPLICATE KEY UPDATE nome = VALUES(nome);

-- Locais de Estoque
INSERT INTO erp_locais_estoque (codigo, nome, descricao, setor, prateleira) VALUES
('CD-01', 'Centro de Distribuição Principal', 'Armazém principal do grupo', 'Recebimento', 'A-01'),
('LOJA-GT', 'Estoque Grann-Tur', 'Estoque da loja Grann-Tur', 'Vendas', 'GT-01'),
('LOJA-CH', 'Estoque Chronos', 'Estoque da loja Chronos', 'Vendas', 'CH-01'),
('LOJA-MM', 'Estoque Moda Mim', 'Estoque da loja Moda Mim', 'Vendas', 'MM-01'),
('LOJA-GT+', 'Estoque Geração Top+', 'Estoque da loja Geração Top+', 'Vendas', 'GT+-01'),
('LOJA-ES', 'Estoque Estruturaline', 'Estoque da loja Estruturaline', 'Vendas', 'ES-01'),
('LOJA-GF', 'Estoque Gran-fest-festas', 'Estoque da loja Gran-fest-festas', 'Vendas', 'GF-01')
ON DUPLICATE KEY UPDATE nome = VALUES(nome);

-- Configurações Fiscais de Exemplo
INSERT INTO erp_impostos_config (descricao, ncm, cfop, aliquota_icms, aliquota_ipi, aliquota_pis, aliquota_cofins, cst_icms, cst_pis, cst_cofins, uf_origem, uf_destino) VALUES
('Venda SP para SP — Relógios', '9101.11.00', '5102', 18.00, 0.00, 0.65, 3.00, '00', '01', '01', 'SP', 'SP'),
('Venda SP para SP — Eletrônicos', '8517.12.00', '5102', 18.00, 0.00, 0.65, 3.00, '00', '01', '01', 'SP', 'SP'),
('Venda SP para RJ — Geral', '9999.99.99', '6102', 12.00, 0.00, 0.65, 3.00, '00', '01', '01', 'SP', 'RJ'),
('Venda SP para MG — Geral', '9999.99.99', '6102', 7.00, 0.00, 0.65, 3.00, '00', '01', '01', 'SP', 'MG')
ON DUPLICATE KEY UPDATE descricao = VALUES(descricao);

-- =====================================================
-- VIEWS PARA DASHBOARD ERP
-- =====================================================

CREATE OR REPLACE VIEW v_resumo_financeiro AS
SELECT
    (SELECT COALESCE(SUM(valor_original - valor_pago), 0) FROM erp_contas_pagar WHERE status = 'Pendente' AND DATE(data_vencimento) = CURDATE()) AS total_pagar_hoje,
    (SELECT COALESCE(SUM(valor_original - valor_pago), 0) FROM erp_contas_pagar WHERE status = 'Pendente' AND DATE(data_vencimento) < CURDATE()) AS total_pagar_atrasado,
    (SELECT COALESCE(SUM(valor_original - valor_pago), 0) FROM erp_contas_pagar WHERE status = 'Pendente' AND DATE(data_vencimento) BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 7 DAY)) AS total_pagar_7dias,
    (SELECT COALESCE(SUM(valor_original - valor_recebido), 0) FROM erp_contas_receber WHERE status = 'Pendente' AND DATE(data_vencimento) = CURDATE()) AS total_receber_hoje,
    (SELECT COALESCE(SUM(valor_original - valor_recebido), 0) FROM erp_contas_receber WHERE status = 'Pendente' AND DATE(data_vencimento) < CURDATE()) AS total_receber_atrasado,
    (SELECT COALESCE(SUM(valor_original - valor_recebido), 0) FROM erp_contas_receber WHERE status = 'Pendente' AND DATE(data_vencimento) BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 7 DAY)) AS total_receber_7dias,
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
    (p.estoque_minimo - p.estoque_atual) AS quantidade_faltante,
    l.nome AS loja_nome
FROM produtos p
LEFT JOIN lojas l ON p.loja_id = l.id
WHERE p.estoque_atual <= p.estoque_minimo
AND p.ativo = 1;

-- =====================================================
-- PROCEDURES
-- =====================================================

DELIMITER //

CREATE OR REPLACE PROCEDURE sp_gerar_numero_documento(
    IN p_tipo VARCHAR(20),
    OUT p_numero VARCHAR(20)
)
BEGIN
    SET p_numero = CONCAT(
        UPPER(p_tipo), '-',
        DATE_FORMAT(NOW(), '%Y%m%d'), '-',
        LPAD(FLOOR(RAND() * 9999), 4, '0')
    );
END //

CREATE OR REPLACE PROCEDURE sp_atualizar_saldo_conta(
    IN p_conta_id INT,
    IN p_valor DECIMAL(18,2),
    IN p_tipo VARCHAR(10)
)
BEGIN
    IF p_tipo = 'ENTRADA' THEN
        UPDATE erp_contas_bancarias SET saldo_atual = saldo_atual + p_valor WHERE id = p_conta_id;
    ELSEIF p_tipo = 'SAIDA' THEN
        UPDATE erp_contas_bancarias SET saldo_atual = saldo_atual - p_valor WHERE id = p_conta_id;
    END IF;
END //

CREATE OR REPLACE PROCEDURE sp_relatorio_dre(
    IN p_data_inicio DATE,
    IN p_data_fim DATE
)
BEGIN
    SELECT
        'Receita Bruta' AS item,
        COALESCE(SUM(CASE WHEN tipo = 'Entrada' THEN valor ELSE 0 END), 0) AS valor
    FROM erp_fluxo_caixa
    WHERE DATE(data) BETWEEN p_data_inicio AND p_data_fim
    UNION ALL
    SELECT
        'Despesas Operacionais' AS item,
        COALESCE(SUM(CASE WHEN tipo = 'Saida' AND categoria != 'Financeira' THEN valor ELSE 0 END), 0) * -1 AS valor
    FROM erp_fluxo_caixa
    WHERE DATE(data) BETWEEN p_data_inicio AND p_data_fim;
END //

DELIMITER ;

-- =====================================================
-- TRIGGERS DE AUDITORIA
-- =====================================================

DELIMITER //

CREATE OR REPLACE TRIGGER trg_auditoria_conta_pagar
AFTER UPDATE ON erp_contas_pagar
FOR EACH ROW
BEGIN
    IF OLD.status != NEW.status THEN
        INSERT INTO log_auditoria (tabela, registro_id, acao, campo, valor_anterior, valor_novo, data_hora, usuario)
        VALUES ('erp_contas_pagar', NEW.id, 'UPDATE', 'status', OLD.status, NEW.status, NOW(), NEW.criado_por);
    END IF;
END //

CREATE OR REPLACE TRIGGER trg_auditoria_lead_crm
AFTER UPDATE ON erp_leads_crm
FOR EACH ROW
BEGIN
    IF OLD.status != NEW.status THEN
        INSERT INTO log_auditoria (tabela, registro_id, acao, campo, valor_anterior, valor_novo, data_hora, usuario)
        VALUES ('erp_leads_crm', NEW.id, 'UPDATE', 'status', OLD.status, NEW.status, NOW(), NEW.criado_por);
    END IF;
END //

DELIMITER ;

-- =====================================================
-- FIM DO SCRIPT
-- =====================================================
