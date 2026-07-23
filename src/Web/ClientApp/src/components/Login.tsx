import React, { useState } from 'react';
import { AuthClient, LoginCommand, LnurlAuthCommand, GoogleLoginCommand } from '../web-api-client';
import { useNavigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { GoogleLogin } from '@react-oauth/google';
import { Mail, Zap, Wallet } from 'lucide-react';

const authClient = new AuthClient(import.meta.env.VITE_API_BASE_URL || '');

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('email');
  const [metamaskStatus, setMetamaskStatus] = useState('');

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
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl');
        if (returnUrl) {
          window.location.href = `${returnUrl}?token=${response.accessToken}`;
        } else {
          navigate('/dashboard');
        }
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
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl');
        if (returnUrl) {
          window.location.href = `${returnUrl}?token=${response.accessToken}`;
        } else {
          navigate('/dashboard');
        }
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
    const maxAttempts = 120;
    
    const interval = setInterval(async () => {
      attempts++;
      if (attempts > maxAttempts) {
        clearInterval(interval);
        setPollingStatus('Session expired. Generate a new QR.');
        setLnurlData(null);
        return;
      }
      
      try {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL || ''}/api/Auth/lnurl-status?k1=${k1}`);
        if (response.status === 200) {
          const data = await response.json();
          if (data.isCompleted && data.result) {
            clearInterval(interval);
            setPollingStatus('Authenticated successfully!');
            localStorage.setItem('token', data.result.accessToken);
            const urlParams = new URLSearchParams(window.location.search);
            const returnUrl = urlParams.get('returnUrl');
            if (returnUrl) {
              window.location.href = `${returnUrl}?token=${data.result.accessToken}`;
            } else {
              navigate('/dashboard');
            }
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

  const handleMetamaskLogin = async () => {
    try {
      setMetamaskStatus('Connecting to Metamask...');
      setError('');
      
      // @ts-ignore
      if (!window.ethereum) {
        throw new Error('Metamask is not installed. Please install it to continue.');
      }

      // @ts-ignore
      const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
      const address = accounts[0];

      const normalizedAddress = address.toLowerCase();

      setMetamaskStatus('Requesting secure nonce...');
      const nonceRes = await fetch(`${import.meta.env.VITE_API_BASE_URL || ''}/api/Auth/metamask-nonce`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ address: normalizedAddress })
      });
      if (!nonceRes.ok) throw new Error('Failed to fetch authentication nonce.');
      
      const { nonce } = await nonceRes.json();

      setMetamaskStatus('Please sign the message in Metamask...');
      const hexNonce = '0x' + Array.from(new TextEncoder().encode(nonce))
        .map(b => b.toString(16).padStart(2, '0'))
        .join('');

      // @ts-ignore
      const signature = await window.ethereum.request({
        method: 'personal_sign',
        params: [hexNonce, address]
      });

      setMetamaskStatus('Verifying signature...');
      const loginRes = await fetch(`${import.meta.env.VITE_API_BASE_URL || ''}/api/Auth/metamask-login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ address, signature })
      });

      if (!loginRes.ok) {
        throw new Error('Login failed. Invalid signature or address.');
      }

      const data = await loginRes.json();
      setMetamaskStatus('Login successful!');
      
      if (data && data.accessToken) {
        localStorage.setItem('token', data.accessToken);
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl');
        if (returnUrl) {
          window.location.href = `${returnUrl}?token=${data.accessToken}`;
        } else {
          navigate('/dashboard');
        }
      } else {
        throw new Error('Invalid response from server.');
      }
    } catch (err: any) {
      console.error(err);
      setError(err?.message || 'Metamask authentication failed.');
      setMetamaskStatus('');
    }
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

  const renderMetamaskForm = () => (
    <div className="lnurl-container fade-in text-center">
      <p>Connect your Web3 wallet securely.</p>
      
      <button 
        type="button" 
        className="primary-btn" 
        onClick={handleMetamaskLogin}
        style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', margin: '0 auto' }}
      >
        <span aria-hidden="true" style={{ fontSize: '1.1rem' }}>🦊</span>
        Connect with Metamask
      </button>

      {metamaskStatus && (
        <p className="mt-4 text-sm" style={{ color: 'var(--pico-primary)' }}>{metamaskStatus}</p>
      )}
      
      {error && <div className="error-message mt-2">{error}</div>}
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
            style={{ display: 'flex', alignItems: 'center', gap: '6px', justifyContent: 'center' }}
          >
            <Mail size={18} /> Email
          </button>
          <button 
            className={`tab-btn ${activeTab === 'lnurl' ? 'active' : ''}`}
            onClick={() => setActiveTab('lnurl')}
            style={{ display: 'flex', alignItems: 'center', gap: '6px', justifyContent: 'center' }}
          >
            <Zap size={18} /> LNURL
          </button>
          <button 
            className={`tab-btn ${activeTab === 'metamask' ? 'active' : ''}`}
            onClick={() => { setActiveTab('metamask'); setError(''); setMetamaskStatus(''); }}
            style={{ display: 'flex', alignItems: 'center', gap: '6px', justifyContent: 'center' }}
          >
            <Wallet size={18} /> Metamask
          </button>
        </div>

        {activeTab === 'email' && renderEmailForm()}
        {activeTab === 'lnurl' && renderLnurlForm()}
        {activeTab === 'metamask' && renderMetamaskForm()}

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
