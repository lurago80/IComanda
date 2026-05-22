import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import './styles/print.css';
import App from './App';
import CardapioPage from './pages/CardapioPage';
import reportWebVitals from './reportWebVitals';

// Roteamento público para /cardapio/:mesa — acessível sem login via QR Code
const path = window.location.pathname;
const cardapioMatch = path.match(/^\/cardapio(?:\/(\d+))?/);

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

if (cardapioMatch) {
  const mesa = cardapioMatch[1] ? parseInt(cardapioMatch[1], 10) : undefined;
  root.render(
    <React.StrictMode>
      <CardapioPage mesa={mesa} />
    </React.StrictMode>
  );
} else {
  root.render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
}

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
