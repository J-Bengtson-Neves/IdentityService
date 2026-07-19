import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Dashboard() {
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    if (!savedToken) {
      navigate('/login');
    } else {
      setToken(savedToken);
    }
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigate('/login');
  };

  if (!token) return <div className="loading">Loading...</div>;

  return (
    <div className="dashboard-page fade-in">
      <div className="glass-panel">
        <header className="dashboard-header">
          <h2>Welcome to IdentityService</h2>
          <button onClick={handleLogout} className="secondary-btn logout-btn">Logout</button>
        </header>
        
        <div className="dashboard-content">
          <div className="status-card success">
            <div className="status-icon">✓</div>
            <div className="status-text">
              <h3>Authentication Successful</h3>
              <p>You have successfully logged in and received a JWT token.</p>
            </div>
          </div>
          
          <div className="token-display">
            <h4>Your Access Token:</h4>
            <div className="token-box">
              <code>{token}</code>
            </div>
            <p className="token-hint">This token is sent automatically with API requests.</p>
          </div>
        </div>
      </div>
      
      <div className="animated-background">
        <div className="blob blob-1"></div>
        <div className="blob blob-2"></div>
      </div>
    </div>
  );
}
