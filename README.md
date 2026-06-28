# Sum of Lies

A turn-based deduction game built with Unity where players bluff, challenge, and infer hidden information to outsmart their opponent.

### Gameplay

Players take turns selecting tiles from a hidden 4×4 board.

Each tile contains a secret value known only to the system.

The active player may declare any value they want, while the opponent must decide whether to trust the declaration or spend a limited number of challenges to expose the lie.

Row and column sums are revealed to provide clues and enable strategic deduction.

### Features

* Turn-based bluffing and deduction gameplay
* Hidden information system
* Challenge/Catch mechanic with limited ammunition
* Row and column clue system
* Time pressure using turn timers
* Score tracking and end-game evaluation
* Animated feedback using DOTween
* Start Menu, Settings, Restart and Game Over flow
* Architecture designed for future multiplayer integration

### Tech Stack

* Unity 2022.3 LTS
* C#
* DOTween
* TextMeshPro

### Core Systems

* Grid Management
* Hidden Value Representation
* Bluffing System
* Challenge Resolution
* Scoring System
* Turn Management
* Timer System
* Tile State Visualization
* UI State Management

### Status

🚧 Prototype / In Development

Planned Features:

* Online Multiplayer
* Matchmaking
* Replay System
* Additional Game Modes
* AI Opponents
