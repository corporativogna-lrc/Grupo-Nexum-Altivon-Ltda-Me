-- Nexum Altivon - schema para checkout e area do cliente

START TRANSACTION;

ALTER TABLE clientes
    ADD COLUMN IF NOT EXISTS email_verificado_em DATETIME NULL,
    ADD COLUMN IF NOT EXISTS token_confirmacao_email VARCHAR(255) NULL,
    ADD COLUMN IF NOT EXISTS token_confirmacao_expira_em DATETIME NULL;

CREATE INDEX IF NOT EXISTS ix_clientes_token_confirmacao_email
    ON clientes (token_confirmacao_email);

COMMIT;
