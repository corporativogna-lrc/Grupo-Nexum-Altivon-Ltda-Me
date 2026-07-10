/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

export const YARA_WHATSAPP = '5514996731879';
export const SOPHIA_WHATSAPP = '5514996348409';

export const supportMessages = Object.freeze({
  yaraSales:
    'Olá, Yara. Vim pelo site do Grupo Nexum Altivon e quero atendimento comercial.',
  yaraCustomer:
    'Olá, Yara. Preciso de atendimento sobre meu cadastro, pedido ou entrega no Grupo Nexum Altivon.',
  sophiaOperations:
    'Olá, Sophia. Preciso de apoio operacional do GenesisGest.Net.',
});

const normalizePhoneNumber = (phoneNumber) => {
  const digits = String(phoneNumber || '').replace(/\D/g, '');
  if (digits.length < 10) {
    throw new Error('Número de WhatsApp inválido para montagem do link de atendimento.');
  }

  return digits.startsWith('55') ? digits : `55${digits}`;
};

export const buildWhatsAppLink = (phoneNumber, message) => {
  const normalizedPhone = normalizePhoneNumber(phoneNumber || YARA_WHATSAPP);
  const normalizedMessage = String(message || '').trim();
  const encodedMessage = normalizedMessage ? `?text=${encodeURIComponent(normalizedMessage)}` : '';

  return `https://wa.me/${normalizedPhone}${encodedMessage}`;
};
