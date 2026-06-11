import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { STORAGE_KEYS, ADMIN_ROLES } from '../constants';
import api, { API_BASE_URL, getRuntimeApiBaseUrl } from '../services/api';

const AuthContext = createContext();
const CLIENT_ROLE = 'Cliente';

const normalizeRole = (userData) => userData?.role || userData?.perfil || '';
const isAdminRole = (userData) => ADMIN_ROLES.includes(normalizeRole(userData));
const getPostLoginDestination = (userData) => (isAdminRole(userData) ? '/dashboard' : '/area-cliente');

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  // Restore session on mount
  const restoreSession = useCallback(() => {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    const userData = localStorage.getItem(STORAGE_KEYS.USER);

    if (token && userData) {
      setUser(JSON.parse(userData));
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }
    setLoading(false);
  }, []);

  useEffect(() => {
    restoreSession();
  }, [restoreSession]);

  const login = async (email, senha) => {
    try {
      const response = await api.post('/auth/login', { email, senha });
      const payload = response.data?.dados || response.data?.Dados || response.data?.data || response.data;
      const accessToken = payload.access_token || payload.token || payload.Token;
      const refreshToken = payload.refresh_token || payload.refreshToken || payload.RefreshToken || '';
      const userData = payload.user || payload.usuario || payload.Usuario || {
        nome: payload.nome || 'Administrador Nexum',
        email,
        role: payload.perfil || payload.role || 'Gerente',
      };
      const normalizedUser = {
        ...userData,
        role: userData.role || userData.perfil || payload.perfil || payload.role || CLIENT_ROLE,
      };

      if (!accessToken) {
        return {
          success: false,
          error: 'Login recebido, mas a API não retornou token de acesso. Verifique a publicação da API.',
        };
      }

      localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(normalizedUser));

      axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      setUser(normalizedUser);

      return {
        success: true,
        destination: getPostLoginDestination(normalizedUser),
        user: normalizedUser,
      };
    } catch (error) {
      const isNetworkError = !error.response;
      const runtimeApiBaseUrl = isNetworkError
        ? await getRuntimeApiBaseUrl().catch(() => API_BASE_URL)
        : API_BASE_URL;

      return {
        success: false,
        error: isNetworkError
          ? `API indisponível no momento (${runtimeApiBaseUrl}). Verifique a ponte pública da API e tente novamente.`
          : error.response?.data?.detail || error.response?.data?.mensagem || 'Erro ao fazer login'
      };
    }
  };

  const logout = () => {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER);
    delete axios.defaults.headers.common['Authorization'];
    setUser(null);
  };

  const value = {
    user,
    loading,
    login,
    logout,
    isAuthenticated: !!user,
    isAdmin: user ? isAdminRole(user) : false,
    isCliente: user ? normalizeRole(user) === CLIENT_ROLE : false,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
