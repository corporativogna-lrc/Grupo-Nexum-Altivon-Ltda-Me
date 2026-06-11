import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn } from 'lucide-react';

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

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

        <div className="rounded-2xl border border-[#C9A227]/20 bg-black/40 px-5 py-4 text-sm text-zinc-200">
          <p className="font-black uppercase tracking-[0.18em] text-[#E8D5A3]">Atendimento inteligente</p>
          <p className="mt-2 leading-6">Yara recepciona os clientes no portal de vendas e Sophia apoia a operação do GenesisGest.Net no fluxo administrativo.</p>
        </div>
      </div>
    </div>
  );
}
