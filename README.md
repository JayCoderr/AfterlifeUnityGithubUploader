# 🌐 WebView2 Browser Template

<p align="left">
  <img src="https://iili.io/Bm7dXVI.jpg" width="90" height="90" />
</p>

Hey — this is just a simple **WebView2 browser template** I put together.

I’m using this as a **base for my future applications**, so it might change over time… or it might stay exactly like this if it does what I need.

---

## 🚀 What this is

This project is a lightweight WinForms + WebView2 setup split into two parts:

- **Navbar (WebView21)** → custom HTML/CSS/JS UI  
- **Browser (WebView22)** → actual web content

Everything is connected through simple message passing between JavaScript and C#.

---

## ✨ What you can do with it

- Navigate websites through a custom UI
- Back / forward navigation
- Minimize, maximize, close controls
- Load local HTML files as the UI
- Build custom tools on top of a browser base

---

## 🧠 How it works

From the navbar (HTML), I send messages like:

```js
window.chrome.webview.postMessage("navigate|google.com");
