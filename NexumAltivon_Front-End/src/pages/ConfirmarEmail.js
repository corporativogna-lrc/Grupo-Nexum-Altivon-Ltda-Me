import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { BadgeCheck, CircleAlert, LoaderCircle, Mail } from 'lucide-react';
import { clienteAPI } from '../services/api';

export default function ConfirmarEmail() {
  const [params] = useSearchParams();
  const [status, setStatus] = useState({ loading: true, success: false, message: '' });
  const [email, setEmail] = useState('');
  const [reenviarStatus, setReenviarStatus] = useState({ loading: false, message: '', tone: '' });

  useEffect(() => {
    const token = params.get('token');
    if (!token) {
      setStatus({ loading: false, success: false, message: 'Link de confirmação inválido.' });
      return;
    }

    clienteAPI.confirmarEmail(token)
      .then(() => setStatus({ loading: false, success: true, message: 'E-mail confirmado. Sua área do cliente está liberada.' }))
      .catch((error) => setStatus({
        loading: false,
        success: false,
        message: error.response?.data?.mensagem || error.response?.data?.detail || 'O link é inválido ou expirou.',
      }));
  }, [params]);

  const handleReenviar = async (event) => {
    event.preventDefault();
    if (!email.trim()) {
      setReenviarStatus({ loading: false, tone: 'error', message: 'Informe o e-mail para reenviar o link.' });
      return;
    }

    setReenviarStatus({ loading: true, tone: '', message: '' });
    try {
      await clienteAPI.reenviarConfirmacao(email.trim());
      setReenviarStatus({
        loading: false,
        tone: 'success',
        message: 'Se existir um cadastro pendente, um novo link foi enviado para o e-mail informado.',
      });
    } catch (error) {
      setReenviarStatus({
        loading: false,
        tone: 'error',
        message: error.response?.data?.mensagem || error.response?.data?.detail || 'Não foi possível reenviar o link agora.',
      });
    }
  };

  const Icon = status.loading ? LoaderCircle : status.success ? BadgeCheck : CircleAlert;

  return (
    <main className="flex min-h-[70vh] items-center justify-center bg-[#050505] px-4 py-16 text-white">
      <section className="w-full max-w-lg rounded-lg border border-white/10 bg-[#111] p-8 text-center">
        <Icon className={`mx-auto ${status.loading ? 'animate-spin text-[#C9A227]' : status.success ? 'text-emerald-400' : 'text-rose-400'}`} size={48} />
        <h1 className="mt-5 text-3xl font-black">{status.loading ? 'Confirmando e-mail' : status.success ? 'Cadastro confirmado' : 'Não foi possível confirmar'}</h1>
        <p className="mt-3 text-zinc-300">{status.loading ? 'Aguarde um instante.' : status.message}</p>
        {!status.loading && !status.success && (
          <form onSubmit={handleReenviar} className="mt-8 rounded-2xl border border-white/10 bg-black/20 p-5 text-left">
            <label className="block text-sm font-semibold text-zinc-200">
              E-mail para reenviar o link
              <div className="mt-3 flex items-center gap-3 rounded-xl border border-white/10 bg-black/30 px-4 py-3 focus-within:border-[#C9A227]">
                <Mail size={18} className="text-[#C9A227]" />
                <input
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  className="w-full bg-transparent text-white outline-none"
                  placeholder="seu@email.com"
                  autoComplete="email"
                />
              </div>
            </label>
            {reenviarStatus.message && (
              <div
                className={`mt-4 rounded-xl border px-4 py-3 text-sm ${
                  reenviarStatus.tone === 'success'
                    ? 'border-emerald-400/30 bg-emerald-500/10 text-emerald-200'
                    : 'border-red-400/30 bg-red-500/10 text-red-200'
                }`}
              >
                {reenviarStatus.message}
              </div>
            )}
            <button
              type="submit"
              disabled={reenviarStatus.loading}
              className="mt-4 inline-flex w-full items-center justify-center rounded-full bg-[#C9A227] px-6 py-3 font-black text-black disabled:cursor-not-allowed disabled:opacity-70"
            >
              {reenviarStatus.loading ? 'Reenviando...' : 'Reenviar confirmação'}
            </button>
          </form>
        )}
        {!status.loading && (
          <Link to={status.success ? '/login' : '/'} className="mt-7 inline-flex rounded-full bg-[#C9A227] px-6 py-3 font-black text-black">
            {status.success ? 'Entrar na área do cliente' : 'Voltar ao site'}
          </Link>
        )}
      </section>
    </main>
  );
}
