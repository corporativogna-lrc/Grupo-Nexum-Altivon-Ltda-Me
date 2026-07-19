/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

const hexColorPattern = /^#[0-9A-F]{6}$/i;

const validColor = (value, fallback) => {
  const normalized = String(value || '').trim().toUpperCase();
  return hexColorPattern.test(normalized) ? normalized : fallback;
};

export const buildPublicThemeStyle = (config = {}) => ({
  '--site-primary': validColor(config.primaryColor, '#C9A227'),
  '--site-secondary': validColor(config.secondaryColor, '#0A0A0A'),
  '--site-background': validColor(config.backgroundColor, '#F5F7FB'),
  '--site-surface': validColor(config.surfaceColor, '#FFFFFF'),
  '--site-text': validColor(config.textColor, '#0F172A'),
  '--site-muted': validColor(config.mutedTextColor, '#64748B'),
});

export const buildProfileThemeStyle = (profile = {}) => ({
  '--profile-primary': validColor(profile.corPrimaria, 'var(--site-primary)'),
  '--profile-secondary': validColor(profile.corSecundaria, 'var(--site-secondary)'),
  '--profile-background': validColor(profile.corFundo, 'var(--site-background)'),
  '--profile-text': validColor(profile.corTexto, 'var(--site-text)'),
});
