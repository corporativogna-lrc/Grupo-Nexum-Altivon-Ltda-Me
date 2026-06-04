import { useState } from 'react';
import { leadAPI } from '../services/api';
import { Mail, Phone, MessageSquare, User, Building, CheckCircle } from 'lucide-react';

export default function Contato() {
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    telefone: '',
    empresa: '',
    mensagem: ''
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      await leadAPI.create({
        ...formData,
        origem: 'Website - Formulário de Contato'
      });
      setSuccess(true);
      setFormData({ nome: '', email: '', telefone: '', empresa: '', mensagem: '' });
    } catch (err) {
      setError(err.response?.data?.detail || 'Erro ao enviar mensagem');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen bg-gray-50 py-12 flex items-center justify-center">
        <div className="max-w-md bg-white p-8 rounded-lg shadow-md text-center" data-testid="success-message">
          <CheckCircle className="text-green-500 mx-auto mb-4" size={64} />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Mensagem Enviada!</h2>
          <p className="text-gray-600 mb-6">Recebemos sua mensagem e nossa equipe entrará em contato em breve.</p>
          <button
            onClick={() => setSuccess(false)}
            className="bg-amber-500 hover:bg-amber-600 text-white px-6 py-2 rounded-lg font-semibold"
          >
            Enviar Nova Mensagem
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4" data-testid="contato-title">Entre em Contato</h1>
          <p className="text-gray-600 text-lg">Estamos prontos para atendê-lo</p>
        </div>

        <div className="grid lg:grid-cols-2 gap-12 max-w-5xl mx-auto">
          {/* Informações de Contato */}
          <div className="space-y-6">
            <div className="bg-white p-6 rounded-lg shadow-md">
              <h2 className="text-2xl font-bold text-gray-900 mb-4">Informações</h2>
              <div className="space-y-4">
                <div className="flex items-center space-x-3">
                  <div className="bg-amber-100 p-3 rounded-full">
                    <Phone className="text-amber-600" size={20} />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Rodrigo</p>
                    <a className="font-semibold hover:text-amber-600" href="tel:+5514996731879">+55 (14) 99673-1879</a>
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="bg-amber-100 p-3 rounded-full">
                    <Phone className="text-amber-600" size={20} />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Vinicius</p>
                    <a className="font-semibold hover:text-amber-600" href="tel:+5514996348409">+55 (14) 99634-8409</a>
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="bg-amber-100 p-3 rounded-full">
                    <Mail className="text-amber-600" size={20} />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Email</p>
                    <a className="font-semibold hover:text-amber-600" href="mailto:corporativo.gna@gmail.com">corporativo.gna@gmail.com</a>
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="bg-amber-100 p-3 rounded-full">
                    <MessageSquare className="text-amber-600" size={20} />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Horário de Atendimento</p>
                    <p className="font-semibold">Seg-Sex: 9h às 18h</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Formulário */}
          <div className="bg-white p-8 rounded-lg shadow-md">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Envie sua Mensagem</h2>

            {error && (
              <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="relative">
                <User className="absolute left-3 top-3 text-gray-400" size={20} />
                <input
                  type="text"
                  name="nome"
                  value={formData.nome}
                  onChange={handleChange}
                  required
                  placeholder="Seu Nome *"
                  className="w-full pl-10 pr-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="contato-nome"
                />
              </div>

              <div className="relative">
                <Mail className="absolute left-3 top-3 text-gray-400" size={20} />
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  placeholder="Email *"
                  className="w-full pl-10 pr-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="contato-email"
                />
              </div>

              <div className="relative">
                <Phone className="absolute left-3 top-3 text-gray-400" size={20} />
                <input
                  type="tel"
                  name="telefone"
                  value={formData.telefone}
                  onChange={handleChange}
                  placeholder="Telefone"
                  className="w-full pl-10 pr-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="contato-telefone"
                />
              </div>

              <div className="relative">
                <Building className="absolute left-3 top-3 text-gray-400" size={20} />
                <input
                  type="text"
                  name="empresa"
                  value={formData.empresa}
                  onChange={handleChange}
                  placeholder="Empresa"
                  className="w-full pl-10 pr-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="contato-empresa"
                />
              </div>

              <div className="relative">
                <MessageSquare className="absolute left-3 top-3 text-gray-400" size={20} />
                <textarea
                  name="mensagem"
                  value={formData.mensagem}
                  onChange={handleChange}
                  required
                  rows="4"
                  placeholder="Sua mensagem *"
                  className="w-full pl-10 pr-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                  data-testid="contato-mensagem"
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-amber-500 hover:bg-amber-600 text-white py-3 rounded-lg font-semibold transition disabled:opacity-50"
                data-testid="contato-submit"
              >
                {loading ? 'Enviando...' : 'Enviar Mensagem'}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
