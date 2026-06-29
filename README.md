# Sum of Lies

A 2-player turn-based deduction game built with Unity where players bluff, challenge, and infer hidden information to outsmart their opponent. The game operates via Local Multiplayer, allowing two players on the same Wi-Fi network to compete using a Host/Client architecture.

---

## 🎮 Gameplay

- **Local Wi-Fi Multiplayer:** Exclusively designed for 2 players connected to the same Wi-Fi network. One player acts as the **Host** (creates the room) and the other acts as the **Client** (joins via IP/Local Discovery).
- **Matchmaking Lobby:** A dedicated lobby room where players can wait, view connection status, and signal readiness before the Host starts the match.
- **Turn-Based Bluffing:** Players take turns selecting tiles from a hidden 4×4 board. Each tile contains a secret value known only to the system.
- **Declare & Challenge:** The active player can declare any value they want for the chosen tile. The opponent must decide whether to trust the declaration or spend a limited number of **Challenges/Catches** to expose a potential lie.
- **Special Tiles:** The grid features unique modifier tiles:
  - **x2 Score:** Doubles the point outcome.
  - **All-in Random:** Introduces high-risk, unpredictable score shifts.
  - ⭐ **Catch Bonus:** Successfully catching a lie on a Special Tile rewards the opponent with **+1 bonus Catch turn**.
- **Strategic Deduction:** Row and column sums are dynamically revealed to provide clues, helping players mathematically deduce the true values over time.

---

## ✨ Features

- **Host/Client Network Architecture:** Optimized for local network play with low latency and seamless synchronization.
- **Interactive Lobby System:** Smooth pre-game state management with player readiness checks.
- **Hidden Information & Special Modifiers:** Enhances replayability and tactical depth with standard and special tiles.
- **Challenge/Catch Mechanic:** High-stakes risk management with a limited ammunition pool and bonus rewards.
- **Row & Column Clue System:** Dynamic math-based puzzle-solving integrated into PvP.
- **Time Pressure:** Turn timers to keep the gameplay fast-paced and intense.
- **Score Tracking & Evaluation:** End-game screens calculating winners based on successful bluffs and catches.
- **Polished UI & Animations:** Fluid interface transitions and satisfying feedback powered by **DOTween**.

---

## 🛠️ Tech Stack

- **Game Engine:** Unity 2022.3 LTS
- **Language:** C#
- **Networking:** Local Network Discovery / Unity Transport (Host/Client Local Wi-Fi)
- **Animation:** DOTween (DEMIGANT)
- **UI & Typography:** TextMeshPro

---

## 🏗️ Core Systems

- **Network Session Management:** Handles Local Wi-Fi discovery, Host creation, Client joining, and Lobby states.
- **Grid & Tile Management:** Generates the 4×4 board and handles the positioning/attributes of standard and special tiles.
- **Hidden Value Representation:** Securely maintains secret data on the Host side to prevent client-side cheating.
- **Bluffing & Declaration Logic:** Manages player inputs for declaring fake/true values across the network.
- **Challenge Resolution:** Determines the outcomes of a "Catch" action and distributes score penalties/rewards and bonus turns.
- **Turn & Timer System:** Synchronizes active player states and turn countdowns between both devices.
- **Tile State Visualization:** Visually updates tiles (revealed, hidden, special) with animated feedback.
- **UI State Management:** Seamlessly switches screens from Start Menu ➡️ Connection/Lobby ➡️ Gameplay ➡️ Game Over.

---

## 🚧 Status

**Current State:** Prototype / In Active Development (Local Wi-Fi Playable)

### Planned Features
- [ ] Online Multiplayer & Global Matchmaking
- [ ] Match Replay & History System
- [ ] Additional Game Modes (3v3 Grid, Time Attack)
- [ ] Smart AI Opponents (for single-player practice)
