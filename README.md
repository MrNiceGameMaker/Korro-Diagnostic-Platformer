# Korro Diagnostic Platformer 🚀

A dynamic 3D platformer developed in Unity, featuring procedural level generation, data-driven campaign management, and polished gameplay mechanics.

## 📺 Project Overview & Technical Deep Dive
[![Korro AI Platformer Overview]([https://img.youtube.com/vi/q6mIQ43P4q0/0.jpg)](https://www.youtube.com/watch?v=q6mIQ43P4q0](https://youtu.be/q6mIQ43P4q0?si=WVrHDL4KtL_VGFHo))

> **Note:** This presentation was generated using **NotebookLM**, providing an automated architectural analysis of the project's codebase and game mechanics.
## 🎮 About the Game
The game balances structured challenges with infinite variety:
* **Campaign Mode:** A series of handcrafted levels loaded from external JSON files. Player progression, including health and score, is persisted across levels.
* **Random Mode:** Utilizes procedural generation to create unique level layouts every time, ensuring high replayability.

## 🛠 Technical Highlights
The project is built using advanced Unity architectural patterns:
* **ScriptableObject Events:** Implements Event Channels to decouple game systems such as UI, Audio, and Logic.
* **Procedural Level Generator:** A flexible system that parses data structures to instantiate platforms, enemies, and collectibles in real-time.
* **Cinemachine Cutscenes:** Smooth camera fly-throughs at the start of each level to introduce objectives to the player.
* **Landing Indicator System:** A raycast-based mechanism providing dynamic visual feedback beneath the player to assist in precise navigation.
* **Git LFS:** Professional management of heavy assets (models and textures) using Git Large File Storage.

## 📊 Analytics & Diagnostic System
The game includes a robust built-in analytics engine designed for player behavior tracking and level balancing:
* **Event Tracking:** Automatically records key gameplay events, including level starts, completions, coin collection, and player deaths.
* **Data Persistence:** Sessions are captured and saved locally as structured JSON reports in the `Application.persistentDataPath` for easy retrieval.
* **Diagnostic Snapshots:** Provides detailed insights into player performance metrics, enabling developers to identify difficulty spikes or engagement bottlenecks.

## ⌨️ Controls
The game includes a visual tutorial in the main menu:
* **Movement:** **WASD** or Arrow keys.
* **Jump:** **Space** bar.
* **Camera:** **Mouse** movement.

## 🚀 Getting Started
1. Ensure **Git LFS** is installed on your machine before cloning the repository.
2. Open the project in Unity (6000.0.58f2 recommended).
3. Verify that both `MainMenu` and `Game` scenes are included in the **Build Settings**.
4. Launch the `MainMenu` scene to begin.
