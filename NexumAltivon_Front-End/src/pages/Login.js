/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Eye, EyeOff, LogIn } from 'lucide-react';

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showSenha, setShowSenha] = useState(false);
  const [mfaRequired, setMfaRequired] = useState(false);
  const [mfaCode, setMfaCode] = useState('');
  const [keepPassword, setKeepPassword] = useState(false);

  useEffect(() => {
    if (!window.isSecureContext || !navigator.credentials?.get) return;
    navigator.credentials.get({ password: true, mediation: 'optional' })
      .then((credential) => {
        if (credential?.type === 'password') {
          setEmail(credential.id || '');
          setSenha(credential.password || '');
          setKeepPassword(true);
        }
      })
      .catch(() => {});
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const result = await login(email, senha, mfaCode.trim());

    if (result.success) {
      if (keepPassword && window.isSecureContext && window.PasswordCredential && navigator.credentials?.store) {
        try {
          await navigator.credentials.store(new window.PasswordCredential({ id: email.trim(), password: senha }));
        } catch {
          window.alert('Login efetuado. O navegador não conseguiu salvar a senha; use o gerenciador de senhas do navegador.');
        }
      } else if (keepPassword) {
        window.alert('Login efetuado. Este navegador não oferece salvamento seguro de senha pelo site; use o gerenciador de senhas do navegador.');
      }
      navigate(result.destination || '/dashboard');
    } else {
      setMfaRequired(Boolean(result.mfaRequired));
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

        <form id="nexum-login-form" className="bg-white p-8 rounded-2xl shadow-2xl space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded" data-testid="login-error">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Email</label>
            <input
              type="email"
              name="username"
              autoComplete="username"
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
            <div className="flex items-center gap-2 rounded-lg border border-gray-300 focus-within:ring-2 focus-within:ring-amber-500">
              <input
                type={showSenha ? 'text' : 'password'}
                name="password"
                autoComplete="current-password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
                className="w-full rounded-lg px-4 py-3 outline-none"
                placeholder="••••••••"
                data-testid="senha-input"
              />
              <button
                type="button"
                onClick={() => setShowSenha((current) => !current)}
                className="mr-2 inline-flex h-10 w-10 items-center justify-center rounded-full text-gray-500 transition hover:bg-gray-100 hover:text-gray-900"
                aria-label={showSenha ? 'Ocultar senha' : 'Mostrar senha'}
                title={showSenha ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showSenha ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              checked={keepPassword}
              onChange={(event) => {
                const checked = event.target.checked;
                setKeepPassword(checked);
                if (!checked) Promise.resolve(navigator.credentials?.preventSilentAccess?.()).catch(() => {});
              }}
            />
            Manter senha salva neste navegador
          </label>

          {mfaRequired && (
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Código MFA</label>
              <input
                type="text"
                inputMode="numeric"
                value={mfaCode}
                onChange={(e) => setMfaCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                required
                className="w-full rounded-lg border border-gray-300 px-4 py-3 tracking-[0.35em] outline-none focus:ring-2 focus:ring-amber-500"
                placeholder="000000"
                data-testid="mfa-code-input"
              />
            </div>
          )}

          <button
            type="submit"
            disabled={loading || (mfaRequired && mfaCode.trim().length < 6)}
            className="w-full bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 text-white py-3 rounded-lg font-semibold transition disabled:opacity-50"
            data-testid="login-submit"
          >
            {loading ? 'Entrando...' : mfaRequired ? 'Validar MFA' : 'Entrar'}
          </button>

          <div className="text-center">
            <Link to="/" className="text-amber-600 hover:underline">← Voltar para Home</Link>
          </div>
        </form>

      </div>
    </div>
  );
}
