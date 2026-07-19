import './styles.scss';
import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App';

import { GoogleOAuthProvider } from '@react-oauth/google';

const baseUrl = document.getElementsByTagName('base')[0]?.getAttribute('href') || '/';
const root = createRoot(document.getElementById('root'));
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || '';

root.render(
  <GoogleOAuthProvider clientId={googleClientId}>
    <BrowserRouter basename={baseUrl}>
      <App />
    </BrowserRouter>
  </GoogleOAuthProvider>
);
