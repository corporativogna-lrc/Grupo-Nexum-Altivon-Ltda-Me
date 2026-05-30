import { useEffect, useState, useCallback } from 'react';
import { lojaAPI } from '../services/api';
import { MapPin, Phone, Mail, Building2 } from 'lucide-react';

export default function Lojas() {
  const [lojas, setLojas] = useState([]);
  const [loading, setLoading] = useState(true);

  const loadLojas = useCallback(async () => {
    try {
      const response = await lojaAPI.getAll();
      setLojas(response.data);
    } catch (error) {
      if (process.env.NODE_ENV === 'development') {
        console.error('Erro:', error);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadLojas();
  }, [loadLojas]);

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4" data-testid="lojas-title">Nossas Lojas</h1>
          <p className="text-gray-600 text-lg">Encontre a loja Nexum Altivon mais próxima de você</p>
        </div>

        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600 mx-auto"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6" data-testid="lojas-grid">
            {lojas.map((loja) => (
              <div key={loja.id} className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition" data-testid={`loja-${loja.id}`}>
                <div className="bg-gradient-to-r from-slate-900 to-slate-800 p-6 text-white">
                  <Building2 className="text-amber-400 mb-2" size={32} />
                  <h2 className="text-xl font-bold">{loja.nome}</h2>
                </div>
                <div className="p-6 space-y-3">
                  <p className="text-gray-600">{loja.descricao}</p>

                  <div className="space-y-2 pt-3 border-t">
                    <div className="flex items-start space-x-2 text-gray-700">
                      <MapPin className="text-amber-600 mt-1 flex-shrink-0" size={18} />
                      <div>
                        <p>{loja.endereco}</p>
                        <p className="text-sm text-gray-500">{loja.cidade}/{loja.estado}</p>
                      </div>
                    </div>

                    {loja.telefone && (
                      <div className="flex items-center space-x-2 text-gray-700">
                        <Phone className="text-amber-600" size={18} />
                        <span>{loja.telefone}</span>
                      </div>
                    )}

                    {loja.email && (
                      <div className="flex items-center space-x-2 text-gray-700">
                        <Mail className="text-amber-600" size={18} />
                        <span className="text-sm">{loja.email}</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
