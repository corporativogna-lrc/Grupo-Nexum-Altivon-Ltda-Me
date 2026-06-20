-- Repara valores vazios gravados antes dos mapeamentos ENUM do Entity Framework.

START TRANSACTION;

UPDATE fiscal
SET status_nfe = 'Pendente'
WHERE status_nfe = '' OR status_nfe IS NULL;

UPDATE pedido_itens
SET tipo_fulfillment = 'Proprio'
WHERE tipo_fulfillment = '' OR tipo_fulfillment IS NULL;

UPDATE pedido_itens
SET status_item = 'Pendente'
WHERE status_item = '' OR status_item IS NULL;

UPDATE enderecos
SET tipo = 'Entrega'
WHERE tipo = '' OR tipo IS NULL;

COMMIT;
