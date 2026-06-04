import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { STORAGE_KEYS, ADMIN_ROLES } from '../constants';
import api, { API_BASE_URL } from '../services/api';

const AuthContext = createContext();

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

      if (!accessToken) {
        return {
          success: false,
          error: 'Login recebido, mas a API não retornou token de acesso. Verifique a publicação da API.',
        };
      }

      localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userData));

      axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      setUser(userData);

      return { success: true };
    } catch (error) {
      const isNetworkError = !error.response;

      return {
        success: false,
        error: isNetworkError
          ? `API pública indisponível no momento (${API_BASE_URL}). Verifique Cloudflare/DNS e tente novamente.`
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
    isAdmin: user ? ADMIN_ROLES.includes(user.role || user.perfil) : false
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
