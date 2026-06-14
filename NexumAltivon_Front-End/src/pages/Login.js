import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, Mail } from 'lucide-react';
import { clienteAPI } from '../services/api';

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [reenviarStatus, setReenviarStatus] = useState({ loading: false, message: '', tone: '' });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const result = await login(email, senha);

    if (result.success) {
      navigate(result.destination || '/dashboard');
    } else {
      setError(result.error);
    }

    setLoading(false);
  };

  const handleReenviarConfirmacao = async () => {
    if (!email.trim()) {
      setReenviarStatus({ loading: false, message: 'Informe o e-mail para reenviar o link.', tone: 'error' });
      return;
    }

    setReenviarStatus({ loading: true, message: '', tone: '' });
    try {
      await clienteAPI.reenviarConfirmacao(email.trim());
      setReenviarStatus({
        loading: false,
        message: 'Se houver um cadastro pendente, um novo link foi enviado.',
        tone: 'success',
      });
    } catch (requestError) {
      setReenviarStatus({
        loading: false,
        message: requestError.response?.data?.mensagem || requestError.response?.data?.detail || 'Não foi possível reenviar agora.',
        tone: 'error',
      });
    }
  };

  return (
    <div className="nexum-admin-login min-h-screen flex items-center justify-center py-12 px-4">
      <div className="max-w-md w-full space-y-8">
        <div className="text-center">
          <div className="w-20 h-20 bg-[#C9A227] rounded-2xl flex items-center justify-center mx-auto mb-4">
            <LogIn className="text-white" size={40} />
          </div>
          <h2 className="text-3xl font-bold text-white" data-testid="login-title">Acessar ambiente Nexum Altivon</h2>
          <p className="mt-2 text-gray-300">Clientes seguem para a área do cliente. Equipe administrativa entra no GenesisGest.Net.</p>
        </div>

        <form className="bg-white p-8 rounded-2xl shadow-2xl space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded" data-testid="login-error">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
              placeholder="seu@email.com"
              data-testid="email-input"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Senha</label>
            <input
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
              placeholder="••••••••"
              data-testid="senha-input"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 text-white py-3 rounded-lg font-semibold transition disabled:opacity-50"
            data-testid="login-submit"
          >
            {loading ? 'Entrando...' : 'Entrar'}
          </button>

          <div className="text-center">
            <Link to="/" className="text-amber-600 hover:underline">← Voltar para Home</Link>
          </div>
        </form>

        <section className="rounded-2xl border border-white/10 bg-black/30 p-6 text-white">
          <h3 className="text-lg font-bold">Não recebeu o e-mail de confirmação?</h3>
          <p className="mt-2 text-sm text-gray-300">
            Informe o mesmo e-mail do cadastro para solicitar um novo link e liberar sua área do cliente.
          </p>
          <div className="mt-4 flex flex-col gap-3 sm:flex-row">
            <label className="flex min-w-0 flex-1 items-center gap-3 rounded-lg border border-white/10 bg-white/5 px-4 py-3">
              <Mail className="text-amber-400" size={18} />
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="w-full bg-transparent text-white outline-none"
                placeholder="seu@email.com"
              />
            </label>
            <button
              type="button"
              onClick={handleReenviarConfirmacao}
              disabled={reenviarStatus.loading}
              className="rounded-lg bg-amber-500 px-5 py-3 font-semibold text-black transition hover:bg-amber-400 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {reenviarStatus.loading ? 'Reenviando...' : 'Reenviar link'}
            </button>
          </div>
          {reenviarStatus.message && (
            <div
              className={`mt-4 rounded-lg border px-4 py-3 text-sm ${
                reenviarStatus.tone === 'success'
                  ? 'border-emerald-400/30 bg-emerald-500/10 text-emerald-200'
                  : 'border-red-400/30 bg-red-500/10 text-red-200'
              }`}
            >
              {reenviarStatus.message}
            </div>
          )}
        </section>

      </div>
    </div>
  );
}
