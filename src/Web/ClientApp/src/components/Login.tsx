import React, { useState } from 'react';
import { AuthClient, LoginCommand, LnurlAuthCommand, GoogleLoginCommand } from '../web-api-client';
import { useNavigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { GoogleLogin } from '@react-oauth/google';

const authClient = new AuthClient(import.meta.env.VITE_API_BASE_URL || '');

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('email');

  const handleEmailLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    
    try {
      const command = new LoginCommand();
      command.email = email;
      command.password = password;
      const response = await authClient.login(command);
      
      if (response && response.accessToken) {
        localStorage.setItem('token', response.accessToken);
        navigate('/dashboard');
      } else {
        setError('Login failed. Please check your credentials.');
      }
    } catch (err: any) {
      setError(err?.message || 'Invalid email or password.');
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSuccess = async (credentialResponse: any) => {
    try {
      setLoading(true);
      setError('');
      const command = new GoogleLoginCommand();
      command.idToken = credentialResponse.credential;
      const response = await authClient.google(command);
      
      if (response && response.accessToken) {
        localStorage.setItem('token', response.accessToken);
        navigate('/dashboard');
      } else {
        setError('Google Login failed.');
      }
    } catch (err: any) {
      setError(err?.message || 'Google Login failed.');
    } finally {
      setLoading(false);
    }
  };

  const renderEmailForm = () => (
    <form onSubmit={handleEmailLogin} className="login-form fade-in">
      <div className="form-group">
        <label htmlFor="email">Email</label>
        <input 
          type="email" 
          id="email" 
          value={email} 
          onChange={(e) => setEmail(e.target.value)} 
          required 
          placeholder="your@email.com"
        />
      </div>
      <div className="form-group">
        <label htmlFor="password">Password</label>
        <input 
          type="password" 
          id="password" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          required 
          placeholder="••••••••"
        />
      </div>
      {error && <div className="error-message">{error}</div>}
      <button type="submit" disabled={loading} className="primary-btn">
        {loading ? 'Authenticating...' : 'Sign In'}
      </button>
    </form>
  );

  const [lnurlData, setLnurlData] = useState<{ k1: string; lnurlEncoded: string } | null>(null);
  const [pollingStatus, setPollingStatus] = useState<string>('');

  const fetchLnurlChallenge = async () => {
    try {
      setPollingStatus('Generating QR code...');
      const response = await authClient.lnurlChallenge();
      if (response && response.k1 && response.lnurlEncoded) {
        setLnurlData({ k1: response.k1, lnurlEncoded: response.lnurlEncoded });
        setPollingStatus('Waiting for wallet scan...');
        pollLnurlStatus(response.k1);
      }
    } catch (err) {
      console.error(err);
      setPollingStatus('Failed to generate challenge. Try again.');
    }
  };

  const pollLnurlStatus = async (k1: string) => {
    let attempts = 0;
    const maxAttempts = 120; // 2 minutes (1 per second)
    const baseUrl = import.meta.env.VITE_API_BASE_URL || '';
    
    const interval = setInterval(async () => {
      attempts++;
      if (attempts > maxAttempts) {
        clearInterval(interval);
        setPollingStatus('Session expired. Generate a new QR.');
        setLnurlData(null);
        return;
      }
      
      try {
        const response = await fetch(`${baseUrl}/api/Auth/lnurl-status?k1=${k1}`);
        if (response.status === 200) {
          const data = await response.json();
          if (data.isCompleted && data.result) {
            clearInterval(interval);
            setPollingStatus('Authenticated successfully!');
            localStorage.setItem('token', data.result.accessToken);
            navigate('/dashboard');
          }
        } else if (response.status === 401) {
            clearInterval(interval);
            setPollingStatus('Session expired.');
            setLnurlData(null);
        }
      } catch (err) {
        console.error('Polling error', err);
      }
    }, 1000);
  };

  const renderLnurlForm = () => (
    <div className="lnurl-container fade-in">
      {!lnurlData ? (
        <div className="text-center">
            <p>Generate a QR code to login with a Lightning wallet securely.</p>
            <button type="button" className="secondary-btn" onClick={fetchLnurlChallenge}>
                Generate QR Code
            </button>
            <p className="mt-2 text-sm">{pollingStatus}</p>
        </div>
      ) : (
        <div className="text-center">
            <div className="qr-container" style={{background: 'white', padding: '16px', borderRadius: '8px', display: 'inline-block', marginBottom: '1rem'}}>
                <QRCodeSVG value={lnurlData.lnurlEncoded} size={200} />
            </div>
            <p>{pollingStatus}</p>
            <button type="button" className="secondary-btn outline" onClick={() => { navigator.clipboard.writeText(lnurlData.lnurlEncoded); alert('Copied to clipboard!'); }}>
                Copy LNURL
            </button>
            <button type="button" className="secondary-btn mt-2" onClick={fetchLnurlChallenge}>
                Generate New QR
            </button>
        </div>
      )}
    </div>
  );

  return (
    <div className="login-page">
      <div className="login-glass-card">
        <div className="brand-header">
          <h2>IdentityService</h2>
          <p>Sign in to continue</p>
        </div>

        <div className="auth-tabs">
          <button 
            className={`tab-btn ${activeTab === 'email' ? 'active' : ''}`}
            onClick={() => setActiveTab('email')}
          >
            Email
          </button>
          <button 
            className={`tab-btn ${activeTab === 'lnurl' ? 'active' : ''}`}
            onClick={() => setActiveTab('lnurl')}
          >
            LNURL
          </button>
        </div>

        {activeTab === 'email' ? renderEmailForm() : renderLnurlForm()}

        <div className="divider">
          <span>or continue with</span>
        </div>

        <div className="social-logins">
          <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={() => {
              setError('Google Login Failed');
            }}
            useOneTap
          />
        </div>
      </div>
      <div className="animated-background">
        <div className="blob blob-1"></div>
        <div className="blob blob-2"></div>
      </div>
    </div>
  );
}
