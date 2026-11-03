import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { STORAGE_KEYS } from '../constants';

// GenesisGest.Net • 1.1.5.7243 • mantém seleções distintas do mesmo SKU no carrinho.

const CartContext = createContext();

const getCartItemKey = (item) => item.cartKey || String(item.id);

export function CartProvider({ children }) {
  const [cart, setCart] = useState([]);

  // Load cart from localStorage on mount
  const loadCart = useCallback(() => {
    const savedCart = localStorage.getItem(STORAGE_KEYS.CART);
    if (savedCart) {
      setCart(JSON.parse(savedCart));
    }
  }, []);

  useEffect(() => {
    loadCart();
  }, [loadCart]);

  // Persist cart to localStorage when it changes
  useEffect(() => {
    localStorage.setItem(STORAGE_KEYS.CART, JSON.stringify(cart));
  }, [cart]);

  const addToCart = (product, quantity = 1, selectedOptions = {}) => {
    const variant = Object.fromEntries(Object.entries(selectedOptions).sort(([left], [right]) => left.localeCompare(right)));
    const variantKey = JSON.stringify(variant);
    const cartItem = { ...product, quantity, variacao: variant, variantKey, cartKey: `${product.id}::${variantKey}` };
    setCart(prevCart => {
      const existingItem = prevCart.find(item => getCartItemKey(item) === cartItem.cartKey);

      if (existingItem) {
        return prevCart.map(item =>
          getCartItemKey(item) === cartItem.cartKey
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }

      return [...prevCart, cartItem];
    });
  };

  const removeFromCart = (cartItemKey) => {
    setCart(prevCart => prevCart.filter(item => getCartItemKey(item) !== String(cartItemKey)));
  };

  const updateQuantity = (cartItemKey, quantity) => {
    if (quantity <= 0) {
      removeFromCart(cartItemKey);
      return;
    }

    setCart(prevCart =>
      prevCart.map(item =>
        getCartItemKey(item) === String(cartItemKey) ? { ...item, quantity } : item
      )
    );
  };

  const clearCart = () => {
    setCart([]);
  };

  const getTotal = () => {
    return cart.reduce((total, item) => {
      const price = item.preco_promocional || item.preco;
      return total + (price * item.quantity);
    }, 0);
  };

  const getItemCount = () => {
    return cart.reduce((count, item) => count + item.quantity, 0);
  };

  const value = {
    cart,
    addToCart,
    removeFromCart,
    updateQuantity,
    clearCart,
    getTotal,
    getItemCount
  };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  return useContext(CartContext);
}
