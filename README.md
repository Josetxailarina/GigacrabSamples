# Gigacrab: Getting Over The Meme 🦀💥

![Game Banner](https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/3445090/ss_e1ee599fd80ae88372178c7c84c5dcf4e78d18a4.600x338.jpg?t=1747357775)

This repository contains a small selection of scripts from my commercial project **Gigacrab: Getting Over The Meme**.

> 🛑 *Note: The full project is not included for security and commercial reasons. This repo is intended to showcase selected systems and code architecture.*

---

## 🧠 What's Inside

This repo includes several systems from the game that highlight my approach to clean and efficient gameplay programming in Unity.

### 🔄 Coin System Optimization

A centralized system that handles the spinning and floating animation of all visible coins simultaneously. This approach replaces per-object logic and significantly improves performance by:
- Avoiding individual Animators or Update calls on each coin.
- Using a single manager to control all coins on screen.
- Dynamically managing which coins are updated based on visibility.

This optimization led to a noticeable performance gain and simpler code maintenance.

---

### 🛍️ Shop System

A modular component used for each item in the in-game shop, responsible for:
- Tracking purchase state and availability.
- Handling UI animations and transitions.
- Displaying cost, description, and status (locked/unlocked).

Designed to be easily extendable for additional item types or features.

---

### 🎨 Skins System

A complete skin management system that includes:
- Unlocking skins through gameplay.
- Managing which skins are available, equipped, or locked.
- Handling visual representation and selection in the menu.

This system was built with flexibility in mind to allow future expansion and easy integration with the UI.


---

## 🕹️ Play the Full Game

**Gigacrab: Getting Over The Meme** is now available on **Steam**!  

🔗 https://store.steampowered.com/app/3445090/Gigacrab_Getting_Over_The_Meme/

---
