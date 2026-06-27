export const normalizeDocument = (value) => String(value || '').replace(/\D/g, '');

export const isValidCpf = (value) => {
  const cpf = normalizeDocument(value);
  if (cpf.length !== 11) return false;
  if (/^(\d)\1+$/.test(cpf)) return false;

  const calcDigit = (baseLength) => {
    let sum = 0;
    for (let i = 0; i < baseLength; i += 1) {
      sum += Number(cpf[i]) * (baseLength + 1 - i);
    }
    const rest = (sum * 10) % 11;
    return rest === 10 ? 0 : rest;
  };

  return calcDigit(9) === Number(cpf[9]) && calcDigit(10) === Number(cpf[10]);
};

export const isValidCnpj = (value) => {
  const cnpj = normalizeDocument(value);
  if (cnpj.length !== 14) return false;
  if (/^(\d)\1+$/.test(cnpj)) return false;

  const calcDigit = (baseLength) => {
    const weights = baseLength === 12
      ? [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
      : [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    let sum = 0;
    for (let i = 0; i < baseLength; i += 1) {
      sum += Number(cnpj[i]) * weights[i + (weights.length - baseLength)];
    }
    const mod = sum % 11;
    return mod < 2 ? 0 : 11 - mod;
  };

  return calcDigit(12) === Number(cnpj[12]) && calcDigit(13) === Number(cnpj[13]);
};

export const isValidCpfCnpj = (value) => {
  const doc = normalizeDocument(value);
  if (doc.length === 11) return isValidCpf(doc);
  if (doc.length === 14) return isValidCnpj(doc);
  return false;
};

export const normalizeCep = (value) => normalizeDocument(value).slice(0, 8);

export const fetchCepAddress = async (cep) => {
  const normalized = normalizeCep(cep);
  if (normalized.length !== 8) return null;

  const response = await fetch(`https://viacep.com.br/ws/${normalized}/json/`);
  if (!response.ok) return null;
  const data = await response.json();
  if (data?.erro) return null;

  return {
    cep: data.cep || normalized,
    logradouro: data.logradouro || '',
    bairro: data.bairro || '',
    cidade: data.localidade || '',
    estado: data.uf || '',
  };
};
