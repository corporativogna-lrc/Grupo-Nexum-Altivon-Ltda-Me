/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

-- GenesisGest.Net v1.1.5
-- Base extraida de genesis_bd_Include.sql em modo seguro.
-- Esta migracao cria a estrutura original ausente sem apagar, renomear ou sobrescrever as tabelas operacionais atuais.
-- Inserts, procedures, triggers e FKs do dump original ficam fora deste primeiro passo para evitar quebra por dados/ordem de dependencias.

CREATE TABLE IF NOT EXISTS `adm_auditoria` (
  `aud_id` bigint(20) NOT NULL COMMENT 'Identificador único do Log de Auditoria.',
  `aud_usr_id` int(11) NOT NULL COMMENT 'ID do usuário que executou a ação (FK: adm_usuarios).',
  `aud_modulo` varchar(50) NOT NULL COMMENT 'Módulo onde a ação ocorreu.',
  `aud_tabela` varchar(100) NOT NULL COMMENT 'Tabela afetada pela ação.',
  `aud_registro_id` int(11) DEFAULT NULL COMMENT 'ID do registro afetado (opcional).',
  `aud_acao` enum('INSERT','UPDATE','DELETE','SELECT') NOT NULL COMMENT 'Tipo de ação executada.',
  `aud_dados_anteriores` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Dados do registro antes da ação (especialmente para UPDATE e DELETE).' CHECK (json_valid(`aud_dados_anteriores`)),
  `aud_dados_novos` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Dados do registro após a ação (especialmente para INSERT e UPDATE).' CHECK (json_valid(`aud_dados_novos`)),
  `aud_ip` varchar(45) DEFAULT NULL COMMENT 'Endereço IP do cliente que executou a ação.',
  `aud_data_hora` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data e hora exata da ação.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro de todas as operações críticas do sistema para fins de auditoria e segurança.';

CREATE TABLE IF NOT EXISTS `adm_contatos` (
  `con_id` int(11) NOT NULL COMMENT 'Identificador único do Contato.',
  `con_pes_id` int(11) NOT NULL COMMENT 'ID da Pessoa à qual o contato pertence (FK: adm_pessoas).',
  `con_nome` varchar(100) NOT NULL COMMENT 'Nome da pessoa de contato.',
  `con_cargo` varchar(100) DEFAULT NULL COMMENT 'Cargo ou função do contato.',
  `con_departamento` varchar(100) DEFAULT NULL COMMENT 'Departamento do contato.',
  `con_telefone` varchar(20) DEFAULT NULL COMMENT 'Telefone comercial.',
  `con_celular` varchar(20) DEFAULT NULL COMMENT 'Celular/WhatsApp do contato.',
  `con_email` varchar(100) DEFAULT NULL COMMENT 'E-mail do contato.',
  `con_principal` tinyint(1) DEFAULT 0 COMMENT 'Indica se este é o contato principal (TRUE).',
  `con_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade do contato.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Lista de contatos (e-mails, telefones) de uma Pessoa (Cliente/Fornecedor).';

CREATE TABLE IF NOT EXISTS `adm_empresas` (
  `emp_id` int(11) NOT NULL COMMENT 'Identificador único da Empresa/Filial.',
  `emp_razao_social` varchar(200) NOT NULL COMMENT 'Nome de registro legal da empresa.',
  `emp_nome_fantasia` varchar(200) DEFAULT NULL COMMENT 'Nome comercial da empresa.',
  `emp_cnpj` varchar(18) NOT NULL COMMENT 'Cadastro Nacional da Pessoa Jurídica (ex: 00.000.000/0000-00).',
  `emp_inscricao_estadual` varchar(20) DEFAULT NULL COMMENT 'Inscrição Estadual (IE) da empresa.',
  `emp_inscricao_municipal` varchar(20) DEFAULT NULL COMMENT 'Inscrição Municipal (IM) da empresa.',
  `emp_endereco` varchar(200) DEFAULT NULL COMMENT 'Nome da rua/avenida.',
  `emp_numero` varchar(10) DEFAULT NULL COMMENT 'Número do endereço.',
  `emp_complemento` varchar(100) DEFAULT NULL COMMENT 'Complemento do endereço (ex: Sala, Andar).',
  `emp_bairro` varchar(100) DEFAULT NULL COMMENT 'Bairro do endereço.',
  `emp_cidade` varchar(100) DEFAULT NULL COMMENT 'Cidade do endereço.',
  `emp_uf` char(2) DEFAULT NULL COMMENT 'Unidade Federativa (UF) - Ex: SP.',
  `emp_cep` varchar(9) DEFAULT NULL COMMENT 'Código de Endereçamento Postal (ex: 00000-000).',
  `emp_telefone` varchar(20) DEFAULT NULL COMMENT 'Telefone principal de contato.',
  `emp_email` varchar(100) DEFAULT NULL COMMENT 'E-mail de contato principal.',
  `emp_matriz` tinyint(1) DEFAULT 0 COMMENT 'Indica se a empresa é a Matriz (TRUE) ou Filial (FALSE).',
  `emp_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade no sistema.',
  `emp_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data e hora da criação do registro.',
  `emp_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Data e hora da última alteração do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de todas as empresas e filiais que usarão o sistema ERP.';

CREATE TABLE IF NOT EXISTS `adm_perfil_permissoes` (
  `ppr_id` int(11) NOT NULL COMMENT 'Identificador único da Permissão por Perfil.',
  `ppr_perfil_id` int(11) NOT NULL COMMENT 'ID do perfil (FK: adm_perfis).',
  `ppr_permissao_id` int(11) NOT NULL COMMENT 'ID da permissão (FK: adm_permissoes).',
  `ppr_leitura` tinyint(1) DEFAULT 0 COMMENT 'Permissão para visualizar/ler dados.',
  `ppr_escrita` tinyint(1) DEFAULT 0 COMMENT 'Permissão para criar/alterar dados.',
  `ppr_exclusao` tinyint(1) DEFAULT 0 COMMENT 'Permissão para deletar dados.',
  `ppr_impressao` tinyint(1) DEFAULT 0 COMMENT 'Permissão para imprimir relatórios/documentos.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Associa as permissões específicas a cada perfil.';

CREATE TABLE IF NOT EXISTS `adm_perfis` (
  `prf_id` int(11) NOT NULL COMMENT 'Identificador único do Perfil de Acesso.',
  `prf_nome` varchar(100) NOT NULL COMMENT 'Nome do Perfil (Ex: Administrador, Vendedor, Financeiro).',
  `prf_descricao` text DEFAULT NULL COMMENT 'Descrição detalhada das responsabilidades do perfil.',
  `prf_alcada_maxima` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor máximo que o perfil pode aprovar sem hierarquia superior.',
  `prf_nivel_hierarquico` int(11) DEFAULT 1 COMMENT 'Nível hierárquico para aprovações (1=Mais alto).',
  `prf_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade do perfil.',
  `prf_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do perfil.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Define os papéis e níveis de segurança dos usuários.';

CREATE TABLE IF NOT EXISTS `adm_permissoes` (
  `prm_id` int(11) NOT NULL COMMENT 'Identificador único da Permissão.',
  `prm_modulo` varchar(50) NOT NULL COMMENT 'Módulo do sistema a que a permissão pertence (Ex: FINANCEIRO, ESTOQUE).',
  `prm_funcionalidade` varchar(100) NOT NULL COMMENT 'Funcionalidade específica (Ex: Cadastro de Produtos, Emissão de NF).',
  `prm_chave` varchar(100) NOT NULL COMMENT 'Chave de código única para referência no sistema (Ex: EST_CAD_PRODUTO_VIEW).',
  `prm_descricao` text DEFAULT NULL COMMENT 'Descrição do que esta permissão permite fazer.',
  `prm_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade da permissão.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Catálogo de todas as permissões/funcionalidades do sistema.';

CREATE TABLE IF NOT EXISTS `adm_pessoas_empresas` (
  `pes_id` int(11) NOT NULL COMMENT 'Identificador único da Pessoa.',
  `pes_tipo` enum('FISICA','JURIDICA') NOT NULL COMMENT 'Tipo de pessoa (Física ou Jurídica).',
  `pes_nome_razao` varchar(200) NOT NULL COMMENT 'Nome completo (Pessoa Física) ou Razão Social (Pessoa Jurídica).',
  `pes_nome_fantasia` varchar(200) DEFAULT NULL COMMENT 'Nome Fantasia (Pessoa Jurídica) ou Apelido (Pessoa Física).',
  `pes_cpf_cnpj` varchar(18) DEFAULT NULL COMMENT 'CPF ou CNPJ.',
  `pes_rg_ie` varchar(20) DEFAULT NULL COMMENT 'RG (Física) ou Inscrição Estadual (Jurídica).',
  `pes_cliente` tinyint(1) DEFAULT 0 COMMENT 'Indica se a pessoa é um Cliente.',
  `pes_fornecedor` tinyint(1) DEFAULT 0 COMMENT 'Indica se a pessoa é um Fornecedor.',
  `pes_colaborador` tinyint(1) DEFAULT 0 COMMENT 'Indica se a pessoa é um Colaborador (funcionário).',
  `pes_transportadora` tinyint(1) DEFAULT 0 COMMENT 'Indica se a pessoa é uma Transportadora.',
  `pes_endereco` varchar(200) DEFAULT NULL COMMENT 'Endereço principal.',
  `pes_numero` varchar(10) DEFAULT NULL COMMENT 'Número do endereço.',
  `pes_complemento` varchar(100) DEFAULT NULL COMMENT 'Complemento do endereço.',
  `pes_bairro` varchar(100) DEFAULT NULL COMMENT 'Bairro do endereço.',
  `pes_cidade` varchar(100) DEFAULT NULL COMMENT 'Cidade.',
  `pes_uf` char(2) DEFAULT NULL COMMENT 'Unidade Federativa (UF).',
  `pes_cep` varchar(9) DEFAULT NULL COMMENT 'CEP.',
  `pes_telefone` varchar(20) DEFAULT NULL COMMENT 'Telefone fixo.',
  `pes_celular` varchar(20) DEFAULT NULL COMMENT 'Telefone celular/WhatsApp.',
  `pes_email` varchar(100) DEFAULT NULL COMMENT 'E-mail de contato.',
  `pes_site` varchar(100) DEFAULT NULL COMMENT 'Website da pessoa/empresa.',
  `pes_observacoes` text DEFAULT NULL COMMENT 'Notas e observações gerais.',
  `pes_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade no sistema.',
  `pes_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data e hora da criação do registro.',
  `pes_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Data e hora da última alteração do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro Mestre Único para todas as Pessoas (Clientes, Fornecedores, Colaboradores, etc.).';

CREATE TABLE IF NOT EXISTS `adm_usuarios` (
  `usr_id` int(11) NOT NULL COMMENT 'Identificador único do Usuário.',
  `usr_nome` varchar(100) NOT NULL COMMENT 'Nome completo do usuário.',
  `usr_login` varchar(50) NOT NULL COMMENT 'Login de acesso ao sistema.',
  `usr_email` varchar(100) NOT NULL COMMENT 'E-mail principal, usado para recuperação de senha.',
  `usr_senha` varchar(255) NOT NULL COMMENT 'Senha criptografada do usuário.',
  `usr_perfil_id` int(11) DEFAULT NULL COMMENT 'ID do perfil de acesso ("FK: adm_perfis").',
  `usr_emp_id` int(11) DEFAULT NULL COMMENT 'ID da empresa/filial principal do usuário ("FK: adm_empresas").',
  `usr_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade no sistema.',
  `usr_ultimo_acesso` timestamp NULL DEFAULT NULL COMMENT 'Data e hora do último login bem-sucedido.',
  `usr_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data e hora da criação do registro.',
  `usr_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Data e hora da última alteração do registro.',
  `usr_status` char(1) NOT NULL COMMENT 'A = Ativo, I = Inativo'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro dos usuários que acessam o sistema.';

CREATE TABLE IF NOT EXISTS `bi_dashboards` (
  `dsh_id` int(11) NOT NULL COMMENT 'Identificador único do Dashboard.',
  `dsh_nome` varchar(100) NOT NULL,
  `dsh_descricao` text DEFAULT NULL,
  `dsh_tipo` enum('FINANCEIRO','COMERCIAL','PRODUCAO','LOGISTICA','RH','GERAL') NOT NULL,
  `dsh_publico` tinyint(1) DEFAULT 0 COMMENT 'Se TRUE, todos os usuários podem visualizar.',
  `dsh_usr_proprietario` int(11) NOT NULL COMMENT 'Usuário criador (FK: adm_usuarios).',
  `dsh_ordem` int(11) DEFAULT NULL COMMENT 'Ordem de exibição na lista de dashboards.',
  `dsh_ativo` tinyint(1) DEFAULT 1,
  `dsh_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro dos painéis de indicadores (Dashboards).';

CREATE TABLE IF NOT EXISTS `bi_kpis` (
  `kpi_id` int(11) NOT NULL COMMENT 'Identificador único do KPI.',
  `kpi_codigo` varchar(50) NOT NULL,
  `kpi_nome` varchar(100) NOT NULL,
  `kpi_descricao` text DEFAULT NULL,
  `kpi_formula` text NOT NULL COMMENT 'Fórmula SQL ou lógica de cálculo para o KPI.',
  `kpi_unidade` enum('VALOR','PERCENTUAL','QUANTIDADE','TEMPO') NOT NULL,
  `kpi_meta_tipo` enum('MAIOR','MENOR','IGUAL','ENTRE') DEFAULT 'MAIOR' COMMENT 'Direção da meta.',
  `kpi_meta_valor` decimal(15,2) DEFAULT NULL COMMENT 'Meta principal (se o tipo não for ENTRE).',
  `kpi_meta_valor_min` decimal(15,2) DEFAULT NULL COMMENT 'Valor mínimo (se kpi_meta_tipo = ENTRE).',
  `kpi_meta_valor_max` decimal(15,2) DEFAULT NULL COMMENT 'Valor máximo (se kpi_meta_tipo = ENTRE).',
  `kpi_periodicidade` enum('DIARIO','SEMANAL','MENSAL','TRIMESTRAL','ANUAL') NOT NULL,
  `kpi_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Definição dos Indicadores Chave de Desempenho (KPIs).';

CREATE TABLE IF NOT EXISTS `bi_kpi_valores` (
  `kpv_id` int(11) NOT NULL COMMENT 'Identificador único do Valor do KPI.',
  `kpv_kpi_id` int(11) NOT NULL COMMENT 'KPI (FK: bi_kpis).',
  `kpv_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `kpv_periodo` date NOT NULL COMMENT 'Data de referência para o valor (Ex: Mês, Semana).',
  `kpv_valor` decimal(15,2) NOT NULL COMMENT 'Valor calculado do KPI.',
  `kpv_meta` decimal(15,2) DEFAULT NULL COMMENT 'Meta que estava vigente no período.',
  `kpv_desvio` decimal(15,2) GENERATED ALWAYS AS (`kpv_valor` - `kpv_meta`) STORED COMMENT 'Desvio em relação à meta (Calculado).',
  `kpv_status` enum('ABAIXO','DENTRO','ACIMA') DEFAULT 'DENTRO' COMMENT 'Status em relação à meta (Semaforização).',
  `kpv_data_calculo` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Armazenamento dos valores calculados dos KPIs por período.';

CREATE TABLE IF NOT EXISTS `bi_relatorios` (
  `rel_id` int(11) NOT NULL COMMENT 'Identificador único do Relatório.',
  `rel_nome` varchar(100) NOT NULL,
  `rel_descricao` text DEFAULT NULL,
  `rel_modulo` varchar(50) NOT NULL,
  `rel_query` text NOT NULL COMMENT 'Query SQL que define os dados do relatório.',
  `rel_parametros` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Definição dos campos de filtro do relatório (JSON).' CHECK (json_valid(`rel_parametros`)),
  `rel_formato_padrao` enum('PDF','EXCEL','CSV','HTML') DEFAULT 'PDF',
  `rel_publico` tinyint(1) DEFAULT 0,
  `rel_usr_proprietario` int(11) NOT NULL COMMENT 'Usuário criador (FK: adm_usuarios).',
  `rel_ativo` tinyint(1) DEFAULT 1,
  `rel_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Definições de relatórios customizáveis do sistema.';

CREATE TABLE IF NOT EXISTS `bi_relatorio_historico` (
  `rhi_id` bigint(20) NOT NULL COMMENT 'Identificador único da Execução.',
  `rhi_rel_id` int(11) NOT NULL COMMENT 'Relatório executado (FK: bi_relatorios).',
  `rhi_usr_id` int(11) NOT NULL COMMENT 'Usuário que executou (FK: adm_usuarios).',
  `rhi_parametros` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Valores dos filtros usados na execução.' CHECK (json_valid(`rhi_parametros`)),
  `rhi_formato` enum('PDF','EXCEL','CSV','HTML') NOT NULL,
  `rhi_arquivo` varchar(200) DEFAULT NULL COMMENT 'Caminho/URL do arquivo gerado.',
  `rhi_tempo_execucao_ms` int(11) DEFAULT NULL COMMENT 'Tempo que levou para gerar o relatório em milissegundos.',
  `rhi_status` enum('SUCESSO','ERRO') NOT NULL,
  `rhi_mensagem_erro` text DEFAULT NULL,
  `rhi_data_execucao` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico de todas as execuções de relatórios.';

CREATE TABLE IF NOT EXISTS `bi_widgets` (
  `wdg_id` int(11) NOT NULL COMMENT 'Identificador único do Widget.',
  `wdg_dsh_id` int(11) NOT NULL COMMENT 'Dashboard (FK: bi_dashboards).',
  `wdg_kpi_id` int(11) DEFAULT NULL COMMENT 'KPI associado (FK: bi_kpis).',
  `wdg_titulo` varchar(100) NOT NULL,
  `wdg_tipo` enum('GRAFICO_LINHA','GRAFICO_BARRA','GRAFICO_PIZZA','NUMERO','TABELA','GAUGE') NOT NULL,
  `wdg_tamanho` enum('PEQUENO','MEDIO','GRANDE') DEFAULT 'MEDIO',
  `wdg_posicao_x` int(11) DEFAULT 0 COMMENT 'Coordenada X na grade do dashboard.',
  `wdg_posicao_y` int(11) DEFAULT 0 COMMENT 'Coordenada Y na grade do dashboard.',
  `wdg_largura` int(11) DEFAULT 1 COMMENT 'Número de colunas que o widget ocupa.',
  `wdg_altura` int(11) DEFAULT 1 COMMENT 'Número de linhas que o widget ocupa.',
  `wdg_query` text DEFAULT NULL COMMENT 'Query SQL específica para o widget (se não for ligado a um KPI).',
  `wdg_configuracao` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Configurações do gráfico/tabela (cores, eixos, filtros).' CHECK (json_valid(`wdg_configuracao`)),
  `wdg_ordem` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Componentes visuais (gráficos, tabelas) que compõem um dashboard.';

CREATE TABLE IF NOT EXISTS `cfg_anexos` (
  `anx_id` int(11) NOT NULL COMMENT 'Identificador único do Anexo.',
  `anx_modulo` varchar(50) NOT NULL,
  `anx_tabela` varchar(50) NOT NULL,
  `anx_registro_id` int(11) NOT NULL COMMENT 'ID do registro que o anexo está vinculado.',
  `anx_nome_original` varchar(200) NOT NULL,
  `anx_nome_arquivo` varchar(200) NOT NULL COMMENT 'Nome no servidor (geralmente hash).',
  `anx_caminho` varchar(500) NOT NULL COMMENT 'Caminho físico ou URL de armazenamento.',
  `anx_tipo_mime` varchar(100) DEFAULT NULL,
  `anx_tamanho_bytes` bigint(20) DEFAULT NULL,
  `anx_descricao` text DEFAULT NULL,
  `anx_usr_upload` int(11) NOT NULL COMMENT 'Usuário que fez o upload (FK: adm_usuarios).',
  `anx_data_upload` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Gerenciamento de arquivos e documentos anexados a registros.';

CREATE TABLE IF NOT EXISTS `cfg_comentarios` (
  `com_id` bigint(20) NOT NULL COMMENT 'Identificador único do Comentário.',
  `com_modulo` varchar(50) NOT NULL,
  `com_tabela` varchar(50) NOT NULL,
  `com_registro_id` int(11) NOT NULL COMMENT 'ID do registro que o comentário está vinculado.',
  `com_usr_id` int(11) NOT NULL COMMENT 'Usuário que postou (FK: adm_usuarios).',
  `com_comentario` text NOT NULL,
  `com_pai_id` bigint(20) DEFAULT NULL COMMENT 'ID do comentário pai (para respostas/threads) (FK: cfg_comentarios).',
  `com_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sistema de comentários/notas internas para registros.';

CREATE TABLE IF NOT EXISTS `cfg_layouts_impressao` (
  `lyi_id` int(11) NOT NULL COMMENT 'Identificador único do Layout.',
  `lyi_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `lyi_tipo` varchar(50) NOT NULL COMMENT 'Tipo de documento (Ex: NOTA_FISCAL, BOLETO, ORDEM_VENDA).',
  `lyi_nome` varchar(100) NOT NULL,
  `lyi_template` text NOT NULL COMMENT 'Código do template (HTML, Blade, Report Server XML, etc.).',
  `lyi_orientacao` enum('RETRATO','PAISAGEM') DEFAULT 'RETRATO',
  `lyi_tamanho_papel` enum('A4','CARTA','OFICIO','PERSONALIZADO') DEFAULT 'A4',
  `lyi_margens` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT 'Definição de margens (JSON).' CHECK (json_valid(`lyi_margens`)),
  `lyi_padrao` tinyint(1) DEFAULT 0,
  `lyi_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Templates e definições de layouts de documentos para impressão.';

CREATE TABLE IF NOT EXISTS `cfg_notificacoes` (
  `ntf_id` bigint(20) NOT NULL COMMENT 'Identificador único da Notificação.',
  `ntf_usr_destinatario` int(11) NOT NULL COMMENT 'Usuário que deve receber (FK: adm_usuarios).',
  `ntf_tipo` enum('INFO','ALERTA','ERRO','SUCESSO','TAREFA') NOT NULL,
  `ntf_titulo` varchar(200) NOT NULL,
  `ntf_mensagem` text NOT NULL,
  `ntf_link` varchar(200) DEFAULT NULL COMMENT 'Link para o registro no sistema.',
  `ntf_modulo` varchar(50) DEFAULT NULL,
  `ntf_registro_id` int(11) DEFAULT NULL,
  `ntf_lida` tinyint(1) DEFAULT 0,
  `ntf_data_leitura` timestamp NULL DEFAULT NULL,
  `ntf_data_criacao` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Fila e histórico de notificações de sistema por usuário.';

CREATE TABLE IF NOT EXISTS `cfg_parametros` (
  `par_id` int(11) NOT NULL COMMENT 'Identificador único do Parâmetro.',
  `par_emp_id` int(11) DEFAULT NULL COMMENT 'ID da Empresa (NULL para parâmetro global) (FK: adm_empresas).',
  `par_chave` varchar(100) NOT NULL COMMENT 'Chave única para acesso ao parâmetro (Ex: DIAS_AVISO_PAGAR).',
  `par_valor` text DEFAULT NULL COMMENT 'Valor armazenado (Ex: 5, TRUE, "{...}").',
  `par_tipo` enum('STRING','INTEGER','DECIMAL','BOOLEAN','DATE','JSON') DEFAULT 'STRING',
  `par_modulo` varchar(50) DEFAULT NULL COMMENT 'Módulo a que o parâmetro se refere (Ex: FIN, VENDAS).',
  `par_descricao` text DEFAULT NULL COMMENT 'Descrição para exibição na tela de configuração.',
  `par_editavel` tinyint(1) DEFAULT 1,
  `par_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Parâmetros globais e específicos por empresa do sistema.';

CREATE TABLE IF NOT EXISTS `cfg_sequenciais` (
  `seq_id` int(11) NOT NULL COMMENT 'Identificador único do Sequencial.',
  `seq_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `seq_tabela` varchar(50) NOT NULL COMMENT 'Tabela que está sendo numerada (Ex: fin_contas_pagar).',
  `seq_campo` varchar(50) NOT NULL COMMENT 'Campo que armazena a numeração (Ex: cpg_numero).',
  `seq_prefixo` varchar(10) DEFAULT NULL COMMENT 'Prefixo fixo da numeração (Ex: CP).',
  `seq_proximo_numero` int(11) NOT NULL DEFAULT 1,
  `seq_tamanho` int(11) DEFAULT 6 COMMENT 'Número mínimo de dígitos (preenchimento com zeros).',
  `seq_ano_mes_atual` varchar(6) DEFAULT NULL COMMENT 'Usado para controle de reinício mensal/anual (Ex: 202511).',
  `seq_reinicia_mes` tinyint(1) DEFAULT 0,
  `seq_reinicia_ano` tinyint(1) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle de numeração automática e sequencial de documentos.';

CREATE TABLE IF NOT EXISTS `cfg_tarefas` (
  `tar_id` int(11) NOT NULL COMMENT 'Identificador único da Tarefa.',
  `tar_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `tar_usr_criador` int(11) NOT NULL COMMENT 'Usuário que criou (FK: adm_usuarios).',
  `tar_titulo` varchar(200) NOT NULL,
  `tar_descricao` text DEFAULT NULL,
  `tar_prioridade` enum('BAIXA','MEDIA','ALTA','URGENTE') DEFAULT 'MEDIA',
  `tar_data_inicio` date DEFAULT NULL,
  `tar_data_prazo` date DEFAULT NULL,
  `tar_data_conclusao` date DEFAULT NULL,
  `tar_status` enum('PENDENTE','EM_ANDAMENTO','CONCLUIDA','CANCELADA') DEFAULT 'PENDENTE',
  `tar_progresso` int(11) DEFAULT 0 COMMENT 'Percentual de conclusão.',
  `tar_modulo` varchar(50) DEFAULT NULL COMMENT 'Módulo de origem (se vinculada a um processo).',
  `tar_registro_id` int(11) DEFAULT NULL COMMENT 'ID do registro vinculado.',
  `tar_observacoes` text DEFAULT NULL,
  `tar_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `cfg_workflow_aprovacoes` (
  `wfa_id` int(11) NOT NULL COMMENT 'Identificador único da Aprovação.',
  `wfa_wfi_id` int(11) NOT NULL COMMENT 'Instância de Workflow (FK: cfg_workflow_instancias).',
  `wfa_wfe_id` int(11) NOT NULL COMMENT 'Etapa do Workflow (FK: cfg_workflow_etapas).',
  `wfa_usr_aprovador` int(11) NOT NULL COMMENT 'Usuário responsável/que aprovou (FK: adm_usuarios).',
  `wfa_status` enum('PENDENTE','APROVADO','REJEITADO') DEFAULT 'PENDENTE',
  `wfa_data_solicitacao` timestamp NOT NULL DEFAULT current_timestamp(),
  `wfa_data_resposta` timestamp NULL DEFAULT NULL,
  `wfa_prazo_limite` timestamp NULL DEFAULT NULL,
  `wfa_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe das ações de aprovação/rejeição de cada etapa.';

CREATE TABLE IF NOT EXISTS `cfg_workflow_definicoes` (
  `wfd_id` int(11) NOT NULL COMMENT 'Identificador único da Definição de Workflow.',
  `wfd_codigo` varchar(50) NOT NULL COMMENT 'Código único do processo (Ex: APROV_COMPRA).',
  `wfd_nome` varchar(100) NOT NULL,
  `wfd_modulo` varchar(50) NOT NULL COMMENT 'Módulo de aplicação (Ex: COMPRAS).',
  `wfd_tabela` varchar(50) NOT NULL COMMENT 'Tabela principal envolvida (Ex: com_pedidos_compra).',
  `wfd_descricao` text DEFAULT NULL,
  `wfd_ativo` tinyint(1) DEFAULT 1,
  `wfd_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro dos modelos de processos de aprovação/workflow.';

CREATE TABLE IF NOT EXISTS `cfg_workflow_etapas` (
  `wfe_id` int(11) NOT NULL COMMENT 'Identificador único da Etapa.',
  `wfe_wfd_id` int(11) NOT NULL COMMENT 'Definição de Workflow (FK: cfg_workflow_definicoes).',
  `wfe_nivel` int(11) NOT NULL COMMENT 'Nível hierárquico da etapa (1, 2, 3...).',
  `wfe_nome` varchar(100) NOT NULL,
  `wfe_tipo` enum('APROVACAO','NOTIFICACAO','AUTOMATICO') NOT NULL,
  `wfe_condicao` text DEFAULT NULL COMMENT 'Regra para esta etapa ser ativada (Ex: valor > 1000).',
  `wfe_usr_aprovador_id` int(11) DEFAULT NULL COMMENT 'Usuário específico aprovador (FK: adm_usuarios).',
  `wfe_perfil_aprovador_id` int(11) DEFAULT NULL COMMENT 'ID do perfil ou grupo aprovador (FK: adm_perfis).',
  `wfe_alcada_minima` decimal(15,2) DEFAULT NULL COMMENT 'Alçada de aprovação mínima.',
  `wfe_alcada_maxima` decimal(15,2) DEFAULT NULL COMMENT 'Alçada de aprovação máxima.',
  `wfe_prazo_horas` int(11) DEFAULT NULL COMMENT 'Prazo limite para a aprovação.',
  `wfe_ordem` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Etapas sequenciais de um processo de workflow.';

CREATE TABLE IF NOT EXISTS `cfg_workflow_instancias` (
  `wfi_id` int(11) NOT NULL COMMENT 'Identificador único da Instância (Execução).',
  `wfi_wfd_id` int(11) NOT NULL COMMENT 'Definição de Workflow (FK: cfg_workflow_definicoes).',
  `wfi_registro_id` int(11) NOT NULL COMMENT 'ID do registro que iniciou o workflow (Ex: ID do Pedido de Compra).',
  `wfi_usr_solicitante` int(11) NOT NULL COMMENT 'Usuário que iniciou (FK: adm_usuarios).',
  `wfi_status` enum('AGUARDANDO','APROVADO','REJEITADO','CANCELADO') DEFAULT 'AGUARDANDO',
  `wfi_data_inicio` timestamp NOT NULL DEFAULT current_timestamp(),
  `wfi_data_conclusao` timestamp NULL DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro das execuções ativas de um workflow em um registro específico.';

CREATE TABLE IF NOT EXISTS `cmp_aprovacoes` (
  `apc_id` int(11) NOT NULL COMMENT 'Identificador único da Etapa de Aprovação.',
  `apc_pdc_id` int(11) NOT NULL COMMENT 'Pedido de Compra a ser aprovado (FK: cmp_pedidos).',
  `apc_usr_solicitante` int(11) NOT NULL COMMENT 'Usuário que solicitou (FK: adm_usuarios).',
  `apc_usr_aprovador` int(11) NOT NULL COMMENT 'Usuário que deve aprovar/rejeitar (FK: adm_usuarios).',
  `apc_nivel` int(11) NOT NULL COMMENT 'Nível hierárquico da aprovação.',
  `apc_status` enum('PENDENTE','APROVADO','REJEITADO') DEFAULT 'PENDENTE',
  `apc_data_solicitacao` timestamp NOT NULL DEFAULT current_timestamp(),
  `apc_data_resposta` timestamp NULL DEFAULT NULL,
  `apc_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico do fluxo de Aprovacao de pedidos de compra.';

CREATE TABLE IF NOT EXISTS `cmp_cotacao_fornecedores` (
  `cof_id` int(11) NOT NULL COMMENT 'Identificador único da Cotacao/Fornecedor.',
  `cof_cot_id` int(11) NOT NULL COMMENT 'Cotacao (FK: cmp_cotacoes).',
  `cof_pes_id` int(11) NOT NULL COMMENT 'Fornecedor (FK: adm_pessoas).',
  `cof_data_envio` date DEFAULT NULL,
  `cof_data_resposta` date DEFAULT NULL,
  `cof_prazo_entrega` int(11) DEFAULT NULL,
  `cof_condicao_pagamento` varchar(100) DEFAULT NULL,
  `cof_frete` decimal(15,2) DEFAULT 0.00,
  `cof_observacoes` text DEFAULT NULL,
  `cof_vencedor` tinyint(1) DEFAULT 0 COMMENT 'Indica se este fornecedor ganhou a Cotacao.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Fornecedores participantes de cada Cotacao.';

CREATE TABLE IF NOT EXISTS `cmp_cotacao_itens` (
  `coi_id` int(11) NOT NULL COMMENT 'Identificador único do Preço do Item Cotado.',
  `coi_cof_id` int(11) NOT NULL COMMENT 'Fornecedor na Cotacao (FK: cmp_cotacao_fornecedores).',
  `coi_itm_id` int(11) NOT NULL COMMENT 'Item cotado (FK: vnd_itens).',
  `coi_quantidade` decimal(15,3) NOT NULL,
  `coi_preco_unitario` decimal(15,2) NOT NULL,
  `coi_valor_total` decimal(15,2) NOT NULL,
  `coi_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe dos precos dos itens por fornecedor na Cotacao.';

CREATE TABLE IF NOT EXISTS `cmp_cotacoes` (
  `cot_id` int(11) NOT NULL COMMENT 'Identificador único da Cotacao.',
  `cot_emp_id` int(11) NOT NULL COMMENT 'Empresa que realiza a compra (FK: adm_empresas).',
  `cot_numero` varchar(50) NOT NULL COMMENT 'Número da Cotacao.',
  `cot_req_id` int(11) DEFAULT NULL COMMENT 'Requisição de origem (FK: cmp_requisicoes).',
  `cot_data` date NOT NULL,
  `cot_data_limite` date DEFAULT NULL,
  `cot_observacoes` text DEFAULT NULL,
  `cot_status` enum('ABERTA','ANALISE','FECHADA','CANCELADA') DEFAULT 'ABERTA',
  `cot_usr_responsavel` int(11) NOT NULL COMMENT 'Comprador responsável (FK: adm_usuarios).',
  `cot_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cabeçalho das Cotações de Compra.';

CREATE TABLE IF NOT EXISTS `cmp_notas_fiscais` (
  `nfe_id` int(11) NOT NULL COMMENT 'Identificador único da NF de Entrada.',
  `nfe_emp_id` int(11) NOT NULL COMMENT 'Empresa recebedora (FK: adm_empresas).',
  `nfe_pdc_id` int(11) DEFAULT NULL COMMENT 'Pedido de Compra relacionado (FK: cmp_pedidos).',
  `nfe_numero` varchar(20) NOT NULL,
  `nfe_serie` varchar(5) NOT NULL,
  `nfe_modelo` varchar(5) NOT NULL,
  `nfe_chave_acesso` varchar(44) DEFAULT NULL,
  `nfe_pes_id` int(11) NOT NULL COMMENT 'Fornecedor (FK: adm_pessoas).',
  `nfe_data_emissao` date NOT NULL,
  `nfe_data_entrada` date NOT NULL,
  `nfe_valor_produtos` decimal(15,2) NOT NULL,
  `nfe_valor_total` decimal(15,2) NOT NULL,
  `nfe_cfop` varchar(5) NOT NULL,
  `nfe_natureza_operacao` varchar(100) NOT NULL,
  `nfe_xml` longtext DEFAULT NULL,
  `nfe_status` enum('DIGITACAO','VALIDADA','LANCADA','CANCELADA') DEFAULT 'DIGITACAO',
  `nfe_observacoes` text DEFAULT NULL,
  `nfe_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que cadastrou (FK: adm_usuarios).',
  `nfe_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro das Notas Fiscais de Entrada (Compras).';

CREATE TABLE IF NOT EXISTS `cmp_pedidos` (
  `pdc_id` int(11) NOT NULL COMMENT 'Identificador único do Pedido de Compra.',
  `pdc_emp_id` int(11) NOT NULL COMMENT 'Empresa que compra (FK: adm_empresas).',
  `pdc_numero` varchar(50) NOT NULL COMMENT 'Número do pedido.',
  `pdc_cot_id` int(11) DEFAULT NULL COMMENT 'Cotacao de origem (FK: cmp_cotacoes).',
  `pdc_req_id` int(11) DEFAULT NULL COMMENT 'Requisição de origem (FK: cmp_requisicoes).',
  `pdc_pes_id` int(11) NOT NULL COMMENT 'Fornecedor (FK: adm_pessoas).',
  `pdc_data_pedido` date NOT NULL,
  `pdc_data_entrega` date DEFAULT NULL,
  `pdc_valor_produtos` decimal(15,2) DEFAULT 0.00,
  `pdc_valor_frete` decimal(15,2) DEFAULT 0.00,
  `pdc_valor_desconto` decimal(15,2) DEFAULT 0.00,
  `pdc_valor_total` decimal(15,2) DEFAULT 0.00,
  `pdc_tipo_frete` enum('CIF','FOB') DEFAULT 'FOB',
  `pdc_condicao_pagamento` varchar(100) DEFAULT NULL,
  `pdc_ccu_id` int(11) NOT NULL COMMENT 'Centro de Custo que arca com o pedido (FK: fin_centros_custo).',
  `pdc_status` enum('ELABORACAO','APROVADO','RECEBIDO_PARCIAL','RECEBIDO','CANCELADO') DEFAULT 'ELABORACAO',
  `pdc_observacoes` text DEFAULT NULL,
  `pdc_usr_comprador` int(11) NOT NULL COMMENT 'Usuário comprador (FK: adm_usuarios).',
  `pdc_usr_aprovador` int(11) DEFAULT NULL COMMENT 'Usuário que aprovou o pedido (FK: adm_usuarios).',
  `pdc_data_aprovacao` timestamp NULL DEFAULT NULL,
  `pdc_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp(),
  `pdc_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `pdc_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que cadastrou (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos Pedidos de Compra.';

CREATE TABLE IF NOT EXISTS `cmp_pedido_itens` (
  `pci_id` int(11) NOT NULL COMMENT 'Identificador único do Item do Pedido.',
  `pci_pdc_id` int(11) NOT NULL COMMENT 'Pedido de Compra (FK: cmp_pedidos).',
  `pci_itm_id` int(11) NOT NULL COMMENT 'Item comprado (FK: vnd_itens).',
  `pci_sequencia` int(11) NOT NULL,
  `pci_descricao` varchar(200) DEFAULT NULL COMMENT 'Descrição no momento da compra.',
  `pci_quantidade` decimal(15,3) NOT NULL,
  `pci_quantidade_recebida` decimal(15,3) DEFAULT 0.000 COMMENT 'Quantidade já recebida fisicamente.',
  `pci_preco_unitario` decimal(15,2) NOT NULL,
  `pci_desconto_percentual` decimal(5,2) DEFAULT 0.00,
  `pci_desconto_valor` decimal(15,2) DEFAULT 0.00,
  `pci_valor_total` decimal(15,2) NOT NULL,
  `pci_pct_id` int(11) DEFAULT NULL COMMENT 'Conta Contábil de Custo (FK: fin_plano_contas).',
  `pci_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo (FK: fin_centros_custo).',
  `pdi_pdc_id` int(11) NOT NULL COMMENT 'Pedido de Compra principal (FK: cmp_pedidos).',
  `pdi_itm_id` int(11) NOT NULL COMMENT 'ID do Item (FK: vnd_itens).',
  `pdi_pct_id` int(11) NOT NULL COMMENT 'ID do Item (FK: vnd_itens).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Itens que compõem cada Pedido de Compra.';

CREATE TABLE IF NOT EXISTS `cmp_requisicao_itens` (
  `rqi_id` int(11) NOT NULL COMMENT 'Identificador único do Item da Requisição.',
  `rqi_req_id` int(11) NOT NULL COMMENT 'Requisição de Compra (FK: cmp_requisicoes).',
  `rqi_itm_id` int(11) NOT NULL COMMENT 'Item requisitado (FK: vnd_itens).',
  `rqi_sequencia` int(11) NOT NULL,
  `rqi_quantidade` decimal(15,3) NOT NULL,
  `rqi_data_necessidade` date DEFAULT NULL,
  `rqi_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Itens requisitados em cada Requisição de Compra.';

CREATE TABLE IF NOT EXISTS `cmp_requisicoes` (
  `req_id` int(11) NOT NULL COMMENT 'Identificador único da Requisição.',
  `req_emp_id` int(11) NOT NULL COMMENT 'Empresa solicitante (FK: adm_empresas).',
  `req_numero` varchar(50) NOT NULL COMMENT 'Número da requisição.',
  `req_data` date NOT NULL,
  `req_ccu_id` int(11) NOT NULL COMMENT 'Centro de Custo requisitante (FK: fin_centros_custo).',
  `req_usr_solicitante` int(11) NOT NULL COMMENT 'Usuário que solicitou (FK: adm_usuarios).',
  `req_justificativa` text DEFAULT NULL,
  `req_status` enum('ELABORACAO','APROVADA','COTACAO','COMPRADA','CANCELADA') DEFAULT 'ELABORACAO',
  `req_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cabeçalho das Requisicoes de Compra.';

CREATE TABLE IF NOT EXISTS `cnt_fechamentos` (
  `fec_id` int(11) NOT NULL COMMENT 'Identificador único do Fechamento.',
  `fec_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `fec_periodo` date NOT NULL COMMENT 'Mês/Ano de competência fechado (Ex: 2025-11-01).',
  `fec_data_fechamento` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `fec_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário que realizou o fechamento (FK: adm_usuarios).',
  `fec_bloqueado` tinyint(1) DEFAULT 1 COMMENT 'Indica se o período está bloqueado para novos lançamentos.',
  `fec_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle dos períodos contábeis fechados.';

CREATE TABLE IF NOT EXISTS `cnt_lancamentos` (
  `lcn_id` int(11) NOT NULL COMMENT 'Identificador único do Lançamento Contábil.',
  `lcn_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `lcn_lote` varchar(20) NOT NULL COMMENT 'Número do lote de agrupamento (Ex: LCT_102025).',
  `lcn_sublote` varchar(10) DEFAULT NULL COMMENT 'Sublote para detalhamento interno.',
  `lcn_data` date NOT NULL COMMENT 'Data efetiva do lançamento.',
  `lcn_historico_padrao` varchar(200) DEFAULT NULL COMMENT 'Código ou descrição de um histórico contábil padrão.',
  `lcn_complemento` text DEFAULT NULL COMMENT 'Histórico complementar/detalhado.',
  `lcn_valor` decimal(15,2) NOT NULL COMMENT 'Valor total do lançamento (Débito = Crédito).',
  `lcn_tipo` enum('MANUAL','AUTOMATICO') DEFAULT 'MANUAL',
  `lcn_origem_modulo` varchar(50) DEFAULT NULL COMMENT 'Módulo de origem (Ex: FIN, COMPRAS, VENDAS).',
  `lcn_origem_id` int(11) DEFAULT NULL COMMENT 'ID do registro de origem (Ex: ID da NF, ID do Pagar).',
  `lcn_estornado` tinyint(1) DEFAULT 0 COMMENT 'Indica se este lançamento foi estornado.',
  `lcn_lcn_estorno_id` int(11) DEFAULT NULL COMMENT 'ID do lançamento de estorno (FK: cnt_lancamentos).',
  `lcn_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que criou o lançamento (FK: adm_usuarios).',
  `lcn_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro de todos os lançamentos contábeis (cabeçalho).';

CREATE TABLE IF NOT EXISTS `cnt_partidas` (
  `prt_id` int(11) NOT NULL COMMENT 'Identificador único da Partida.',
  `prt_lcn_id` int(11) NOT NULL COMMENT 'Lançamento Contábil (FK: cnt_lancamentos).',
  `prt_tipo` enum('DEBITO','CREDITO') NOT NULL,
  `prt_pct_id` int(11) NOT NULL COMMENT 'ID da Conta Contábil (FK: cnt_plano_contas).',
  `prt_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo (FK: fin_centros_custo).',
  `prt_valor` decimal(15,2) NOT NULL,
  `prt_historico` text DEFAULT NULL COMMENT 'Histórico específico para a partida.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe das partidas de débito e crédito de um lançamento.';

CREATE TABLE IF NOT EXISTS `est_curva_abc` (
  `abc_id` int(11) NOT NULL COMMENT 'Identificador único da Classificação ABC.',
  `abc_itm_id` int(11) NOT NULL COMMENT 'Item (FK: vnd_itens).',
  `abc_ano` int(11) NOT NULL,
  `abc_mes` int(11) NOT NULL,
  `abc_quantidade_vendida` decimal(15,3) DEFAULT NULL,
  `abc_valor_vendido` decimal(15,2) DEFAULT NULL,
  `abc_classificacao` enum('A','B','C','D') NOT NULL,
  `abc_percentual_acumulado` decimal(5,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Resultado da Classificação de Curva ABC por período.';

CREATE TABLE IF NOT EXISTS `est_depositos` (
  `dep_id` int(11) NOT NULL COMMENT 'Identificador único do Depósito/Armazém.',
  `dep_emp_id` int(11) NOT NULL COMMENT 'Empresa proprietária (FK: adm_empresas).',
  `dep_codigo` varchar(20) NOT NULL COMMENT 'Código único para identificação do depósito.',
  `dep_nome` varchar(100) NOT NULL COMMENT 'Nome do depósito.',
  `dep_tipo` enum('GERAL','MATERIA_PRIMA','PRODUTO_ACABADO','QUARENTENA','TRANSITO') NOT NULL COMMENT 'Classificação do tipo de depósito.',
  `dep_endereco` varchar(200) DEFAULT NULL COMMENT 'Endereço físico.',
  `dep_responsavel_usr_id` int(11) DEFAULT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `dep_ativo` tinyint(1) DEFAULT 1,
  `dep_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de locais de armazenamento (Depósitos e Armazéns).';

CREATE TABLE IF NOT EXISTS `est_enderecos` (
  `end_id` int(11) NOT NULL COMMENT 'Identificador único do Endereço (Posição WMS).',
  `end_dep_id` int(11) NOT NULL COMMENT 'Depósito ao qual pertence (FK: est_depositos).',
  `end_codigo` varchar(50) NOT NULL COMMENT 'Código do endereço (Ex: A01-01-01).',
  `end_corredor` varchar(10) DEFAULT NULL,
  `end_prateleira` varchar(10) DEFAULT NULL,
  `end_nivel` varchar(10) DEFAULT NULL,
  `end_tipo` enum('PICKING','ARMAZENAGEM','EXPEDICAO','RECEBIMENTO') NOT NULL COMMENT 'Função do endereço.',
  `end_capacidade_peso` decimal(10,2) DEFAULT NULL,
  `end_capacidade_volume` decimal(10,2) DEFAULT NULL,
  `end_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Endereçamento detalhado dentro dos depósitos (WMS).';

CREATE TABLE IF NOT EXISTS `est_inventarios` (
  `inv_id` int(11) NOT NULL COMMENT 'Identificador único do Inventário.',
  `inv_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `inv_numero` varchar(50) NOT NULL,
  `inv_dep_id` int(11) NOT NULL COMMENT 'Depósito sendo inventariado (FK: est_depositos).',
  `inv_tipo` enum('GERAL','CICLICO','PARCIAL') NOT NULL,
  `inv_data_abertura` timestamp NOT NULL DEFAULT current_timestamp(),
  `inv_data_fechamento` timestamp NULL DEFAULT NULL,
  `inv_status` enum('ABERTO','CONTAGEM','RECONTAGEM','FECHADO','CANCELADO') DEFAULT 'ABERTO',
  `inv_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `inv_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos eventos de Inventário (Geral ou Cíclico).';

CREATE TABLE IF NOT EXISTS `est_inventario_contagens` (
  `inc_id` int(11) NOT NULL COMMENT 'Identificador único da Contagem.',
  `inc_inv_id` int(11) NOT NULL COMMENT 'Inventário (FK: est_inventarios).',
  `inc_itm_id` int(11) NOT NULL COMMENT 'Item contado (FK: vnd_itens).',
  `inc_end_id` int(11) DEFAULT NULL COMMENT 'Endereço contado (FK: est_enderecos).',
  `inc_lote` varchar(50) DEFAULT NULL,
  `inc_serie` varchar(50) DEFAULT NULL,
  `inc_quantidade_sistema` decimal(15,3) NOT NULL COMMENT 'Saldo do sistema no momento da abertura.',
  `inc_quantidade_contada` decimal(15,3) DEFAULT NULL,
  `inc_diferenca` decimal(15,3) GENERATED ALWAYS AS (`inc_quantidade_contada` - `inc_quantidade_sistema`) STORED COMMENT 'Diferença entre o contado e o sistema.',
  `inc_usr_contador` int(11) DEFAULT NULL COMMENT 'Usuário que realizou a contagem (FK: adm_usuarios).',
  `inc_data_contagem` timestamp NULL DEFAULT NULL,
  `inc_recontagem` tinyint(1) DEFAULT 0,
  `inc_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe das contagens realizadas durante o inventário.';

CREATE TABLE IF NOT EXISTS `est_movimentacoes` (
  `mov_id` int(11) NOT NULL COMMENT 'Identificador único da Movimentação.',
  `mov_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `mov_tipo` enum('ENTRADA','SAIDA','TRANSFERENCIA','AJUSTE','INVENTARIO') NOT NULL,
  `mov_data` timestamp NOT NULL DEFAULT current_timestamp(),
  `mov_itm_id` int(11) NOT NULL COMMENT 'Item movimentado (FK: vnd_itens).',
  `mov_dep_origem_id` int(11) DEFAULT NULL COMMENT 'Depósito de onde saiu (FK: est_depositos).',
  `mov_dep_destino_id` int(11) DEFAULT NULL COMMENT 'Depósito para onde foi (FK: est_depositos).',
  `mov_end_origem_id` int(11) DEFAULT NULL COMMENT 'Endereço de origem (FK: est_enderecos).',
  `mov_end_destino_id` int(11) DEFAULT NULL COMMENT 'Endereço de destino (FK: est_enderecos).',
  `mov_lote` varchar(50) DEFAULT NULL,
  `mov_serie` varchar(50) DEFAULT NULL,
  `mov_quantidade` decimal(15,3) NOT NULL,
  `mov_custo_unitario` decimal(15,2) DEFAULT NULL,
  `mov_custo_total` decimal(15,2) DEFAULT NULL,
  `mov_documento_tipo` varchar(50) DEFAULT NULL COMMENT 'Tipo do documento de origem (NFE, PDV, OP).',
  `mov_documento_id` int(11) DEFAULT NULL COMMENT 'ID do documento de origem (nfe_id, pdv_id, orp_id, etc.).',
  `mov_observacoes` text DEFAULT NULL,
  `mov_usr_id` int(11) NOT NULL COMMENT 'Usuário que registrou a movimentação (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico de todas as movimentações físicas de estoque.';

CREATE TABLE IF NOT EXISTS `est_ordem_separacao_itens` (
  `ori_id` int(11) NOT NULL COMMENT 'Identificador único do Item da Ordem.',
  `ori_ors_id` int(11) NOT NULL COMMENT 'Ordem de Separação (FK: est_ordens_separacao).',
  `ori_itm_id` int(11) NOT NULL COMMENT 'Item a ser separado (FK: vnd_itens).',
  `ori_end_id` int(11) NOT NULL COMMENT 'Endereço sugerido para picking (FK: est_enderecos).',
  `ori_lote` varchar(50) DEFAULT NULL,
  `ori_quantidade_solicitada` decimal(15,3) NOT NULL,
  `ori_quantidade_separada` decimal(15,3) DEFAULT 0.000,
  `ori_sequencia` int(11) DEFAULT NULL,
  `ori_status` enum('PENDENTE','SEPARADO','FALTA') DEFAULT 'PENDENTE'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe dos itens a serem separados em cada Ordem.';

CREATE TABLE IF NOT EXISTS `est_ordens_separacao` (
  `ors_id` int(11) NOT NULL COMMENT 'Identificador único da Ordem de Separação (Picking).',
  `ors_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `ors_numero` varchar(50) NOT NULL,
  `ors_pdv_id` int(11) DEFAULT NULL COMMENT 'Pedido de Venda relacionado (FK: vnd_pedidos).',
  `ors_dep_id` int(11) NOT NULL COMMENT 'Depósito de onde será separado (FK: est_depositos).',
  `ors_data_criacao` timestamp NOT NULL DEFAULT current_timestamp(),
  `ors_data_inicio` timestamp NULL DEFAULT NULL,
  `ors_data_conclusao` timestamp NULL DEFAULT NULL,
  `ors_prioridade` enum('BAIXA','NORMAL','ALTA','URGENTE') DEFAULT 'NORMAL',
  `ors_status` enum('PENDENTE','SEPARANDO','CONCLUIDA','CANCELADA') DEFAULT 'PENDENTE',
  `ors_usr_separador` int(11) DEFAULT NULL COMMENT 'Usuário responsável pela separação (FK: adm_usuarios).',
  `ors_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle das Ordens de Separação para pedidos de venda/transferência.';

CREATE TABLE IF NOT EXISTS `est_ponto_pedido` (
  `ptp_id` int(11) NOT NULL COMMENT 'Identificador único do Ponto de Pedido.',
  `ptp_itm_id` int(11) NOT NULL COMMENT 'Item (FK: vnd_itens).',
  `ptp_dep_id` int(11) NOT NULL COMMENT 'Depósito (FK: est_depositos).',
  `ptp_estoque_minimo` decimal(15,3) NOT NULL,
  `ptp_estoque_maximo` decimal(15,3) NOT NULL,
  `ptp_ponto_pedido` decimal(15,3) NOT NULL,
  `ptp_lote_compra` decimal(15,3) DEFAULT NULL,
  `ptp_lead_time_dias` int(11) DEFAULT NULL,
  `ptp_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Parâmetros para controle de ressuprimento (Estoque Mínimo, Ponto de Pedido).';

CREATE TABLE IF NOT EXISTS `est_saldos` (
  `sld_id` int(11) NOT NULL COMMENT 'Identificador único do Saldo de Estoque.',
  `sld_itm_id` int(11) NOT NULL COMMENT 'Item (Produto/Serviço) (FK: vnd_itens).',
  `sld_dep_id` int(11) NOT NULL COMMENT 'Depósito onde o saldo está (FK: est_depositos).',
  `sld_end_id` int(11) DEFAULT NULL COMMENT 'Endereço específico (FK: est_enderecos).',
  `sld_lote` varchar(50) DEFAULT NULL COMMENT 'Número do Lote (se aplicável).',
  `sld_serie` varchar(50) DEFAULT NULL COMMENT 'Número de Série (se aplicável).',
  `sld_data_validade` date DEFAULT NULL,
  `sld_quantidade` decimal(15,3) NOT NULL DEFAULT 0.000 COMMENT 'Quantidade física total.',
  `sld_quantidade_reservada` decimal(15,3) DEFAULT 0.000 COMMENT 'Quantidade comprometida (ex: em Ordens de Separação).',
  `sld_quantidade_disponivel` decimal(15,3) GENERATED ALWAYS AS (`sld_quantidade` - `sld_quantidade_reservada`) STORED COMMENT 'Quantidade pronta para uso/venda.',
  `sld_custo_unitario` decimal(15,2) DEFAULT NULL COMMENT 'Custo médio unitário (ou custo específico).',
  `sld_custo_total` decimal(15,2) GENERATED ALWAYS AS (`sld_quantidade` * `sld_custo_unitario`) STORED COMMENT 'Custo total em estoque.',
  `sld_data_ultima_movimentacao` timestamp NULL DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos saldos em estoque, por item, lote e endereço.';

CREATE TABLE IF NOT EXISTS `fin_aprovacoes_pagar` (
  `app_id` int(11) NOT NULL COMMENT 'Identificador único da Etapa de Aprovação.',
  `app_tpg_id` int(11) NOT NULL COMMENT 'ID do Título a Pagar a ser aprovado (FK: fin_titulos_pagar).',
  `app_usr_solicitante` int(11) NOT NULL COMMENT 'Usuário que solicitou a Aprovacao (FK: adm_usuarios).',
  `app_usr_aprovador` int(11) NOT NULL COMMENT 'Usuário que deve aprovar/rejeitar (FK: adm_usuarios).',
  `app_nivel` int(11) NOT NULL COMMENT 'Nível hierárquico da Aprovacao (pode haver múltiplos).',
  `app_status` enum('PENDENTE','APROVADO','REJEITADO') DEFAULT 'PENDENTE' COMMENT 'Status da etapa de aprovação.',
  `app_data_solicitacao` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data e hora da solicitação.',
  `app_data_resposta` timestamp NULL DEFAULT NULL COMMENT 'Data e hora da resposta do aprovador.',
  `app_observacoes` text DEFAULT NULL COMMENT 'Comentários do aprovador.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico do fluxo de Aprovacao de títulos a pagar.';

CREATE TABLE IF NOT EXISTS `fin_centros_custo` (
  `ccu_id` int(11) NOT NULL COMMENT 'Identificador único do Centro de Custo.',
  `ccu_codigo` varchar(20) NOT NULL COMMENT 'Código de referência do Centro de Custo.',
  `ccu_nome` varchar(200) NOT NULL COMMENT 'Nome do Centro de Custo (Ex: Marketing, Produção, Administrativo).',
  `ccu_descricao` text DEFAULT NULL COMMENT 'Descrição detalhada das responsabilidades.',
  `ccu_observacoes` text DEFAULT NULL COMMENT 'Observações e notas sobre o Centro de Custo',
  `ccu_tipo` enum('CUSTO','LUCRO','INVESTIMENTO','OPERACIONAL','ADMINISTRATIVO') DEFAULT 'CUSTO' COMMENT 'Classificação gerencial do centro',
  `ccu_pai_id` int(11) DEFAULT NULL COMMENT 'ID do Centro de Custo superior (para hierarquia).',
  `ccu_responsavel_usr_id` int(11) DEFAULT NULL COMMENT 'ID do usuário responsável pelo Centro (FK: adm_usuarios).',
  `ccu_status` char(1) NOT NULL DEFAULT 'S' COMMENT 'Status de atividade (S=Ativo, N=Inativo)',
  `ccu_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.',
  `ccu_data_alteracao` timestamp NULL DEFAULT NULL COMMENT 'Data e hora da última alteração do registro',
  `ccu_data_exclusao` timestamp NULL DEFAULT NULL COMMENT 'Data e hora da exclusão lógica do registro',
  `ccu_data_inclusao` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Estrutura para rateio de despesas e custos (Centros de Custo).';

CREATE TABLE IF NOT EXISTS `fin_conciliacoes` (
  `ccl_id` int(11) NOT NULL COMMENT 'Identificador único do Pareamento.',
  `ccl_ext_id` int(11) NOT NULL COMMENT 'ID do Extrato Bancário (FK: fin_extratos_bancarios).',
  `ccl_tipo_lancamento` enum('PAGAR','RECEBER','TRANSFERENCIA','AJUSTE') NOT NULL COMMENT 'Tipo de lançamento interno pareado.',
  `ccl_lancamento_id` int(11) DEFAULT NULL COMMENT 'ID do lançamento interno (tpg_id, trc_id, trf_id, etc.).',
  `ccl_diferenca` decimal(15,2) DEFAULT 0.00 COMMENT 'Diferença de valor entre o extrato e o lançamento interno.',
  `ccl_usr_id` int(11) NOT NULL COMMENT 'Usuário responsável pelo pareamento (FK: adm_usuarios).',
  `ccl_data_conciliacao` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data do pareamento.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Relação entre os movimentos bancários e os lançamentos internos.';

CREATE TABLE IF NOT EXISTS `fin_contas_bancarias` (
  `cba_id` int(11) NOT NULL COMMENT 'Identificador único da Conta Bancária.',
  `cba_emp_id` int(11) NOT NULL COMMENT 'Empresa/Filial proprietária da conta (FK: adm_empresas).',
  `cba_banco_codigo` varchar(10) NOT NULL COMMENT 'Código do Banco (Ex: 001 - Banco do Brasil).',
  `cba_banco_nome` varchar(100) NOT NULL COMMENT 'Nome completo do Banco.',
  `cba_agencia` varchar(10) NOT NULL COMMENT 'Número da Agência.',
  `cba_agencia_digito` varchar(2) DEFAULT NULL COMMENT 'Dígito verificador da Agência (se houver).',
  `cba_conta` varchar(20) NOT NULL COMMENT 'Número da Conta.',
  `cba_conta_digito` varchar(2) DEFAULT NULL COMMENT 'Dígito verificador da Conta (se houver).',
  `cba_tipo` enum('CORRENTE','POUPANCA','INVESTIMENTO','APLICACAO') NOT NULL COMMENT 'Tipo da conta bancária.',
  `cba_saldo_inicial` decimal(15,2) DEFAULT 0.00 COMMENT 'Saldo no momento da abertura do registro no ERP.',
  `cba_saldo_atual` decimal(15,2) DEFAULT 0.00 COMMENT 'Saldo atualizado (calculado pelas movimentações).',
  `cba_data_saldo` date DEFAULT NULL COMMENT 'Data da última atualização do saldo.',
  `cba_pct_id` int(11) DEFAULT NULL COMMENT 'Conta contábil (Plano de Contas) associada ao saldo da conta bancária (FK: fin_plano_contas).',
  `cba_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade.',
  `cba_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro das Contas Bancárias da empresa.';

CREATE TABLE IF NOT EXISTS `fin_extratos_bancarios` (
  `ext_id` int(11) NOT NULL COMMENT 'Identificador único do item do extrato.',
  `ext_cba_id` int(11) NOT NULL COMMENT 'Conta bancária de origem (FK: fin_contas_bancarias).',
  `ext_data` date NOT NULL COMMENT 'Data do movimento bancário.',
  `ext_historico` varchar(200) DEFAULT NULL COMMENT 'Descrição do lançamento no extrato.',
  `ext_documento` varchar(50) DEFAULT NULL COMMENT 'Número do documento ou transação.',
  `ext_valor` decimal(15,2) NOT NULL COMMENT 'Valor do movimento.',
  `ext_tipo` enum('CREDITO','DEBITO') NOT NULL COMMENT 'Tipo de movimento.',
  `ext_saldo` decimal(15,2) DEFAULT NULL COMMENT 'Saldo após a transação (se fornecido pelo extrato).',
  `ext_conciliado` tinyint(1) DEFAULT 0 COMMENT 'Indica se o item foi pareado com um lançamento interno.',
  `ext_data_conciliacao` timestamp NULL DEFAULT NULL COMMENT 'Data da Conciliacao.',
  `ext_usr_conciliacao` int(11) DEFAULT NULL COMMENT 'Usuário que conciliou (FK: adm_usuarios).',
  `ext_data_importacao` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de importação do extrato.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registros importados de extratos bancários.';

CREATE TABLE IF NOT EXISTS `fin_moedas` (
  `moe_id` int(11) NOT NULL COMMENT 'Identificador único da Moeda.',
  `moe_codigo` varchar(3) NOT NULL COMMENT 'Código ISO da Moeda (Ex: BRL, USD, EUR).',
  `moe_nome` varchar(50) NOT NULL COMMENT 'Nome completo da Moeda (Ex: Real Brasileiro).',
  `moe_simbolo` varchar(5) NOT NULL COMMENT 'Símbolo da Moeda (Ex: R$, $).',
  `moe_padrao` tinyint(1) DEFAULT 0 COMMENT 'Indica se esta é a moeda padrão (local) do sistema.',
  `moe_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de Moedas utilizadas nas transações.';

CREATE TABLE IF NOT EXISTS `fin_orcamentos` (
  `orc_id` int(11) NOT NULL COMMENT 'Identificador único do Orcamento.',
  `orc_emp_id` int(11) NOT NULL COMMENT 'Empresa à qual o Orcamento se refere (FK: adm_empresas).',
  `orc_ano` int(11) NOT NULL COMMENT 'Ano de vigência do Orcamento.',
  `orc_versao` int(11) NOT NULL DEFAULT 1 COMMENT 'Número da versão (ex: 1, 2, 3).',
  `orc_descricao` varchar(200) DEFAULT NULL COMMENT 'Nome ou descrição do Orcamento.',
  `orc_status` enum('ELABORACAO','APROVADO','VIGENTE','ENCERRADO') DEFAULT 'ELABORACAO' COMMENT 'Status do ciclo de vida do Orcamento.',
  `orc_data_inicio` date NOT NULL COMMENT 'Data de início da vigência.',
  `orc_data_fim` date NOT NULL COMMENT 'Data de fim da vigência.',
  `orc_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `orc_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cabeçalho dos Orcamentos Anuais (Budget).';

CREATE TABLE IF NOT EXISTS `fin_orcamento_itens` (
  `ori_id` int(11) NOT NULL COMMENT 'Identificador único da linha orçamentária.',
  `ori_orc_id` int(11) NOT NULL COMMENT 'Orcamento ao qual pertence (FK: fin_orcamentos).',
  `ori_pct_id` int(11) NOT NULL COMMENT 'Conta Contábil orçada (FK: fin_plano_contas).',
  `ori_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo orçado (FK: fin_centros_custo).',
  `ori_mes` int(11) NOT NULL COMMENT 'Mês orçado (1 a 12).',
  `ori_valor_orcado` decimal(15,2) NOT NULL COMMENT 'Valor previsto/orçado para o mês.',
  `ori_valor_realizado` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor efetivamente gasto/realizado (para acompanhamento).',
  `ori_observacoes` text DEFAULT NULL COMMENT 'Observações da linha orçamentária.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `fin_pagamentos` (
  `pag_id` int(11) NOT NULL COMMENT 'Identificador único do Pagamento (Baixa).',
  `pag_tpg_id` int(11) NOT NULL COMMENT 'ID do Título a Pagar baixado (FK: fin_titulos_pagar).',
  `pag_cba_id` int(11) NOT NULL COMMENT 'Conta Bancária de onde o valor foi debitado (FK: fin_contas_bancarias).',
  `pag_data_pagamento` date NOT NULL COMMENT 'Data efetiva da saída do dinheiro.',
  `pag_valor` decimal(15,2) NOT NULL COMMENT 'Valor líquido do pagamento.',
  `pag_forma` enum('DINHEIRO','CHEQUE','TED','DOC','PIX','BOLETO','CARTAO','DEBITO') NOT NULL COMMENT 'Meio de pagamento utilizado.',
  `pag_numero_documento` varchar(50) DEFAULT NULL COMMENT 'Número do cheque/comprovante/transação.',
  `pag_observacoes` text DEFAULT NULL COMMENT 'Observações sobre a baixa.',
  `pag_usr_id` int(11) NOT NULL COMMENT 'Usuário que efetuou o registro da baixa (FK: adm_usuarios).',
  `pag_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe das baixas (pagamentos) realizadas nos títulos a pagar.';

CREATE TABLE IF NOT EXISTS `fin_plano_contas` (
  `pct_id` int(11) NOT NULL COMMENT 'Identificador único da Conta.',
  `pct_codigo` varchar(20) NOT NULL COMMENT 'Código hierárquico da conta (Ex: 1.01.001).',
  `pct_nome` varchar(200) NOT NULL COMMENT 'Nome da Conta (Ex: Ativo Circulante, Despesas Operacionais).',
  `pct_nivel` int(11) NOT NULL COMMENT 'Nível hierárquico (Ex: 1, 2, 3...).',
  `pct_pai_id` int(11) DEFAULT NULL COMMENT 'ID da Conta Pai para formação da hierarquia (FK: fin_plano_contas).',
  `pct_tipo` enum('ANALITICA','SINTETICA') NOT NULL COMMENT 'Tipo da conta: Analítica (aceita lançamento) ou Sintética (soma de filhas).',
  `pct_natureza` enum('DEVEDORA','CREDORA') NOT NULL COMMENT 'Natureza da conta para saldos e lançamentos (Débito/Crédito).',
  `pct_classe` enum('ATIVO','PASSIVO','RECEITA','CUSTO','DESPESA','PATRIMONIO') NOT NULL COMMENT 'Classe contábil (DRE/BP).',
  `pct_aceita_lancamento` tinyint(1) DEFAULT 0 COMMENT 'Indica se a conta pode receber lançamentos diretamente (deve ser TRUE para Analíticas).',
  `pct_ordem` int(11) DEFAULT NULL COMMENT 'Ordem de exibição em relatórios.',
  `pct_ativo` tinyint(1) DEFAULT 1 COMMENT 'Status de atividade.',
  `pct_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Estrutura hierárquica do Plano de Contas Gerencial.';

CREATE TABLE IF NOT EXISTS `fin_recebimentos` (
  `rec_id` int(11) NOT NULL COMMENT 'Identificador único do Recebimento (Baixa).',
  `rec_trc_id` int(11) NOT NULL COMMENT 'ID do Título a Receber baixado (FK: fin_titulos_receber).',
  `rec_cba_id` int(11) NOT NULL COMMENT 'Conta Bancária onde o valor foi depositado (FK: fin_contas_bancarias).',
  `rec_data_recebimento` date NOT NULL COMMENT 'Data efetiva da entrada do dinheiro.',
  `rec_valor` decimal(15,2) NOT NULL COMMENT 'Valor líquido do recebimento.',
  `rec_forma` enum('DINHEIRO','CHEQUE','TED','DOC','PIX','BOLETO','CARTAO') NOT NULL COMMENT 'Meio de pagamento utilizado.',
  `rec_numero_documento` varchar(50) DEFAULT NULL COMMENT 'Número do cheque/comprovante/transação.',
  `rec_observacoes` text DEFAULT NULL COMMENT 'Observações sobre a baixa.',
  `rec_usr_id` int(11) NOT NULL COMMENT 'Usuário que efetuou o registro da baixa (FK: adm_usuarios).',
  `rec_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhe das baixas (recebimentos) realizadas nos títulos.';

CREATE TABLE IF NOT EXISTS `fin_taxas_cambio` (
  `txc_id` int(11) NOT NULL COMMENT 'Identificador único da Taxa de Câmbio.',
  `txc_moe_id` int(11) NOT NULL COMMENT 'ID da Moeda estrangeira (FK: fin_moedas).',
  `txc_data` date NOT NULL COMMENT 'Data de validade da taxa.',
  `txc_taxa_compra` decimal(15,6) NOT NULL COMMENT 'Taxa de câmbio para compra da moeda.',
  `txc_taxa_venda` decimal(15,6) NOT NULL COMMENT 'Taxa de câmbio para venda da moeda.',
  `txc_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de registro no sistema.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico de Taxas de Câmbio diárias.';

CREATE TABLE IF NOT EXISTS `fin_titulos_pagar` (
  `tpg_id` int(11) NOT NULL COMMENT 'Identificador único do Título a Pagar.',
  `tpg_emp_id` int(11) NOT NULL COMMENT 'Empresa/Filial devedora (FK: adm_empresas).',
  `tpg_numero` varchar(50) DEFAULT NULL COMMENT 'Número do título (gerado automaticamente ou manual).',
  `tpg_tipo` enum('FORNECEDOR','EMPRESTIMO','FOLHA','IMPOSTO','OUTROS') NOT NULL COMMENT 'Origem da obrigação (Tipo: Fornecedor, Imposto, etc.).',
  `tpg_pes_id` int(11) NOT NULL COMMENT 'ID do Fornecedor/Credor (FK: adm_pessoas).',
  `tpg_documento` varchar(50) DEFAULT NULL COMMENT 'Referência ao documento de origem (NFe, Boleto, Contrato).',
  `tpg_parcela` varchar(10) DEFAULT NULL COMMENT 'Número da parcela (Ex: 01/05).',
  `tpg_data_emissao` date NOT NULL COMMENT 'Data de emissão/lançamento do título.',
  `tpg_data_vencimento` date NOT NULL COMMENT 'Data de vencimento da obrigação.',
  `tpg_data_pagamento` date DEFAULT NULL COMMENT 'Data efetiva do pagamento (baixa total ou parcial).',
  `tpg_valor_original` decimal(15,2) NOT NULL COMMENT 'Valor base do título.',
  `tpg_valor_juros` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de juros pago.',
  `tpg_valor_multa` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de multa pago.',
  `tpg_valor_desconto` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de desconto obtido no pagamento.',
  `tpg_valor_pago` decimal(15,2) DEFAULT 0.00 COMMENT 'Soma dos valores pagos (para controle de saldo).',
  `tpg_pct_id` int(11) NOT NULL COMMENT 'Conta contábil (Despesa/Custo) associada (FK: fin_plano_contas).',
  `tpg_ccu_id` int(11) NOT NULL COMMENT 'Centro de Custo que arca com a despesa (FK: fin_centros_custo).',
  `tpg_cba_id` int(11) DEFAULT NULL COMMENT 'Conta bancária de débito esperada (FK: fin_contas_bancarias).',
  `tpg_status` enum('ABERTO','APROVADO','PAGO','CANCELADO','VENCIDO','PARCIAL') DEFAULT 'ABERTO' COMMENT 'Status atual do título.',
  `tpg_observacoes` text DEFAULT NULL COMMENT 'Observações pertinentes ao título.',
  `tpg_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que registrou o título (FK: adm_usuarios).',
  `tpg_usr_aprovacao` int(11) DEFAULT NULL COMMENT 'Usuário que aprovou o título (FK: adm_usuarios).',
  `tpg_data_aprovacao` timestamp NULL DEFAULT NULL COMMENT 'Data e hora da aprovação.',
  `tpg_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.',
  `tpg_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Data de última alteração.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle dos valores a serem pagos a fornecedores e credores.';

CREATE TABLE IF NOT EXISTS `fin_titulos_receber` (
  `trc_id` int(11) NOT NULL COMMENT 'Identificador único do Título a Receber.',
  `trc_emp_id` int(11) NOT NULL COMMENT 'Empresa/Filial que receberá (FK: adm_empresas).',
  `trc_numero` varchar(50) DEFAULT NULL COMMENT 'Número do título (gerado automaticamente ou manual).',
  `trc_tipo` enum('VENDA','SERVICO','OUTROS') NOT NULL COMMENT 'Origem do recebimento (Venda, Serviço, etc.).',
  `trc_pes_id` int(11) NOT NULL COMMENT 'ID do Cliente pagador (FK: adm_pessoas).',
  `trc_documento` varchar(50) DEFAULT NULL COMMENT 'Referência ao documento de origem (NF, Contrato).',
  `trc_parcela` varchar(10) DEFAULT NULL COMMENT 'Número da parcela (Ex: 01/05).',
  `trc_data_emissao` date NOT NULL COMMENT 'Data de emissão do título.',
  `trc_data_vencimento` date NOT NULL COMMENT 'Data de vencimento original.',
  `trc_data_recebimento` date DEFAULT NULL COMMENT 'Data efetiva do recebimento (baixa total ou parcial).',
  `trc_valor_original` decimal(15,2) NOT NULL COMMENT 'Valor base do título.',
  `trc_valor_juros` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de juros cobrado no recebimento.',
  `trc_valor_multa` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de multa cobrado no recebimento.',
  `trc_valor_desconto` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor de desconto concedido no recebimento.',
  `trc_valor_recebido` decimal(15,2) DEFAULT 0.00 COMMENT 'Soma dos valores recebidos (para controle de saldo).',
  `trc_pct_id` int(11) NOT NULL COMMENT 'Conta contábil (Receita) associada ao título (FK: fin_plano_contas).',
  `trc_cba_id` int(11) DEFAULT NULL COMMENT 'Conta bancária de destino esperada (FK: fin_contas_bancarias).',
  `trc_status` enum('ABERTO','RECEBIDO','CANCELADO','VENCIDO','PARCIAL') DEFAULT 'ABERTO' COMMENT 'Status atual do título. Adicionado PARCIAL.',
  `trc_observacoes` text DEFAULT NULL COMMENT 'Observações pertinentes ao título.',
  `trc_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que registrou o título (FK: adm_usuarios).',
  `trc_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp() COMMENT 'Data de criação do registro.',
  `trc_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp() COMMENT 'Data de última alteração.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle dos valores a serem recebidos de clientes e outras fontes.';

CREATE TABLE IF NOT EXISTS `fis_apuracao_impostos` (
  `api_id` int(11) NOT NULL COMMENT 'Identificador único da Apuração.',
  `api_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `api_periodo` date NOT NULL COMMENT 'Mês/Ano de apuração.',
  `api_imposto` enum('ICMS','IPI','PIS','COFINS','IRPJ','CSLL','ISS') NOT NULL,
  `api_base_calculo` decimal(15,2) DEFAULT 0.00,
  `api_valor_debito` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor a pagar/débito.',
  `api_valor_credito` decimal(15,2) DEFAULT 0.00 COMMENT 'Valor a compensar/crédito.',
  `api_saldo_anterior` decimal(15,2) DEFAULT 0.00 COMMENT 'Saldo credor transportado do período anterior.',
  `api_valor_a_recolher` decimal(15,2) GENERATED ALWAYS AS (`api_valor_debito` - `api_valor_credito` + `api_saldo_anterior`) STORED COMMENT 'Valor final devido no período.',
  `api_saldo_credor` decimal(15,2) DEFAULT 0.00 COMMENT 'Saldo credor a transportar para o próximo período.',
  `api_data_vencimento` date DEFAULT NULL,
  `api_pago` tinyint(1) DEFAULT 0,
  `api_tpg_id` int(11) DEFAULT NULL COMMENT 'ID da Conta a Pagar/Transação de Pagamento (FK: fin_transacoes_pagamento).',
  `api_observacoes` text DEFAULT NULL,
  `api_data_apuracao` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro da apuração periódica de impostos.';

CREATE TABLE IF NOT EXISTS `fis_sped_contabil` (
  `spc_id` int(11) NOT NULL COMMENT 'Identificador único do Registro SPED Contábil.',
  `spc_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `spc_ano` int(11) NOT NULL COMMENT 'Ano calendário de referência.',
  `spc_tipo` enum('ECD','ECF') NOT NULL COMMENT 'Tipo de SPED Contábil (Escrituração Contábil Digital/Fiscal).',
  `spc_arquivo` longtext DEFAULT NULL COMMENT 'Conteúdo completo do arquivo gerado.',
  `spc_nome_arquivo` varchar(200) DEFAULT NULL COMMENT 'Nome do arquivo para download.',
  `spc_data_geracao` timestamp NOT NULL DEFAULT current_timestamp(),
  `spc_status` enum('GERADO','VALIDADO','TRANSMITIDO','ERRO') DEFAULT 'GERADO',
  `spc_protocolo` varchar(50) DEFAULT NULL COMMENT 'Protocolo de transmissão.',
  `spc_mensagem_erro` text DEFAULT NULL,
  `spc_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário que gerou o SPED (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos arquivos e status de geração do SPED Contábil (ECD/ECF).';

CREATE TABLE IF NOT EXISTS `fis_sped_fiscal` (
  `spf_id` int(11) NOT NULL COMMENT 'Identificador único do Registro SPED Fiscal.',
  `spf_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `spf_periodo` date NOT NULL COMMENT 'Mês/Ano de referência (Ex: 2025-11-01).',
  `spf_tipo` enum('ICMS_IPI','CONTRIBUICOES') NOT NULL COMMENT 'Bloco/Tipo do SPED.',
  `spf_arquivo` longtext DEFAULT NULL COMMENT 'Conteúdo completo do arquivo gerado (pode ser grande).',
  `spf_nome_arquivo` varchar(200) DEFAULT NULL COMMENT 'Nome do arquivo para download.',
  `spf_data_geracao` timestamp NOT NULL DEFAULT current_timestamp(),
  `spf_status` enum('GERADO','VALIDADO','TRANSMITIDO','ERRO') DEFAULT 'GERADO',
  `spf_protocolo` varchar(50) DEFAULT NULL COMMENT 'Protocolo de transmissão, se aplicável.',
  `spf_mensagem_erro` text DEFAULT NULL,
  `spf_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário que gerou o SPED (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos arquivos e status de geração do SPED Fiscal (EFD).';

CREATE TABLE IF NOT EXISTS `fis_tributacao_ncm` (
  `tnc_id` int(11) NOT NULL COMMENT 'Identificador único da Regra.',
  `tnc_ncm` varchar(10) NOT NULL COMMENT 'Nomenclatura Comum do Mercosul (8 dígitos).',
  `tnc_uf_origem` char(2) NOT NULL,
  `tnc_uf_destino` char(2) NOT NULL,
  `tnc_cfop` varchar(5) NOT NULL COMMENT 'Código Fiscal de Operações e Prestações.',
  `tnc_cst_icms` varchar(3) DEFAULT NULL COMMENT 'Código de Situação Tributária do ICMS (Ex: 000, 101).',
  `tnc_aliquota_icms` decimal(5,2) DEFAULT 0.00,
  `tnc_reducao_bc_icms` decimal(5,2) DEFAULT 0.00 COMMENT 'Percentual de redução na Base de Cálculo do ICMS.',
  `tnc_aliquota_icms_st` decimal(5,2) DEFAULT 0.00 COMMENT 'Alíquota de ICMS Substituição Tributária.',
  `tnc_mva_st` decimal(5,2) DEFAULT 0.00 COMMENT 'Margem de Valor Agregado (MVA) para ST.',
  `tnc_cst_ipi` varchar(2) DEFAULT NULL COMMENT 'Código de Situação Tributária do IPI.',
  `tnc_aliquota_ipi` decimal(5,2) DEFAULT 0.00,
  `tnc_cst_pis` varchar(2) DEFAULT NULL COMMENT 'Código de Situação Tributária do PIS.',
  `tnc_aliquota_pis` decimal(5,2) DEFAULT 0.00,
  `tnc_cst_cofins` varchar(2) DEFAULT NULL COMMENT 'Código de Situação Tributária do COFINS.',
  `tnc_aliquota_cofins` decimal(5,2) DEFAULT 0.00,
  `tnc_data_inicio` date NOT NULL COMMENT 'Data de início de vigência da regra.',
  `tnc_data_fim` date DEFAULT NULL COMMENT 'Data final de vigência da regra.',
  `tnc_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Matriz de regras tributárias por NCM, UF de origem e destino.';

CREATE TABLE IF NOT EXISTS `jur_aditivos` (
  `adt_id` int(11) NOT NULL COMMENT 'Identificador único do Aditivo.',
  `adt_ctr_id` int(11) NOT NULL COMMENT 'Contrato principal (FK: jur_contratos).',
  `adt_numero` varchar(20) NOT NULL,
  `adt_tipo` enum('PRAZO','VALOR','OBJETO','RESCISAO','OUTROS') NOT NULL,
  `adt_data` date NOT NULL,
  `adt_descricao` text NOT NULL,
  `adt_valor_anterior` decimal(15,2) DEFAULT NULL,
  `adt_valor_novo` decimal(15,2) DEFAULT NULL,
  `adt_data_fim_anterior` date DEFAULT NULL,
  `adt_data_fim_nova` date DEFAULT NULL,
  `adt_arquivo` varchar(200) DEFAULT NULL COMMENT 'Caminho/URL do arquivo do aditivo.',
  `adt_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `adt_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle de aditivos e alterações contratuais.';

CREATE TABLE IF NOT EXISTS `jur_andamentos` (
  `and_id` int(11) NOT NULL COMMENT 'Identificador único do Andamento.',
  `and_prc_id` int(11) NOT NULL COMMENT 'Processo Judicial (FK: jur_processos).',
  `and_data` date NOT NULL,
  `and_tipo` enum('PETICAO','AUDIENCIA','SENTENCA','RECURSO','PUBLICACAO','INTIMACAO','OUTROS') NOT NULL,
  `and_descricao` text NOT NULL,
  `and_proximo_prazo` date DEFAULT NULL,
  `and_usr_responsavel` int(11) DEFAULT NULL COMMENT 'Usuário que registrou (FK: adm_usuarios).',
  `and_documento` varchar(200) DEFAULT NULL COMMENT 'Referência ao documento protocolado.',
  `and_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Histórico de eventos e andamentos de um processo.';

CREATE TABLE IF NOT EXISTS `jur_base_conhecimento` (
  `bco_id` int(11) NOT NULL COMMENT 'Identificador único do Item da Base.',
  `bco_titulo` varchar(200) NOT NULL,
  `bco_tipo` enum('LEGISLACAO','JURISPRUDENCIA','PARECER','SUMULA','DOUTRINA','OUTROS') NOT NULL,
  `bco_area` enum('CIVEL','TRABALHISTA','FISCAL','CRIMINAL','REGULATORIO','OUTROS') NOT NULL,
  `bco_conteudo` longtext NOT NULL,
  `bco_fonte` varchar(200) DEFAULT NULL,
  `bco_data_publicacao` date DEFAULT NULL,
  `bco_tags` varchar(500) DEFAULT NULL,
  `bco_arquivo` varchar(200) DEFAULT NULL COMMENT 'Caminho/URL do arquivo original (ex: lei, decisão).',
  `bco_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que cadastrou (FK: adm_usuarios).',
  `bco_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Repositório de documentos e textos jurídicos para consulta interna.';

CREATE TABLE IF NOT EXISTS `jur_certidoes` (
  `cer_id` int(11) NOT NULL COMMENT 'Identificador único da Certidão/Licença.',
  `cer_emp_id` int(11) NOT NULL COMMENT 'Empresa relacionada (FK: adm_empresas).',
  `cer_tipo` enum('CND_FEDERAL','CND_ESTADUAL','CND_MUNICIPAL','CNDT','FGTS','ALVARA','LICENCA_AMBIENTAL','OUTROS') NOT NULL,
  `cer_descricao` varchar(200) NOT NULL,
  `cer_orgao_emissor` varchar(100) DEFAULT NULL,
  `cer_numero` varchar(50) DEFAULT NULL,
  `cer_data_emissao` date NOT NULL,
  `cer_data_validade` date NOT NULL,
  `cer_dias_antecedencia_alerta` int(11) DEFAULT 30,
  `cer_data_alerta` date GENERATED ALWAYS AS (`cer_data_validade` - interval `cer_dias_antecedencia_alerta` day) STORED COMMENT 'Data de disparo do alerta de vencimento.',
  `cer_arquivo` varchar(200) DEFAULT NULL COMMENT 'Caminho/URL do arquivo da certidão.',
  `cer_renovado` tinyint(1) DEFAULT 0,
  `cer_observacoes` text DEFAULT NULL,
  `cer_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável pela renovação/cadastro (FK: adm_usuarios).',
  `cer_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle de certidões negativas e licenças com prazo de validade.';

CREATE TABLE IF NOT EXISTS `jur_contratos` (
  `ctr_id` int(11) NOT NULL COMMENT 'Identificador único do Contrato.',
  `ctr_emp_id` int(11) NOT NULL COMMENT 'Empresa que é parte (FK: adm_empresas).',
  `ctr_numero` varchar(50) NOT NULL,
  `ctr_tipo` enum('COMPRA','VENDA','SERVICO','LOCACAO','TRABALHO','PARCERIA','OUTROS') NOT NULL,
  `ctr_parte_pes_id` int(11) NOT NULL COMMENT 'Pessoa/Empresa da outra parte (FK: adm_pessoas).',
  `ctr_objeto` text NOT NULL,
  `ctr_valor_total` decimal(15,2) DEFAULT NULL,
  `ctr_data_inicio` date NOT NULL,
  `ctr_data_fim` date DEFAULT NULL,
  `ctr_prazo_meses` int(11) DEFAULT NULL,
  `ctr_renovacao_automatica` tinyint(1) DEFAULT 0,
  `ctr_dias_aviso_vencimento` int(11) DEFAULT 30 COMMENT 'Dias para notificar sobre o vencimento/renovação.',
  `ctr_status` enum('MINUTANDO','VIGENTE','SUSPENSO','RESCINDIDO','CONCLUIDO') DEFAULT 'MINUTANDO',
  `ctr_arquivo` varchar(200) DEFAULT NULL COMMENT 'Caminho/URL do arquivo PDF do contrato.',
  `ctr_observacoes` text DEFAULT NULL,
  `ctr_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável pelo contrato (FK: adm_usuarios).',
  `ctr_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp(),
  `ctr_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro e gestão de todos os contratos firmados pela empresa.';

CREATE TABLE IF NOT EXISTS `jur_contrato_clausulas` (
  `ccl_id` int(11) NOT NULL COMMENT 'Identificador único da Cláusula.',
  `ccl_ctr_id` int(11) NOT NULL COMMENT 'Contrato (FK: jur_contratos).',
  `ccl_numero` varchar(10) NOT NULL COMMENT 'Número da cláusula (ex: 1.1, II).',
  `ccl_titulo` varchar(200) NOT NULL,
  `ccl_texto` text NOT NULL,
  `ccl_ordem` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Estrutura detalhada das cláusulas de um contrato.';

CREATE TABLE IF NOT EXISTS `jur_depositos_judiciais` (
  `dpj_id` int(11) NOT NULL COMMENT 'Identificador único do Depósito.',
  `dpj_prc_id` int(11) NOT NULL COMMENT 'Processo Judicial (FK: jur_processos).',
  `dpj_tipo` enum('RECURSAL','GARANTIA','EXECUCAO','OUTROS') NOT NULL,
  `dpj_valor` decimal(15,2) NOT NULL,
  `dpj_data_deposito` date NOT NULL,
  `dpj_cba_id` int(11) NOT NULL COMMENT 'Conta Bancária de origem (FK: fin_contas_bancarias).',
  `dpj_agencia` varchar(10) DEFAULT NULL COMMENT 'Agência judicial (se for conta específica).',
  `dpj_conta` varchar(20) DEFAULT NULL COMMENT 'Conta judicial (se for conta específica).',
  `dpj_comprovante` varchar(200) DEFAULT NULL COMMENT 'Referência ao comprovante de depósito.',
  `dpj_levantado` tinyint(1) DEFAULT 0 COMMENT 'Foi resgatado/levantado?',
  `dpj_data_levantamento` date DEFAULT NULL,
  `dpj_valor_levantado` decimal(15,2) DEFAULT NULL,
  `dpj_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle dos depósitos realizados em juízo.';

CREATE TABLE IF NOT EXISTS `jur_prazos` (
  `prz_id` int(11) NOT NULL COMMENT 'Identificador único do Prazo.',
  `prz_prc_id` int(11) NOT NULL COMMENT 'Processo Judicial (FK: jur_processos).',
  `prz_and_id` int(11) DEFAULT NULL COMMENT 'Andamento que originou o prazo (FK: jur_andamentos).',
  `prz_tipo` varchar(100) NOT NULL,
  `prz_descricao` text NOT NULL,
  `prz_data_limite` date NOT NULL,
  `prz_dias_antecedencia` int(11) DEFAULT 5 COMMENT 'Dias para notificação antes do vencimento.',
  `prz_data_alerta` date GENERATED ALWAYS AS (`prz_data_limite` - interval `prz_dias_antecedencia` day) STORED COMMENT 'Data de disparo do alerta.',
  `prz_cumprido` tinyint(1) DEFAULT 0,
  `prz_data_cumprimento` date DEFAULT NULL,
  `prz_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável por cumprir o prazo (FK: adm_usuarios).',
  `prz_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle e monitoramento dos prazos processuais.';

CREATE TABLE IF NOT EXISTS `jur_processos` (
  `prc_id` int(11) NOT NULL COMMENT 'Identificador único do Processo Judicial.',
  `prc_emp_id` int(11) NOT NULL COMMENT 'Empresa que é parte no processo (FK: adm_empresas).',
  `prc_numero_processo` varchar(50) NOT NULL COMMENT 'Número único do processo (CNJ).',
  `prc_tac_id` int(11) NOT NULL COMMENT 'Tipo de Ação (FK: jur_tipos_acao).',
  `prc_vara` varchar(100) DEFAULT NULL,
  `prc_comarca` varchar(100) DEFAULT NULL,
  `prc_uf` char(2) DEFAULT NULL,
  `prc_polo` enum('ATIVO','PASSIVO') NOT NULL COMMENT 'Empresa como autora ou ré.',
  `prc_parte_contraria` varchar(200) DEFAULT NULL,
  `prc_advogado_pes_id` int(11) DEFAULT NULL COMMENT 'Pessoa relacionada ao advogado responsável (FK: adm_pessoas).',
  `prc_valor_causa` decimal(15,2) NOT NULL,
  `prc_valor_condenacao` decimal(15,2) DEFAULT NULL COMMENT 'Valor final em caso de perda.',
  `prc_risco` enum('REMOTO','POSSIVEL','PROVAVEL') NOT NULL COMMENT 'Classificação de risco de perda.',
  `prc_percentual_risco` decimal(5,2) DEFAULT NULL,
  `prc_valor_provisao` decimal(15,2) GENERATED ALWAYS AS (`prc_valor_causa` * (`prc_percentual_risco` / 100)) STORED COMMENT 'Valor reservado contabilmente (provisão).',
  `prc_data_distribuicao` date NOT NULL,
  `prc_data_citacao` date DEFAULT NULL,
  `prc_data_sentenca` date DEFAULT NULL,
  `prc_data_transito_julgado` date DEFAULT NULL,
  `prc_status` enum('ANDAMENTO','SUSPENSO','ARQUIVADO','GANHO','PERDIDO','ACORDO') DEFAULT 'ANDAMENTO',
  `prc_objeto` text DEFAULT NULL COMMENT 'Resumo do objeto da ação.',
  `prc_observacoes` text DEFAULT NULL,
  `prc_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável pelo acompanhamento (FK: adm_usuarios).',
  `prc_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp(),
  `prc_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle e gestão de processos judiciais.';

CREATE TABLE IF NOT EXISTS `jur_tipos_acao` (
  `tac_id` int(11) NOT NULL COMMENT 'Identificador único do Tipo de Ação.',
  `tac_codigo` varchar(20) NOT NULL,
  `tac_nome` varchar(100) NOT NULL,
  `tac_area` enum('CIVEL','TRABALHISTA','FISCAL','CRIMINAL','REGULATORIO','OUTROS') NOT NULL,
  `tac_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de tipos de ações judiciais e suas áreas.';

CREATE TABLE IF NOT EXISTS `log_auditoria_fretes` (
  `auf_id` int(11) NOT NULL COMMENT 'Identificador único da Auditoria.',
  `auf_cte_id` int(11) NOT NULL COMMENT 'CT-e auditado (FK: log_cte).',
  `auf_valor_cobrado` decimal(15,2) NOT NULL COMMENT 'Valor cobrado pela transportadora.',
  `auf_valor_calculado` decimal(15,2) NOT NULL COMMENT 'Valor calculado pelo sistema (baseado nas tabelas).',
  `auf_diferenca` decimal(15,2) GENERATED ALWAYS AS (`auf_valor_cobrado` - `auf_valor_calculado`) STORED,
  `auf_status` enum('PENDENTE','APROVADO','CONTESTADO','AJUSTADO') DEFAULT 'PENDENTE',
  `auf_observacoes` text DEFAULT NULL,
  `auf_usr_auditor` int(11) DEFAULT NULL COMMENT 'Usuário auditor (FK: adm_usuarios).',
  `auf_data_auditoria` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Auditoria dos valores de frete cobrados versus os valores previstos.';

CREATE TABLE IF NOT EXISTS `log_cte` (
  `cte_id` int(11) NOT NULL COMMENT 'Identificador único do CT-e.',
  `cte_emp_id` int(11) NOT NULL COMMENT 'Empresa emitente (FK: adm_empresas).',
  `cte_numero` varchar(20) NOT NULL,
  `cte_serie` varchar(5) NOT NULL,
  `cte_modelo` varchar(5) NOT NULL DEFAULT '57',
  `cte_chave_acesso` varchar(44) DEFAULT NULL,
  `cte_nfs_id` int(11) DEFAULT NULL COMMENT 'Nota Fiscal de Saída relacionada (FK: vnd_notas_fiscais).',
  `cte_tra_id` int(11) NOT NULL COMMENT 'Transportadora (FK: log_transportadoras).',
  `cte_remetente_pes_id` int(11) NOT NULL COMMENT 'Remetente (FK: adm_pessoas).',
  `cte_destinatario_pes_id` int(11) NOT NULL COMMENT 'Destinatário (FK: adm_pessoas).',
  `cte_data_emissao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `cte_tipo_servico` enum('NORMAL','SUBCONTRATACAO','REDESPACHO','REDESPACHO_INTERMEDIARIO') DEFAULT 'NORMAL',
  `cte_modal` enum('RODOVIARIO','AEREO','MARITIMO','FERROVIARIO') DEFAULT 'RODOVIARIO',
  `cte_valor_servico` decimal(15,2) NOT NULL,
  `cte_valor_receber` decimal(15,2) NOT NULL,
  `cte_cfop` varchar(5) NOT NULL,
  `cte_natureza_operacao` varchar(100) NOT NULL,
  `cte_peso_bruto` decimal(10,2) DEFAULT NULL,
  `cte_peso_cubado` decimal(10,2) DEFAULT NULL,
  `cte_quantidade_volumes` int(11) DEFAULT NULL,
  `cte_status` enum('DIGITACAO','TRANSMITIDO','AUTORIZADO','CANCELADO','DENEGADO') DEFAULT 'DIGITACAO',
  `cte_xml_envio` longtext DEFAULT NULL,
  `cte_xml_retorno` longtext DEFAULT NULL,
  `cte_protocolo` varchar(50) DEFAULT NULL,
  `cte_observacoes` text DEFAULT NULL,
  `cte_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário (FK: adm_usuarios).',
  `cte_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Conhecimentos de Transporte Eletrônico (CT-e) emitidos/recebidos.';

CREATE TABLE IF NOT EXISTS `log_frete_faixas` (
  `ffx_id` int(11) NOT NULL COMMENT 'Identificador único da Faixa de Frete.',
  `ffx_tfr_id` int(11) NOT NULL COMMENT 'Tabela de Frete (FK: log_tabelas_frete).',
  `ffx_cep_origem` varchar(9) DEFAULT NULL,
  `ffx_cep_destino` varchar(9) DEFAULT NULL,
  `ffx_uf_origem` char(2) DEFAULT NULL,
  `ffx_uf_destino` char(2) DEFAULT NULL,
  `ffx_peso_inicial` decimal(10,2) DEFAULT NULL,
  `ffx_peso_final` decimal(10,2) DEFAULT NULL,
  `ffx_valor_inicial` decimal(15,2) DEFAULT NULL,
  `ffx_valor_final` decimal(15,2) DEFAULT NULL,
  `ffx_preco_frete` decimal(15,2) NOT NULL,
  `ffx_prazo_entrega_dias` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Faixas de peso/valor/região para cálculo do preço e prazo do frete.';

CREATE TABLE IF NOT EXISTS `log_mdfe` (
  `mdf_id` int(11) NOT NULL COMMENT 'Identificador único do MDF-e.',
  `mdf_emp_id` int(11) NOT NULL COMMENT 'Empresa emitente (FK: adm_empresas).',
  `mdf_numero` varchar(20) NOT NULL,
  `mdf_serie` varchar(5) NOT NULL,
  `mdf_modelo` varchar(5) NOT NULL DEFAULT '58',
  `mdf_chave_acesso` varchar(44) DEFAULT NULL,
  `mdf_tra_id` int(11) NOT NULL COMMENT 'Transportadora (FK: log_transportadoras).',
  `mdf_data_emissao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `mdf_uf_inicio` char(2) NOT NULL,
  `mdf_uf_fim` char(2) NOT NULL,
  `mdf_tipo_emitente` enum('PROPRIO','TERCEIRO') DEFAULT 'PROPRIO',
  `mdf_modal` enum('RODOVIARIO','AEREO','AQUAVIARIO','FERROVIARIO') DEFAULT 'RODOVIARIO',
  `mdf_veiculo_placa` varchar(10) DEFAULT NULL,
  `mdf_veiculo_uf` char(2) DEFAULT NULL,
  `mdf_motorista_cpf` varchar(14) DEFAULT NULL,
  `mdf_motorista_nome` varchar(100) DEFAULT NULL,
  `mdf_peso_bruto` decimal(10,2) DEFAULT NULL,
  `mdf_valor_total` decimal(15,2) DEFAULT NULL,
  `mdf_status` enum('DIGITACAO','TRANSMITIDO','AUTORIZADO','ENCERRADO','CANCELADO') DEFAULT 'DIGITACAO',
  `mdf_xml_envio` longtext DEFAULT NULL,
  `mdf_xml_retorno` longtext DEFAULT NULL,
  `mdf_protocolo` varchar(50) DEFAULT NULL,
  `mdf_observacoes` text DEFAULT NULL,
  `mdf_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário (FK: adm_usuarios).',
  `mdf_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Manifestos de Documentos Fiscais (MDF-e) para transporte de carga.';

CREATE TABLE IF NOT EXISTS `log_mdfe_documentos` (
  `mdd_id` int(11) NOT NULL COMMENT 'Identificador único do Documento no MDF-e.',
  `mdd_mdf_id` int(11) NOT NULL COMMENT 'Manifesto (FK: log_mdfe).',
  `mdd_tipo` enum('CTE','NFE') NOT NULL,
  `mdd_chave_acesso` varchar(44) NOT NULL,
  `mdd_cte_id` int(11) DEFAULT NULL COMMENT 'CT-e relacionado (FK: log_cte).',
  `mdd_nfs_id` int(11) DEFAULT NULL COMMENT 'NF de Saída relacionada (FK: vnd_notas_fiscais).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Lista de documentos fiscais (NF-e, CT-e) vinculados a um MDF-e.';

CREATE TABLE IF NOT EXISTS `log_tabelas_frete` (
  `tfr_id` int(11) NOT NULL COMMENT 'Identificador único da Tabela de Frete.',
  `tfr_tra_id` int(11) NOT NULL COMMENT 'Transportadora (FK: log_transportadoras).',
  `tfr_nome` varchar(100) NOT NULL,
  `tfr_modal` enum('RODOVIARIO','AEREO','MARITIMO','FERROVIARIO') NOT NULL,
  `tfr_tipo_calculo` enum('PESO','VALOR','PESO_OU_VALOR','CUBAGEM') NOT NULL,
  `tfr_data_inicio` date NOT NULL,
  `tfr_data_fim` date DEFAULT NULL,
  `tfr_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tabelas com regras de cálculo de frete por transportadora.';

CREATE TABLE IF NOT EXISTS `log_tracking` (
  `trk_id` int(11) NOT NULL COMMENT 'Identificador único do Evento de Tracking.',
  `trk_nfs_id` int(11) DEFAULT NULL COMMENT 'NF de Saída rastreada (FK: vnd_notas_fiscais).',
  `trk_cte_id` int(11) DEFAULT NULL COMMENT 'CT-e rastreado (FK: log_cte).',
  `trk_status` enum('COLETADO','EM_TRANSITO','SAIU_ENTREGA','ENTREGUE','DEVOLUCAO','EXTRAVIADO') NOT NULL,
  `trk_localizacao` varchar(200) DEFAULT NULL,
  `trk_latitude` decimal(10,8) DEFAULT NULL,
  `trk_longitude` decimal(11,8) DEFAULT NULL,
  `trk_data_hora` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `trk_observacoes` text DEFAULT NULL,
  `trk_usr_id` int(11) DEFAULT NULL COMMENT 'Usuário que registrou o evento (Ex: motorista) (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Eventos de rastreamento e status de entrega de documentos fiscais.';

CREATE TABLE IF NOT EXISTS `log_transportadoras` (
  `tra_id` int(11) NOT NULL COMMENT 'Identificador único da Transportadora.',
  `tra_pes_id` int(11) NOT NULL COMMENT 'Pessoa relacionada (FK: adm_pessoas).',
  `tra_antt` varchar(20) DEFAULT NULL COMMENT 'Registro Nacional de Transportadores Rodoviários de Cargas.',
  `tra_tipo_veiculo` varchar(50) DEFAULT NULL,
  `tra_capacidade_peso` decimal(10,2) DEFAULT NULL,
  `tra_capacidade_volume` decimal(10,2) DEFAULT NULL,
  `tra_rastreamento` tinyint(1) DEFAULT 0,
  `tra_url_rastreamento` varchar(200) DEFAULT NULL,
  `tra_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Informações logísticas complementares à tabela de Pessoas.';

CREATE TABLE IF NOT EXISTS `pcp_apontamentos` (
  `apt_id` int(11) NOT NULL COMMENT 'Identificador único do Apontamento.',
  `apt_orp_id` int(11) NOT NULL COMMENT 'Ordem de Produção (FK: pcp_ordens_producao).',
  `apt_rop_id` int(11) NOT NULL COMMENT 'Operação do Roteiro apontada (FK: pcp_roteiro_operacoes).',
  `apt_ctr_id` int(11) NOT NULL COMMENT 'Centro de Trabalho apontado (FK: pcp_centros_trabalho).',
  `apt_usr_operador` int(11) NOT NULL COMMENT 'Usuário operador (FK: adm_usuarios).',
  `apt_data_inicio` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `apt_data_fim` timestamp NULL DEFAULT NULL,
  `apt_tempo_setup_minutos` int(11) DEFAULT 0,
  `apt_tempo_producao_minutos` int(11) DEFAULT NULL,
  `apt_quantidade_produzida` decimal(15,3) DEFAULT NULL,
  `apt_quantidade_refugo` decimal(15,3) DEFAULT 0.000,
  `apt_motivo_refugo` text DEFAULT NULL,
  `apt_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos tempos e quantidades produzidas em cada operação.';

CREATE TABLE IF NOT EXISTS `pcp_bom` (
  `bom_id` int(11) NOT NULL COMMENT 'Identificador único da BOM/Ficha Técnica.',
  `bom_itm_id` int(11) NOT NULL COMMENT 'Produto acabado/semi-acabado (FK: vnd_itens).',
  `bom_versao` varchar(10) NOT NULL DEFAULT '1.0',
  `bom_descricao` varchar(200) DEFAULT NULL,
  `bom_quantidade_base` decimal(15,3) DEFAULT 1.000 COMMENT 'Quantidade produzida pela lista de componentes.',
  `bom_data_inicio` date NOT NULL,
  `bom_data_fim` date DEFAULT NULL,
  `bom_ativo` tinyint(1) DEFAULT 1,
  `bom_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `bom_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Estrutura de Produto (Bill of Materials).';

CREATE TABLE IF NOT EXISTS `pcp_bom_itens` (
  `bmi_id` int(11) NOT NULL COMMENT 'Identificador único do Componente da BOM.',
  `bmi_bom_id` int(11) NOT NULL COMMENT 'BOM/Ficha Técnica (FK: pcp_bom).',
  `bmi_itm_id` int(11) NOT NULL COMMENT 'Componente/Matéria-prima (FK: vnd_itens).',
  `bmi_sequencia` int(11) NOT NULL,
  `bmi_quantidade` decimal(15,3) NOT NULL COMMENT 'Quantidade necessária para a quantidade base da BOM.',
  `bmi_unidade` varchar(10) NOT NULL,
  `bmi_tipo` enum('MATERIA_PRIMA','COMPONENTE','INSUMO') NOT NULL,
  `bmi_perda_percentual` decimal(5,2) DEFAULT 0.00,
  `bmi_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Componentes necessários para a fabricação do produto.';

CREATE TABLE IF NOT EXISTS `pcp_centros_trabalho` (
  `ctr_id` int(11) NOT NULL COMMENT 'Identificador único do Centro de Trabalho.',
  `ctr_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `ctr_codigo` varchar(20) NOT NULL,
  `ctr_nome` varchar(100) NOT NULL,
  `ctr_tipo` enum('MAQUINA','SETOR','LINHA_PRODUCAO') NOT NULL,
  `ctr_capacidade_hora` decimal(10,2) DEFAULT NULL,
  `ctr_custo_hora` decimal(15,2) DEFAULT NULL,
  `ctr_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo associado (FK: fin_centros_custo).',
  `ctr_ativo` tinyint(1) DEFAULT 1,
  `ctr_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de Máquinas, Setores ou Linhas de Produção.';

CREATE TABLE IF NOT EXISTS `pcp_inspecoes` (
  `ins_id` int(11) NOT NULL COMMENT 'Identificador único da Inspeção.',
  `ins_tipo` enum('RECEBIMENTO','PROCESSO','FINAL','EXPEDICAO') NOT NULL,
  `ins_origem_tipo` varchar(50) DEFAULT NULL COMMENT 'Tipo do documento de origem (NFE, OP, PDV).',
  `ins_origem_id` int(11) DEFAULT NULL COMMENT 'ID do documento de origem (nfe_id, orp_id, etc.).',
  `ins_itm_id` int(11) NOT NULL COMMENT 'Item inspecionado (FK: vnd_itens).',
  `ins_lote` varchar(50) DEFAULT NULL,
  `ins_quantidade_inspecionada` decimal(15,3) NOT NULL,
  `ins_quantidade_aprovada` decimal(15,3) DEFAULT NULL,
  `ins_quantidade_rejeitada` decimal(15,3) DEFAULT NULL,
  `ins_data_inspecao` timestamp NOT NULL DEFAULT current_timestamp(),
  `ins_usr_inspetor` int(11) NOT NULL COMMENT 'Usuário inspetor (FK: adm_usuarios).',
  `ins_resultado` enum('APROVADO','REPROVADO','APROVADO_RESTRICAO') NOT NULL,
  `ins_observacoes` text DEFAULT NULL,
  `ins_laudo` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro das inspeções de qualidade.';

CREATE TABLE IF NOT EXISTS `pcp_nao_conformidades` (
  `ncf_id` int(11) NOT NULL COMMENT 'Identificador único da Não Conformidade.',
  `ncf_numero` varchar(50) NOT NULL,
  `ncf_ins_id` int(11) DEFAULT NULL COMMENT 'Inspeção de origem (FK: pcp_inspecoes).',
  `ncf_tipo` enum('PRODUTO','PROCESSO','SISTEMA') NOT NULL,
  `ncf_gravidade` enum('BAIXA','MEDIA','ALTA','CRITICA') NOT NULL,
  `ncf_descricao` text NOT NULL,
  `ncf_causa_raiz` text DEFAULT NULL,
  `ncf_acao_imediata` text DEFAULT NULL,
  `ncf_acao_corretiva` text DEFAULT NULL,
  `ncf_acao_preventiva` text DEFAULT NULL,
  `ncf_data_abertura` timestamp NOT NULL DEFAULT current_timestamp(),
  `ncf_data_prazo` date DEFAULT NULL,
  `ncf_data_conclusao` date DEFAULT NULL,
  `ncf_status` enum('ABERTA','ANALISE','ACAO','VERIFICACAO','CONCLUIDA','CANCELADA') DEFAULT 'ABERTA',
  `ncf_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `ncf_usr_aprovador` int(11) DEFAULT NULL COMMENT 'Usuário aprovador (FK: adm_usuarios).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle das Não Conformidades (NC) e ações corretivas/preventivas.';

CREATE TABLE IF NOT EXISTS `pcp_ordens_producao` (
  `orp_id` int(11) NOT NULL COMMENT 'Identificador único da Ordem de Produção.',
  `orp_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `orp_numero` varchar(50) NOT NULL,
  `orp_pdv_id` int(11) DEFAULT NULL COMMENT 'Pedido de Venda de origem (FK: vnd_pedidos).',
  `orp_itm_id` int(11) NOT NULL COMMENT 'Produto a ser produzido (FK: vnd_itens).',
  `orp_bom_id` int(11) NOT NULL COMMENT 'BOM utilizada (FK: pcp_bom).',
  `orp_rot_id` int(11) DEFAULT NULL COMMENT 'Roteiro utilizado (FK: pcp_roteiros).',
  `orp_quantidade_planejada` decimal(15,3) NOT NULL,
  `orp_quantidade_produzida` decimal(15,3) DEFAULT 0.000,
  `orp_quantidade_refugo` decimal(15,3) DEFAULT 0.000,
  `orp_data_abertura` date NOT NULL,
  `orp_data_inicio_prevista` date NOT NULL,
  `orp_data_fim_prevista` date NOT NULL,
  `orp_data_inicio_real` date DEFAULT NULL,
  `orp_data_fim_real` date DEFAULT NULL,
  `orp_prioridade` enum('BAIXA','NORMAL','ALTA','URGENTE') DEFAULT 'NORMAL',
  `orp_status` enum('PLANEJADA','LIBERADA','EM_PRODUCAO','CONCLUIDA','CANCELADA') DEFAULT 'PLANEJADA',
  `orp_custo_previsto` decimal(15,2) DEFAULT NULL,
  `orp_custo_real` decimal(15,2) DEFAULT NULL,
  `orp_observacoes` text DEFAULT NULL,
  `orp_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável pelo PCP (FK: adm_usuarios).',
  `orp_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle das Ordens de Produção (Produto final a ser fabricado).';

CREATE TABLE IF NOT EXISTS `pcp_requisicoes_material` (
  `rqm_id` int(11) NOT NULL COMMENT 'Identificador único da Requisição de Material.',
  `rqm_orp_id` int(11) NOT NULL COMMENT 'Ordem de Produção (FK: pcp_ordens_producao).',
  `rqm_itm_id` int(11) NOT NULL COMMENT 'Item/Componente requisitado (FK: vnd_itens).',
  `rqm_quantidade_prevista` decimal(15,3) NOT NULL COMMENT 'Quantidade prevista pela BOM.',
  `rqm_quantidade_requisitada` decimal(15,3) DEFAULT 0.000 COMMENT 'Quantidade já separada/entregue.',
  `rqm_dep_id` int(11) NOT NULL COMMENT 'Depósito de origem (FK: est_depositos).',
  `rqm_status` enum('PENDENTE','SEPARADO','ENTREGUE') DEFAULT 'PENDENTE',
  `rqm_data_requisicao` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Requisições de componentes feitas pela OP ao Estoque.';

CREATE TABLE IF NOT EXISTS `pcp_roteiros` (
  `rot_id` int(11) NOT NULL COMMENT 'Identificador único do Roteiro.',
  `rot_bom_id` int(11) NOT NULL COMMENT 'BOM ao qual este roteiro se aplica (FK: pcp_bom).',
  `rot_codigo` varchar(20) NOT NULL,
  `rot_descricao` varchar(200) DEFAULT NULL,
  `rot_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sequência de operações necessárias para a produção.';

CREATE TABLE IF NOT EXISTS `pcp_roteiro_operacoes` (
  `rop_id` int(11) NOT NULL COMMENT 'Identificador único da Operação no Roteiro.',
  `rop_rot_id` int(11) NOT NULL COMMENT 'Roteiro (FK: pcp_roteiros).',
  `rop_sequencia` int(11) NOT NULL,
  `rop_ctr_id` int(11) NOT NULL COMMENT 'Centro de Trabalho onde a operação ocorre (FK: pcp_centros_trabalho).',
  `rop_descricao` varchar(200) NOT NULL,
  `rop_tempo_setup_minutos` int(11) DEFAULT 0,
  `rop_tempo_operacao_minutos` int(11) NOT NULL,
  `rop_custo_operacao` decimal(15,2) DEFAULT NULL,
  `rop_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Etapas e tempos de produção de um roteiro.';

CREATE TABLE IF NOT EXISTS `rh_afastamentos` (
  `afa_id` int(11) NOT NULL COMMENT 'Identificador único do Afastamento.',
  `afa_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `afa_tipo` enum('ATESTADO','LICENCA_MATERNIDADE','LICENCA_PATERNIDADE','ACIDENTE_TRABALHO','INSS','OUTROS') NOT NULL,
  `afa_data_inicio` date NOT NULL,
  `afa_data_fim` date DEFAULT NULL,
  `afa_data_retorno_previsto` date DEFAULT NULL,
  `afa_cid` varchar(10) DEFAULT NULL COMMENT 'Código Internacional de Doenças (se aplicável).',
  `afa_observacoes` text DEFAULT NULL,
  `afa_documento` varchar(200) DEFAULT NULL COMMENT 'Caminho ou referência ao documento (ex: atestado).',
  `afa_status` enum('ATIVO','CONCLUIDO','CANCELADO') DEFAULT 'ATIVO',
  `afa_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle dos afastamentos do trabalho (licenças, atestados).';

CREATE TABLE IF NOT EXISTS `rh_cargos` (
  `car_id` int(11) NOT NULL COMMENT 'Identificador único do Cargo.',
  `car_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `car_codigo` varchar(20) NOT NULL,
  `car_nome` varchar(100) NOT NULL,
  `car_cbo` varchar(10) DEFAULT NULL COMMENT 'Código Brasileiro de Ocupações.',
  `car_descricao` text DEFAULT NULL COMMENT 'Descrição detalhada das responsabilidades.',
  `car_salario_base` decimal(15,2) DEFAULT NULL COMMENT 'Salário base do cargo.',
  `car_nivel_hierarquico` int(11) DEFAULT NULL,
  `car_ativo` tinyint(1) DEFAULT 1,
  `car_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de cargos e suas especificações.';

CREATE TABLE IF NOT EXISTS `rh_colaboradores` (
  `col_id` int(11) NOT NULL COMMENT 'Identificador único do Colaborador.',
  `col_pes_id` int(11) NOT NULL COMMENT 'Pessoa relacionada (FK: adm_pessoas).',
  `col_emp_id` int(11) NOT NULL COMMENT 'Empresa contratante (FK: adm_empresas).',
  `col_matricula` varchar(20) NOT NULL COMMENT 'Matrícula interna do funcionário.',
  `col_car_id` int(11) NOT NULL COMMENT 'Cargo atual (FK: rh_cargos).',
  `col_dpt_id` int(11) NOT NULL COMMENT 'Departamento atual (FK: rh_departamentos).',
  `col_usr_id` int(11) DEFAULT NULL COMMENT 'Usuário do sistema (se houver) (FK: adm_usuarios).',
  `col_data_admissao` date NOT NULL,
  `col_data_demissao` date DEFAULT NULL,
  `col_tipo_contrato` enum('CLT','PJ','ESTAGIO','TEMPORARIO','AUTONOMO') NOT NULL,
  `col_salario_atual` decimal(15,2) NOT NULL,
  `col_carga_horaria` int(11) DEFAULT 44 COMMENT 'Carga horária semanal em horas.',
  `col_pis` varchar(15) DEFAULT NULL,
  `col_ctps` varchar(20) DEFAULT NULL COMMENT 'Carteira de Trabalho e Previdência Social.',
  `col_ctps_serie` varchar(10) DEFAULT NULL,
  `col_ctps_uf` char(2) DEFAULT NULL,
  `col_titulo_eleitor` varchar(20) DEFAULT NULL,
  `col_reservista` varchar(20) DEFAULT NULL,
  `col_cnh` varchar(20) DEFAULT NULL COMMENT 'Carteira Nacional de Habilitação.',
  `col_cnh_categoria` varchar(5) DEFAULT NULL,
  `col_cnh_validade` date DEFAULT NULL,
  `col_estado_civil` enum('SOLTEIRO','CASADO','DIVORCIADO','VIUVO','UNIAO_ESTAVEL') DEFAULT NULL,
  `col_grau_instrucao` enum('FUNDAMENTAL','MEDIO','SUPERIOR','POS_GRADUACAO','MESTRADO','DOUTORADO') DEFAULT NULL,
  `col_nome_mae` varchar(200) DEFAULT NULL,
  `col_nome_pai` varchar(200) DEFAULT NULL,
  `col_banco` varchar(10) DEFAULT NULL,
  `col_agencia` varchar(10) DEFAULT NULL,
  `col_conta` varchar(20) DEFAULT NULL,
  `col_tipo_conta` enum('CORRENTE','POUPANCA') DEFAULT NULL,
  `col_status` enum('ATIVO','FERIAS','AFASTADO','DEMITIDO') DEFAULT 'ATIVO',
  `col_observacoes` text DEFAULT NULL,
  `col_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Informações detalhadas dos colaboradores da empresa.';

CREATE TABLE IF NOT EXISTS `rh_departamentos` (
  `dpt_id` int(11) NOT NULL COMMENT 'Identificador único do Departamento.',
  `dpt_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `dpt_codigo` varchar(20) NOT NULL COMMENT 'Código interno do departamento.',
  `dpt_nome` varchar(100) NOT NULL COMMENT 'Nome do departamento.',
  `dpt_pai_id` int(11) DEFAULT NULL COMMENT 'Departamento hierárquico superior (FK: rh_departamentos).',
  `dpt_responsavel_usr_id` int(11) DEFAULT NULL COMMENT 'Usuário responsável (FK: adm_usuarios).',
  `dpt_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo associado (FK: fin_centros_custo).',
  `dpt_ativo` tinyint(1) DEFAULT 1,
  `dpt_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Estrutura organizacional da empresa.';

CREATE TABLE IF NOT EXISTS `rh_dependentes` (
  `dep_id` int(11) NOT NULL COMMENT 'Identificador único do Dependente.',
  `dep_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `dep_nome` varchar(200) NOT NULL,
  `dep_cpf` varchar(14) DEFAULT NULL,
  `dep_data_nascimento` date NOT NULL,
  `dep_parentesco` enum('FILHO','CONJUGE','COMPANHEIRO','PAI','MAE','OUTROS') NOT NULL,
  `dep_irrf` tinyint(1) DEFAULT 0 COMMENT 'Indica se é dependente para cálculo de IRRF.',
  `dep_salario_familia` tinyint(1) DEFAULT 0 COMMENT 'Indica se tem direito a Salário Família.',
  `dep_plano_saude` tinyint(1) DEFAULT 0,
  `dep_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro dos dependentes dos colaboradores.';

CREATE TABLE IF NOT EXISTS `rh_esocial_eventos` (
  `eso_id` int(11) NOT NULL COMMENT 'Identificador único do Evento eSocial.',
  `eso_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `eso_tipo_evento` varchar(10) NOT NULL COMMENT 'Código do evento S-XXXX (ex: S-2200).',
  `eso_identificador` varchar(50) DEFAULT NULL COMMENT 'Identificador único do evento na base local.',
  `eso_col_id` int(11) DEFAULT NULL COMMENT 'Colaborador relacionado (FK: rh_colaboradores).',
  `eso_competencia` date DEFAULT NULL COMMENT 'Competência do evento (se aplicável).',
  `eso_xml_envio` longtext NOT NULL COMMENT 'XML enviado para o eSocial.',
  `eso_xml_retorno` longtext DEFAULT NULL COMMENT 'XML de retorno do eSocial.',
  `eso_protocolo` varchar(50) DEFAULT NULL COMMENT 'Protocolo de transmissão/recibo.',
  `eso_status` enum('PENDENTE','ENVIADO','PROCESSADO','ERRO','REJEITADO') DEFAULT 'PENDENTE',
  `eso_mensagem_erro` text DEFAULT NULL,
  `eso_data_envio` timestamp NULL DEFAULT NULL,
  `eso_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Log de eventos enviados e recebidos do eSocial.';

CREATE TABLE IF NOT EXISTS `rh_eventos_folha` (
  `evf_id` int(11) NOT NULL COMMENT 'Identificador único do Evento.',
  `evf_codigo` varchar(10) NOT NULL,
  `evf_descricao` varchar(100) NOT NULL,
  `evf_tipo` enum('PROVENTO','DESCONTO') NOT NULL,
  `evf_natureza` enum('REMUNERACAO','DESCONTO_IRRF','DESCONTO_INSS','DESCONTO_FGTS','OUTROS') DEFAULT NULL COMMENT 'Natureza tributária do evento.',
  `evf_calculo_tipo` enum('VALOR','PERCENTUAL','FORMULA') NOT NULL,
  `evf_formula` text DEFAULT NULL COMMENT 'Fórmula para cálculo (se evf_calculo_tipo = FORMULA).',
  `evf_incide_irrf` tinyint(1) DEFAULT 1,
  `evf_incide_inss` tinyint(1) DEFAULT 1,
  `evf_incide_fgts` tinyint(1) DEFAULT 1,
  `evf_ativo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro dos códigos de Proventos e Descontos da folha.';

CREATE TABLE IF NOT EXISTS `rh_ferias` (
  `fer_id` int(11) NOT NULL COMMENT 'Identificador único do Período de Férias.',
  `fer_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `fer_periodo_aquisitivo_inicio` date NOT NULL,
  `fer_periodo_aquisitivo_fim` date NOT NULL,
  `fer_periodo_gozo_inicio` date NOT NULL,
  `fer_periodo_gozo_fim` date NOT NULL,
  `fer_dias_direito` int(11) NOT NULL,
  `fer_dias_gozados` int(11) NOT NULL,
  `fer_dias_abono` int(11) DEFAULT 0 COMMENT 'Dias vendidos (abono pecuniário).',
  `fer_valor_ferias` decimal(15,2) DEFAULT NULL,
  `fer_valor_abono` decimal(15,2) DEFAULT NULL,
  `fer_valor_terco` decimal(15,2) DEFAULT NULL COMMENT 'Valor do adicional de 1/3 constitucional.',
  `fer_aviso` tinyint(1) DEFAULT 0 COMMENT 'Aviso de férias foi dado?',
  `fer_status` enum('PROGRAMADA','APROVADA','EM_GOZO','CONCLUIDA','CANCELADA') DEFAULT 'PROGRAMADA',
  `fer_usr_aprovador` int(11) DEFAULT NULL COMMENT 'Usuário que aprovou as férias (FK: adm_usuarios).',
  `fer_data_aprovacao` timestamp NULL DEFAULT NULL,
  `fer_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Controle e programação dos períodos de férias.';

CREATE TABLE IF NOT EXISTS `rh_folhas_pagamento` (
  `fol_id` int(11) NOT NULL COMMENT 'Identificador único da Folha.',
  `fol_emp_id` int(11) NOT NULL COMMENT 'Empresa (FK: adm_empresas).',
  `fol_competencia` date NOT NULL COMMENT 'Mês/Ano de referência (Ex: 2025-11-01).',
  `fol_tipo` enum('MENSAL','ADIANTAMENTO','FERIAS','DECIMO_TERCEIRO','RESCISAO','PLR') NOT NULL,
  `fol_data_pagamento` date NOT NULL,
  `fol_valor_bruto` decimal(15,2) DEFAULT 0.00,
  `fol_valor_descontos` decimal(15,2) DEFAULT 0.00,
  `fol_valor_liquido` decimal(15,2) DEFAULT 0.00,
  `fol_valor_inss` decimal(15,2) DEFAULT 0.00,
  `fol_valor_irrf` decimal(15,2) DEFAULT 0.00,
  `fol_valor_fgts` decimal(15,2) DEFAULT 0.00,
  `fol_status` enum('ABERTA','CALCULADA','APROVADA','PAGA','CANCELADA') DEFAULT 'ABERTA',
  `fol_usr_responsavel` int(11) NOT NULL COMMENT 'Usuário responsável pelo fechamento (FK: adm_usuarios).',
  `fol_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cabeçalho da folha de pagamento por competência.';

CREATE TABLE IF NOT EXISTS `rh_folha_itens` (
  `fit_id` int(11) NOT NULL COMMENT 'Identificador único do Item da Folha.',
  `fit_fol_id` int(11) NOT NULL COMMENT 'Folha de Pagamento (FK: rh_folhas_pagamento).',
  `fit_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `fit_evf_id` int(11) NOT NULL COMMENT 'Evento de Folha (FK: rh_eventos_folha).',
  `fit_referencia` decimal(15,3) DEFAULT 1.000 COMMENT 'Quantidade, horas ou percentual de referência (Ex: 20 dias, 10 horas).',
  `fit_valor` decimal(15,2) NOT NULL COMMENT 'Valor monetário final do provento/desconto.',
  `fit_observacoes` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Detalhes dos proventos e descontos por colaborador e evento.';

CREATE TABLE IF NOT EXISTS `rh_historico_salarial` (
  `hsa_id` int(11) NOT NULL COMMENT 'Identificador único do Histórico.',
  `hsa_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `hsa_data_vigencia` date NOT NULL,
  `hsa_salario_anterior` decimal(15,2) DEFAULT NULL,
  `hsa_salario_novo` decimal(15,2) NOT NULL,
  `hsa_percentual_reajuste` decimal(5,2) DEFAULT NULL,
  `hsa_motivo` enum('DISSIDIO','MERITO','PROMOCAO','MUDANCA_CARGO','OUTROS') NOT NULL,
  `hsa_observacoes` text DEFAULT NULL,
  `hsa_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que registrou (FK: adm_usuarios).',
  `hsa_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro de todas as alterações salariais do colaborador.';

CREATE TABLE IF NOT EXISTS `rh_ponto` (
  `pto_id` bigint(20) NOT NULL COMMENT 'Identificador único do Registro de Ponto.',
  `pto_col_id` int(11) NOT NULL COMMENT 'Colaborador (FK: rh_colaboradores).',
  `pto_data` date NOT NULL,
  `pto_entrada_1` time DEFAULT NULL,
  `pto_saida_1` time DEFAULT NULL,
  `pto_entrada_2` time DEFAULT NULL,
  `pto_saida_2` time DEFAULT NULL,
  `pto_entrada_3` time DEFAULT NULL,
  `pto_saida_3` time DEFAULT NULL,
  `pto_horas_trabalhadas` time DEFAULT NULL COMMENT 'Tempo total trabalhado no dia.',
  `pto_horas_extras` time DEFAULT NULL,
  `pto_horas_faltas` time DEFAULT NULL,
  `pto_justificativa` text DEFAULT NULL,
  `pto_tipo_dia` enum('NORMAL','FOLGA','FERIADO','FERIAS','ATESTADO','FALTA') DEFAULT NULL,
  `pto_usr_aprovador` int(11) DEFAULT NULL COMMENT 'Usuário que aprovou o ajuste/justificativa (FK: adm_usuarios).',
  `pto_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro diário dos batimentos do ponto eletrônico.';

CREATE TABLE IF NOT EXISTS `vnd_itens` (
  `itm_id` int(11) NOT NULL COMMENT 'Identificador único do Item (Produto/Serviço).',
  `itm_emp_id` int(11) NOT NULL COMMENT 'Empresa responsável (FK: adm_empresas).',
  `itm_codigo` varchar(50) NOT NULL COMMENT 'Código/SKU do item.',
  `itm_tipo` enum('PRODUTO','SERVICO','MATERIA_PRIMA','INSUMO') NOT NULL COMMENT 'Classificação do item.',
  `itm_descricao` varchar(200) NOT NULL COMMENT 'Nome do item.',
  `itm_descricao_detalhada` text DEFAULT NULL,
  `itm_unidade` varchar(10) NOT NULL COMMENT 'Unidade de medida (Ex: UN, KG, PC).',
  `itm_ncm` varchar(10) DEFAULT NULL COMMENT 'Nomenclatura Comum do Mercosul.',
  `itm_cest` varchar(10) DEFAULT NULL COMMENT 'Código Especificador da Substituição Tributária.',
  `itm_peso_bruto` decimal(10,3) DEFAULT NULL,
  `itm_peso_liquido` decimal(10,3) DEFAULT NULL,
  `itm_altura` decimal(10,2) DEFAULT NULL,
  `itm_largura` decimal(10,2) DEFAULT NULL,
  `itm_profundidade` decimal(10,2) DEFAULT NULL,
  `itm_controla_estoque` tinyint(1) DEFAULT 1 COMMENT 'Deve controlar saldo físico?',
  `itm_controla_lote` tinyint(1) DEFAULT 0 COMMENT 'Requer rastreamento por lote?',
  `itm_controla_serie` tinyint(1) DEFAULT 0 COMMENT 'Requer rastreamento por número de série?',
  `itm_ativo` tinyint(1) DEFAULT 1,
  `itm_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro de Itens (Produtos, Serviços, Matérias-primas).';

CREATE TABLE IF NOT EXISTS `vnd_itens_precos` (
  `ipr_id` int(11) NOT NULL COMMENT 'Identificador único do Preço do Item.',
  `ipr_tpr_id` int(11) NOT NULL COMMENT 'ID da Tabela de Preço (FK: vnd_tabelas_preco).',
  `ipr_itm_id` int(11) NOT NULL COMMENT 'ID do Item (FK: vnd_itens).',
  `ipr_preco_venda` decimal(15,2) NOT NULL COMMENT 'Preço de venda na tabela.',
  `ipr_preco_custo` decimal(15,2) DEFAULT NULL COMMENT 'Custo para cálculo da margem.',
  `ipr_margem_percentual` decimal(5,2) DEFAULT NULL COMMENT 'Margem de lucro aplicada.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='precos dos itens em cada tabela de preço.';

CREATE TABLE IF NOT EXISTS `vnd_notas_fiscais` (
  `nfs_id` int(11) NOT NULL COMMENT 'Identificador único da NF de Saída.',
  `nfs_emp_id` int(11) NOT NULL COMMENT 'Empresa emitente (FK: adm_empresas).',
  `nfs_pdv_id` int(11) DEFAULT NULL COMMENT 'Pedido de Venda relacionado (FK: vnd_pedidos).',
  `nfs_numero` varchar(20) NOT NULL COMMENT 'Número da NF.',
  `nfs_serie` varchar(5) NOT NULL COMMENT 'Série da NF.',
  `nfs_modelo` varchar(5) NOT NULL,
  `nfs_chave_acesso` varchar(44) DEFAULT NULL COMMENT 'Chave de acesso da NF-e.',
  `nfs_pes_id` int(11) NOT NULL COMMENT 'Cliente (FK: adm_pessoas).',
  `nfs_data_emissao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `nfs_data_saida` date NOT NULL,
  `nfs_valor_produtos` decimal(15,2) NOT NULL,
  `nfs_valor_total` decimal(15,2) NOT NULL,
  `nfs_cfop` varchar(5) NOT NULL,
  `nfs_natureza_operacao` varchar(100) NOT NULL,
  `nfs_status` enum('DIGITACAO','TRANSMITIDA','AUTORIZADA','CANCELADA','DENEGADA') DEFAULT 'DIGITACAO',
  `nfs_xml_envio` longtext DEFAULT NULL,
  `nfs_xml_retorno` longtext DEFAULT NULL,
  `nfs_protocolo` varchar(50) DEFAULT NULL,
  `nfs_observacoes` text DEFAULT NULL,
  `nfs_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que cadastrou (FK: adm_usuarios).',
  `nfs_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro das Notas Fiscais de Saída (Vendas).';

CREATE TABLE IF NOT EXISTS `vnd_pedidos` (
  `pdv_id` int(11) NOT NULL COMMENT 'Identificador único do Pedido de Venda.',
  `pdv_emp_id` int(11) NOT NULL COMMENT 'Empresa que vende (FK: adm_empresas).',
  `pdv_numero` varchar(50) NOT NULL COMMENT 'Número do pedido.',
  `pdv_pes_id` int(11) NOT NULL COMMENT 'Cliente (FK: adm_pessoas).',
  `pdv_tpr_id` int(11) NOT NULL COMMENT 'Tabela de Preço utilizada (FK: vnd_tabelas_preco).',
  `pdv_data_pedido` date NOT NULL COMMENT 'Data de emissão do pedido.',
  `pdv_data_entrega` date DEFAULT NULL COMMENT 'Data de entrega prevista.',
  `pdv_valor_produtos` decimal(15,2) DEFAULT 0.00,
  `pdv_valor_desconto` decimal(15,2) DEFAULT 0.00,
  `pdv_valor_frete` decimal(15,2) DEFAULT 0.00,
  `pdv_valor_total` decimal(15,2) DEFAULT 0.00,
  `pdv_tipo_frete` enum('CIF','FOB') DEFAULT 'CIF',
  `pdv_forma_pagamento` varchar(50) DEFAULT NULL COMMENT 'Condição/Forma de pagamento (Ex: 30/60/90).',
  `pdv_condicao_pagamento` varchar(50) DEFAULT NULL COMMENT 'Descrição da condição.',
  `pdv_status` enum('ORCAMENTO','APROVADO','FATURADO','CANCELADO') DEFAULT 'ORCAMENTO',
  `pdv_observacoes` text DEFAULT NULL,
  `pdv_usr_vendedor` int(11) DEFAULT NULL COMMENT 'Vendedor responsável (FK: adm_usuarios).',
  `pdv_usr_cadastro` int(11) NOT NULL COMMENT 'Usuário que cadastrou (FK: adm_usuarios).',
  `pdv_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp(),
  `pdv_data_atualizacao` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Registro dos Pedidos de Venda.';

CREATE TABLE IF NOT EXISTS `vnd_pedido_itens` (
  `pdi_id` int(11) NOT NULL COMMENT 'Identificador único do Item do Pedido.',
  `pdi_pdv_id` int(11) NOT NULL COMMENT 'Pedido de Venda (FK: vnd_pedidos).',
  `pdi_itm_id` int(11) NOT NULL COMMENT 'Item vendido (FK: vnd_itens).',
  `pdi_sequencia` int(11) NOT NULL,
  `pdi_descricao` varchar(200) DEFAULT NULL COMMENT 'Descrição no momento da venda (pode ser diferente do cadastro).',
  `pdi_quantidade` decimal(15,3) NOT NULL,
  `pdi_preco_unitario` decimal(15,2) NOT NULL,
  `pdi_desconto_percentual` decimal(5,2) DEFAULT 0.00,
  `pdi_desconto_valor` decimal(15,2) DEFAULT 0.00,
  `pdi_valor_total` decimal(15,2) NOT NULL,
  `pdi_pct_id` int(11) DEFAULT NULL COMMENT 'Conta Contábil de Receita (FK: fin_plano_contas).',
  `pdi_ccu_id` int(11) DEFAULT NULL COMMENT 'Centro de Custo (FK: fin_centros_custo).'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Itens que compõem cada Pedido de Venda.';

CREATE TABLE IF NOT EXISTS `vnd_tabelas_preco` (
  `tpr_id` int(11) NOT NULL COMMENT 'Identificador único da Tabela de Preço.',
  `tpr_emp_id` int(11) NOT NULL COMMENT 'Empresa que utiliza a tabela (FK: adm_empresas).',
  `tpr_codigo` varchar(20) NOT NULL COMMENT 'Código único da tabela.',
  `tpr_nome` varchar(100) NOT NULL COMMENT 'Nome comercial da tabela.',
  `tpr_descricao` text DEFAULT NULL,
  `tpr_data_inicio` date NOT NULL COMMENT 'Data de início de vigência.',
  `tpr_data_fim` date DEFAULT NULL COMMENT 'Data de fim de vigência.',
  `tpr_padrao` tinyint(1) DEFAULT 0 COMMENT 'Indica se é a tabela padrão.',
  `tpr_ativo` tinyint(1) DEFAULT 1,
  `tpr_data_cadastro` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cadastro das tabelas de preço de venda.';


-- Estruturas stand-in das views originais GenesisGest.Net preservadas para compatibilidade.
CREATE TABLE IF NOT EXISTS `vw_balanco_patrimonial` (
`empresa_id` int(11)
,`competencia` varchar(7)
,`classe_conta` enum('ATIVO','PASSIVO','RECEITA','CUSTO','DESPESA','PATRIMONIO')
,`pct_codigo` varchar(20)
,`pct_nome` varchar(200)
,`pct_nivel` int(11)
,`saldo_conta` decimal(37,2)
);

CREATE TABLE IF NOT EXISTS `vw_certidoes_vencer` (
`cer_id` int(11)
,`empresa_id` int(11)
,`empresa_nome` varchar(200)
,`cer_tipo` enum('CND_FEDERAL','CND_ESTADUAL','CND_MUNICIPAL','CNDT','FGTS','ALVARA','LICENCA_AMBIENTAL','OUTROS')
,`cer_descricao` varchar(200)
,`cer_numero` varchar(50)
,`cer_data_emissao` date
,`cer_data_validade` date
,`cer_data_alerta` date
,`dias_para_vencer` int(7)
,`situacao` varchar(7)
,`responsavel_nome` varchar(100)
);

CREATE TABLE IF NOT EXISTS `vw_contratos_vencer` (
`ctr_id` int(11)
,`empresa_id` int(11)
,`ctr_numero` varchar(50)
,`ctr_tipo` enum('COMPRA','VENDA','SERVICO','LOCACAO','TRABALHO','PARCERIA','OUTROS')
,`parte_nome` varchar(200)
,`ctr_objeto` text
,`ctr_valor_total` decimal(15,2)
,`ctr_data_inicio` date
,`ctr_data_fim` date
,`dias_para_vencer` int(7)
,`ctr_renovacao_automatica` tinyint(1)
,`ctr_dias_aviso_vencimento` int(11)
,`situacao` varchar(7)
,`ctr_status` enum('MINUTANDO','VIGENTE','SUSPENSO','RESCINDIDO','CONCLUIDO')
,`responsavel_nome` varchar(100)
);

CREATE TABLE IF NOT EXISTS `vw_custos_producao` (
`orp_id` int(11)
,`orp_numero` varchar(50)
,`empresa_id` int(11)
,`item_id` int(11)
,`itm_descricao` varchar(200)
,`orp_quantidade_planejada` decimal(15,3)
,`orp_quantidade_produzida` decimal(15,3)
,`custo_material` decimal(37,2)
,`custo_mao_obra` decimal(51,6)
,`orp_custo_real` decimal(15,2)
,`orp_custo_previsto` decimal(15,2)
,`variacao_custo` decimal(16,2)
,`custo_unitario_real` decimal(22,6)
);

CREATE TABLE IF NOT EXISTS `vw_dre_gerencial` (
`empresa_id` int(11)
,`competencia` varchar(7)
,`classe_conta` enum('ATIVO','PASSIVO','RECEITA','CUSTO','DESPESA','PATRIMONIO')
,`pct_codigo` varchar(20)
,`pct_nome` varchar(200)
,`valor_debito` decimal(37,2)
,`valor_credito` decimal(37,2)
,`saldo_conta` decimal(37,2)
);

CREATE TABLE IF NOT EXISTS `vw_fluxo_caixa_projetado` (
`data_referencia` date
,`empresa_id` int(11)
,`entradas_previstas` decimal(60,2)
,`saidas_previstas` decimal(60,2)
,`saldo_projetado` decimal(61,2)
);

CREATE TABLE IF NOT EXISTS `vw_inadimplencia` (
`empresa_id` int(11)
,`cliente_id` int(11)
,`cliente_nome` varchar(200)
,`qtd_titulos_vencidos` bigint(21)
,`data_vencimento_mais_antigo` date
,`data_vencimento_mais_recente` date
,`dias_atraso_maximo` int(7)
,`valor_total_vencido` decimal(41,2)
);

CREATE TABLE IF NOT EXISTS `vw_ordens_producao_andamento` (
`orp_id` int(11)
,`orp_numero` varchar(50)
,`empresa_id` int(11)
,`item_id` int(11)
,`itm_codigo` varchar(50)
,`itm_descricao` varchar(200)
,`orp_quantidade_planejada` decimal(15,3)
,`orp_quantidade_produzida` decimal(15,3)
,`quantidade_restante` decimal(16,3)
,`percentual_concluido` decimal(21,2)
,`orp_data_inicio_prevista` date
,`orp_data_fim_prevista` date
,`orp_data_inicio_real` date
,`dias_atraso` int(7)
,`orp_status` enum('PLANEJADA','LIBERADA','EM_PRODUCAO','CONCLUIDA','CANCELADA')
,`orp_custo_previsto` decimal(15,2)
,`orp_custo_real` decimal(15,2)
,`responsavel_nome` varchar(100)
);

CREATE TABLE IF NOT EXISTS `vw_performance_vendedores` (
`empresa_id` int(11)
,`competencia` varchar(7)
,`vendedor_id` int(11)
,`vendedor_nome` varchar(100)
,`qtd_pedidos` bigint(21)
,`qtd_clientes_atendidos` bigint(21)
,`valor_produtos` decimal(37,2)
,`valor_descontos` decimal(37,2)
,`valor_total_vendas` decimal(37,2)
,`ticket_medio` decimal(19,6)
);

CREATE TABLE IF NOT EXISTS `vw_processos_risco` (
`prc_id` int(11)
,`prc_numero_processo` varchar(50)
,`empresa_id` int(11)
,`tipo_acao` varchar(100)
,`area` enum('CIVEL','TRABALHISTA','FISCAL','CRIMINAL','REGULATORIO','OUTROS')
,`prc_polo` enum('ATIVO','PASSIVO')
,`prc_valor_causa` decimal(15,2)
,`prc_risco` enum('REMOTO','POSSIVEL','PROVAVEL')
,`prc_percentual_risco` decimal(5,2)
,`prc_valor_provisao` decimal(15,2)
,`prc_status` enum('ANDAMENTO','SUSPENSO','ARQUIVADO','GANHO','PERDIDO','ACORDO')
,`prc_data_distribuicao` date
,`dias_tramitacao` int(7)
,`responsavel_nome` varchar(100)
);

CREATE TABLE IF NOT EXISTS `vw_ranking_produtos` (
`item_id` int(11)
,`itm_codigo` varchar(50)
,`itm_descricao` varchar(200)
,`competencia` varchar(7)
,`qtd_pedidos` bigint(21)
,`quantidade_vendida` decimal(37,3)
,`valor_total_vendas` decimal(37,2)
,`preco_medio` decimal(19,6)
);

CREATE TABLE IF NOT EXISTS `vw_saldo_contas_pagar` (
`empresa_id` int(11)
,`fornecedor_id` int(11)
,`fornecedor_nome` varchar(200)
,`qtd_titulos` bigint(21)
,`saldo_devedor` decimal(41,2)
,`saldo_vencido` decimal(41,2)
);

CREATE TABLE IF NOT EXISTS `vw_saldo_contas_receber` (
`empresa_id` int(11)
,`cliente_id` int(11)
,`cliente_nome` varchar(200)
,`qtd_titulos` bigint(21)
,`saldo_credor` decimal(41,2)
,`saldo_vencido` decimal(41,2)
);

CREATE TABLE IF NOT EXISTS `vw_saldo_estoque` (
`item_id` int(11)
,`itm_codigo` varchar(50)
,`itm_descricao` varchar(200)
,`deposito_id` int(11)
,`deposito_nome` varchar(100)
,`quantidade_total` decimal(37,3)
,`quantidade_reservada` decimal(37,3)
,`quantidade_disponivel` decimal(37,3)
,`custo_medio` decimal(19,6)
,`valor_total_estoque` decimal(37,2)
);


CREATE OR REPLACE VIEW `vw_nexum_genesis_produtos` AS
SELECT
    p.id AS produto_id,
    p.sku AS codigo_item,
    p.nome AS descricao_item,
    p.preco AS preco_venda,
    p.custo AS custo_reposicao,
    p.estoque_atual AS saldo_atual,
    p.estoque_minimo AS saldo_minimo,
    p.codigo_barras AS codigo_barras,
    p.identificacao_estoque AS identificacao_estoque,
    p.tipo_produto AS formato_aquisicao,
    p.ativo AS ativo,
    p.created_at AS criado_em,
    p.updated_at AS atualizado_em
FROM produtos p;

CREATE OR REPLACE VIEW `vw_nexum_genesis_compras` AS
SELECT
    CAST('SOLICITACAO' AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS etapa,
    s.id AS documento_id,
    CAST(s.id AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS numero,
    s.produto_id AS produto_id,
    s.produto_nome COLLATE utf8mb4_unicode_ci AS produto_nome,
    s.quantidade_solicitada AS quantidade,
    NULL AS valor_total,
    s.origem COLLATE utf8mb4_unicode_ci AS origem,
    s.finalidade COLLATE utf8mb4_unicode_ci AS finalidade,
    s.status COLLATE utf8mb4_unicode_ci AS status,
    s.created_at AS criado_em
FROM compras_solicitacoes s
UNION ALL
SELECT
    CAST('PEDIDO' AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS etapa,
    p.id AS documento_id,
    p.numero COLLATE utf8mb4_unicode_ci AS numero,
    NULL AS produto_id,
    CAST(NULL AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS produto_nome,
    NULL AS quantidade,
    p.valor_total AS valor_total,
    p.origem COLLATE utf8mb4_unicode_ci AS origem,
    p.finalidade COLLATE utf8mb4_unicode_ci AS finalidade,
    p.status COLLATE utf8mb4_unicode_ci AS status,
    p.created_at AS criado_em
FROM compras_pedidos p
UNION ALL
SELECT
    CAST('ENTRADA' AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS etapa,
    e.id AS documento_id,
    COALESCE(e.numero_documento, CAST(e.id AS CHAR CHARACTER SET utf8mb4)) COLLATE utf8mb4_unicode_ci AS numero,
    NULL AS produto_id,
    CAST(NULL AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci AS produto_nome,
    NULL AS quantidade,
    e.valor_total AS valor_total,
    e.tipo_entrada COLLATE utf8mb4_unicode_ci AS origem,
    e.status_fiscal COLLATE utf8mb4_unicode_ci AS finalidade,
    e.status_fiscal COLLATE utf8mb4_unicode_ci AS status,
    e.created_at AS criado_em
FROM compras_entradas e;

CREATE OR REPLACE VIEW `vw_nexum_genesis_financeiro_aberto` AS
SELECT
    f.id AS lancamento_id,
    f.tipo AS tipo,
    f.categoria AS categoria,
    f.descricao AS descricao,
    f.valor AS valor,
    f.status AS status,
    f.data_vencimento AS vencimento,
    f.created_at AS criado_em
FROM financeiro f
WHERE f.status IN ('Pendente', 'EmAberto', 'Aberto');
