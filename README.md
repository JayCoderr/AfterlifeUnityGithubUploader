# 🌐 Unity Github Tool

<p align="left">
  <img src="https://iili.io/Bm7dXVI.jpg" width="90" height="90" />
</p>

Hey — this is a lightweight tool I use to upload files faster to GitHub and manage simple workflows.

I’m using this as a **base for future applications**, so it may evolve over time or stay as-is if it continues doing what I need.

---

## 🚀 What this is

This project is a lightweight WinForms + WebView2 setup split into two parts:

- **Navbar (WebView21)** → custom HTML/CSS/JS UI  
- **Browser (WebView22)** → main web content / pages / tools

Everything is connected through simple message passing between JavaScript and C#.

---

## ✨ What you can do with it

- Navigate websites through a custom UI
- Back / forward navigation
- Minimize, maximize, close controls
- Load local HTML files as the UI (served locally)
- Build custom tools on top of a browser base
- Extend into utility apps (upload tools, dashboards, etc.)

---

## 🧠 How it works

From the navbar (HTML), messages are sent to C# like this:

```js
window.chrome.webview.postMessage("navigate|google.com");
```

C# listens and handles commands such as:
- navigation
- file uploads
- UI events
- app control actions

This keeps the UI and logic loosely coupled and easy to extend.

---

## 🔧 Why this exists

This project exists to reduce friction when working with GitHub and other repetitive tasks, while also acting as a base for future tools and utilities.

---

## 📌 Notes

This is not meant to be a finished product — it’s a reusable foundation that can grow into different applications over time.

---
