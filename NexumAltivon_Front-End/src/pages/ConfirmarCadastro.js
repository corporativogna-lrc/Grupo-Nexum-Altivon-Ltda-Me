/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircle2, Loader2, Mail, XCircle } from 'lucide-react';
import { clienteAPI } from '../services/api';

function getApiMessage(error, fallback) {
  return (
    error?.response?.data?.mensagem ||
    error?.response?.data?.message ||
    error?.response?.data?.erro ||
    error?.response?.data?.detail ||
    fallback
  );
}

export default function ConfirmarCadastro() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const emailInicial = searchParams.get('email') || '';

  const [status, setStatus] = useState(token ? 'validando' : 'sem-token');
  const [mensagem, setMensagem] = useState('');
  const [email, setEmail] = useState(emailInicial);
  const [reenviando, setReenviando] = useState(false);
  const [reenviado, setReenviado] = useState('');

  useEffect(() => {
    if (!token) return;

    let cancelled = false;

    const confirmar = async () => {
      try {
        const response = await clienteAPI.confirmarCadastro(token);
        if (cancelled) return;
        setStatus('confirmado');
        setMensagem(response.data?.mensagem || response.data?.message || 'Cadastro confirmado com sucesso.');
      } catch (error) {
        if (cancelled) return;
        setStatus('erro');
        setMensagem(getApiMessage(error, 'Nao foi possivel confirmar o cadastro com este link.'));
      }
    };

    confirmar();

    return () => {
      cancelled = true;
    };
  }, [token]);

  const reenviarConfirmacao = async (event) => {
    event.preventDefault();
    if (!email.trim()) {
      setReenviado('Informe o e-mail do cadastro para reenviar a confirmação.');
      return;
    }

    setReenviando(true);
    setReenviado('');

    try {
      const response = await clienteAPI.reenviarConfirmacao(email.trim());
      setReenviado(response.data?.mensagem || response.data?.message || 'Link de confirmação reenviado.');
    } catch (error) {
      setReenviado(getApiMessage(error, 'Nao foi possivel reenviar a confirmação agora.'));
    } finally {
      setReenviando(false);
    }
  };

  return (
    <main className="min-h-screen bg-[#050505] px-4 py-16 text-white">
      <section className="mx-auto max-w-2xl rounded-2xl border border-[#242424] bg-[#0B0B0B] p-8 shadow-2xl">
        <div className="mb-6 flex items-center gap-3">
          {status === 'validando' && <Loader2 className="h-8 w-8 animate-spin text-[#C9A227]" />}
          {status === 'confirmado' && <CheckCircle2 className="h-8 w-8 text-emerald-400" />}
          {(status === 'erro' || status === 'sem-token') && <XCircle className="h-8 w-8 text-red-400" />}
          <div>
            <p className="text-sm font-black uppercase tracking-[0.18em] text-[#C9A227]">Área do cliente</p>
            <h1 className="mt-1 text-3xl font-black">Confirmação de cadastro</h1>
          </div>
        </div>

        {status === 'validando' && (
          <p className="text-[#D0D0D0]">Validando o link de confirmação do cadastro.</p>
        )}

        {status === 'confirmado' && (
          <div className="space-y-5">
            <p className="rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4 text-emerald-100">
              {mensagem}
            </p>
            <Link
              to="/login"
              className="inline-flex rounded-full bg-[#C9A227] px-6 py-3 font-black text-black transition hover:bg-[#FFD95A]"
            >
              Entrar na conta
            </Link>
          </div>
        )}

        {(status === 'erro' || status === 'sem-token') && (
          <div className="space-y-5">
            <p className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-red-100">
              {mensagem || 'Link ausente ou inválido. Reenvie a confirmação pelo e-mail cadastrado.'}
            </p>
            <form onSubmit={reenviarConfirmacao} className="space-y-3">
              <label className="block text-sm font-bold text-[#D0D0D0]" htmlFor="email-confirmacao">
                E-mail do cadastro
              </label>
              <div className="flex flex-col gap-3 sm:flex-row">
                <div className="relative flex-1">
                  <Mail className="pointer-events-none absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-[#777]" />
                  <input
                    id="email-confirmacao"
                    type="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    className="w-full rounded-xl border border-[#2A2A2A] bg-[#080808] py-3 pl-12 pr-4 text-white outline-none placeholder:text-[#777] focus:border-[#C9A227] focus:ring-2 focus:ring-[#C9A227]/20"
                    placeholder="cliente@email.com"
                  />
                </div>
                <button
                  type="submit"
                  disabled={reenviando}
                  className="rounded-xl bg-[#C9A227] px-5 py-3 font-black text-black transition hover:bg-[#FFD95A] disabled:opacity-60"
                >
                  {reenviando ? 'Reenviando...' : 'Reenviar'}
                </button>
              </div>
            </form>
            {reenviado && <p className="rounded-xl border border-[#2A2A2A] bg-[#101010] p-4 text-[#D0D0D0]">{reenviado}</p>}
          </div>
        )}
      </section>
    </main>
  );
}
