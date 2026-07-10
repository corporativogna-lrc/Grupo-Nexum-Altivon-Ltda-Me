/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

export const onlyDigits = (value) => String(value || '').replace(/\D/g, '');

export const normalizeCep = (value) => onlyDigits(value).slice(0, 8);

const isRepeatedDigits = (digits) => /^(\d)\1+$/.test(digits);

const calculateCpfDigit = (digits, factorStart) => {
  let total = 0;
  for (let index = 0; index < factorStart - 1; index += 1) {
    total += Number(digits[index]) * (factorStart - index);
  }

  const remainder = (total * 10) % 11;
  return remainder === 10 ? 0 : remainder;
};

const isValidCpf = (documentDigits) => {
  if (documentDigits.length !== 11 || isRepeatedDigits(documentDigits)) return false;

  const firstDigit = calculateCpfDigit(documentDigits, 10);
  const secondDigit = calculateCpfDigit(documentDigits, 11);

  return firstDigit === Number(documentDigits[9]) && secondDigit === Number(documentDigits[10]);
};

const calculateCnpjDigit = (digits, factors) => {
  const total = factors.reduce((sum, factor, index) => sum + Number(digits[index]) * factor, 0);
  const remainder = total % 11;
  return remainder < 2 ? 0 : 11 - remainder;
};

const isValidCnpj = (documentDigits) => {
  if (documentDigits.length !== 14 || isRepeatedDigits(documentDigits)) return false;

  const firstFactors = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const secondFactors = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const firstDigit = calculateCnpjDigit(documentDigits, firstFactors);
  const secondDigit = calculateCnpjDigit(documentDigits, secondFactors);

  return firstDigit === Number(documentDigits[12]) && secondDigit === Number(documentDigits[13]);
};

export const isValidCpfCnpj = (value) => {
  const documentDigits = onlyDigits(value);
  if (documentDigits.length === 11) return isValidCpf(documentDigits);
  if (documentDigits.length === 14) return isValidCnpj(documentDigits);
  return false;
};

export const fetchCepAddress = async (cep) => {
  const normalizedCep = normalizeCep(cep);
  if (normalizedCep.length !== 8) return null;

  try {
    const response = await fetch(`https://viacep.com.br/ws/${normalizedCep}/json/`, {
      method: 'GET',
      headers: { Accept: 'application/json' },
    });

    if (!response.ok) return null;

    const data = await response.json();
    if (!data || data.erro) return null;

    return {
      cep: normalizeCep(data.cep) || normalizedCep,
      logradouro: data.logradouro || '',
      complemento: data.complemento || '',
      bairro: data.bairro || '',
      cidade: data.localidade || '',
      estado: data.uf || '',
    };
  } catch {
    return null;
  }
};
