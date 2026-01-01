# Project Context: Korro AI Assignment - "The Neuro-Grid" (Data-Driven)

## 1. Executive Summary
**Goal:** Create a scalable, data-driven 3D platformer where levels are generated procedurally via JSON configuration. The game measures player learning curves ("Neuroplasticity") across multiple levels.
**Core Concept:** A linear course on the Z-axis, with variable Y-heights and specific platform types defined by data.
**Target Audience:** Korro AI (Demonstrating System Architecture & Analytics).
**Engine:** Unity 6000.0.58f.
**Priority:** **Architecture (Scalability)** > **Analytics** > **Visuals**.

## 2. Technical Architecture
**Pattern:** Data-Oriented Design.
**Core Systems:**
1.  **LevelManager:** Reads JSON -> Instantiates Level.
2.  **LevelGenerator:** Algorithm to interpret "Platform Types" and "Height Rules".
3.  **AnalyticsManager:** Tracks session data, calculates averages, saves progress.
4.  **PlayerController:** Tight, arcade-like movement (CharacterController).

## 3. Data Structure (JSON Schema)
The level is defined by an array of "Rows". Each row represents a Z-step.
* **X-Width:** Fixed at 4 units (or 1 wide platform of width 4).
* **Platform Types:**
    * `0`: **Standard** (Solid ground).
    * `1`: **Moving** (Pings-Pongs Left/Right).
    * `2`: **Ghost** (Blinks in/out).
    * `3`: **Gap** (Empty space - requires jump).

**Example `level_1.json`:**
```json
{
  "levelId": 1,
  "startHeight": 0,
  "rows": [
    { "type": 0, "heightOffset": 0 }, // Start
    { "type": 0, "heightOffset": 0 },
    { "type": 3, "heightOffset": 0 }, // Gap (Jump)
    { "type": 0, "heightOffset": 1 }, // Land higher
    { "type": 1, "heightOffset": 1 }  // Moving Platform
  ]
}