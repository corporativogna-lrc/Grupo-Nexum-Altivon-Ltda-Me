/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

UPDATE configuracoes_sistema
SET valor = 'https://www.nexumaltivon.com.br'
WHERE chave = 'site_url'
  AND valor <> 'https://www.nexumaltivon.com.br';

UPDATE configuracoes_sistema
SET valor = 'nexumaltivon.com.br'
WHERE chave = 'home_intro_badge'
  AND valor <> 'nexumaltivon.com.br';
