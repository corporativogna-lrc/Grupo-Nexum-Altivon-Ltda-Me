import React from "react";
import ReactDOM from "react-dom/client";
import "@/index.css";
import App from "@/App";

const redirectPath = window.location.search;

if (redirectPath.startsWith('?/')) {
  const restoredPath = redirectPath
    .slice(1)
    .replace(/~and~/g, '&');
  window.history.replaceState(null, '', restoredPath + window.location.hash);
}

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
