import { useState } from 'react';
import { AuthContext } from './AuthContextObject';

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const stored = sessionStorage.getItem('user');
      if (!stored) return null;
      const parsed = JSON.parse(stored);
      return parsed && typeof parsed === 'object' ? parsed : null;
    } catch {
      sessionStorage.removeItem('user');
      return null;
    }
  });

  const [token, setToken] = useState(() => {
    const stored = sessionStorage.getItem('token');
    return stored && stored !== 'null' && stored !== 'undefined' ? stored : null;
  });

  const login = (userData, authToken) => {
    if (!authToken || !userData) {
      return;
    }
    sessionStorage.setItem('token', authToken);
    sessionStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
    setToken(authToken);
  };

  const logout = () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    setUser(null);
    setToken(null);
  };

  const isAuthenticated = !!user && !!token;

  return (
    <AuthContext.Provider value={{ user, token, login, logout, isAuthenticated }}>
      {children}
    </AuthContext.Provider>
  );
}