# Sum of Lies

A 2-player turn-based deduction game built with Unity where players bluff, challenge, and infer hidden information to outsmart their opponent. The game operates via Local Multiplayer, allowing two players on the same Wi-Fi network to compete using a Netcode-driven Host/Client architecture.

---

## 🎮 Gameplay

- **Local Wi-Fi Multiplayer:** Exclusively designed for 2 players connected to the same Wi-Fi network. One player starts as the **Host** (Server & Player 1) and the other joins as the **Client** (Player 2) with dynamic connection callbacks.
- **Matchmaking Lobby:** A dedicated pre-game panel where the Host waits for Client connection, displaying live status updates before authorized to start the match.
- **Turn-Based Bluffing:** Players select unowned tiles from a hidden 4×4 board (16 tiles total). The active player submits a declaration value while the tile's true random value (1-9) remains hidden.
- **Declare & Challenge:** The opponent can either let the fake value pass or spend 1 **Catch Ammo** to challenge the claim. 
  - *Ammo Mechanic:* Initiating a challenge always refunds +1 ammo to the challenger, making the limited pool (starts at 3) highly tactical.
- **Special Tiles:** The board generates unique hidden modifier tiles with high-stakes risk/reward systems:
  - **`[x2]` Modifier:** Multiplies the scoring outcome by 2.
  - **`[ALL-IN]` Modifier:** Multiplies the scoring outcome by 3.
  - ⭐ **High-Stakes Resolution:** Catching a lie rewards the challenger with the tile's real value multiplied. Challenging a true statement triggers a massive score penalty: `(Declared Value + 10) * Multiplier`.
- **Dynamic Time Pressure:** Integrated a state-aware turn timer that dynamically shrinks the countdown time as the board fills up (`12s` ➡️ `8s` ➡️ `5s`) to ramp up tension.
- **Strategic Deduction:** Row and column sums are computed and displayed instantly to help players mathematically deduce hidden values over time.

---

## ✨ Features

- **Unity Netcode Architecture:** Optimized with `ServerRpc` and `ClientRpc` network logic to ensure secure state synchronization and prevent client-side cheating.
- **Interactive Lobby Flow:** Complete UI transitions from Start Menu ➡️ Host/Client Connection ➡️ Lobby Status ➡️ Live Gameplay.
- **Challenge & Ammo Economy:** High-risk bluff-catching loops backed by an ammo counter system and custom penalties.
- **Polished UX & Animations:** Fluid tactile feedback powered by **DOTween** featuring unique punch, shake, and rotation animations depending on Catch success, failure, or pass states.

---

## 🛠️ Tech Stack

- **Game Engine:** Unity 2022.3 LTS
- **Language:** C#
- **Networking:** Unity Netcode for GameObjects (`Unity.Netcode`)
- **Animation:** DOTween (DEMIGANT)
- **UI & Typography:** TextMeshPro

---

## 🏗️ Core Systems

- **Network Session & Lobby Management:** Handles Host hosting, Client joining, and cross-network initial board data deployment.
- **Grid & Evaluation Logic:** Generates a 4×4 grid of standard/special tiles, updates global row/column arithmetic clues, and monitors the 16-tile depletion state.
- **Challenge Resolution System:** Evaluates real vs. fake values upon user inputs or timer timeouts, distributing tailored points or penalties across the network.
- **Dynamic Timer System:** Regulates turn-based phases (Selection Phase & Challenge Phase) with aggressive speed scaling near end-game.
- **UI & Tile State Visualization:** Synchronizes board imagery, localized typography tags (`[x2]`, `[ALL-IN]`), color states, and player turn overlays in real-time.

---

## 🚧 Status

**Current State:** Prototype / Playable Local Wi-Fi Built via Netcode

### Planned Features
- [ ] LAN Network Discovery / Auto-room Finding
- [ ] Global Online Matchmaking
- [ ] Match History & Replay System
- [ ] Single-Player Practice Mode with Smart AI Opponents
