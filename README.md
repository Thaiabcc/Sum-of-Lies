# Sum of Lies

A 2-player turn-based deduction game built with Unity where players bluff, challenge, and infer hidden information. The game operates via Local Wi-Fi using a Netcode-driven Host/Client architecture.

---

## 🎮 Gameplay & Features

- **Local Wi-Fi Multiplayer:** 2-player Host/Client setup over the same Wi-Fi with an interactive matchmaking lobby.
- **Turn-Based Bluffing:** Active player selects a tile from a hidden 4×4 board and declares a fake value, while the opponent decides to trust or challenge.
- **Challenge & Ammo System:** Strategic bluff-catching mechanics governed by a limited and recoverable Catch Ammo pool.
- **Special Modifier Tiles:** Features hidden `[x2]` and `[ALL-IN]` tiles that amplify rewards for successful catches or inflict heavy penalties for false accusations.
- **Dynamic Turn Timer:** A state-aware countdown that automatically shrinks as the board empties to increase endgame tension.
- **Strategic Clues:** Real-time row and column sums are displayed to enable mathematical deduction.
- **Juicy UX:** Fluid interface transitions and satisfying tactile feedback (punch, shake, and rotation) powered by **DOTween**.

---

## 🛠️ Tech Stack & Systems

- **Core Tech:** Unity 2022.3 LTS, C#, **Unity Netcode for GameObjects**, DOTween, TextMeshPro.
- **Core Systems:** Network & Lobby Management, Grid Evaluation Logic, Challenge Resolution System, Dynamic Timer, and Real-time UI Synchronization.
