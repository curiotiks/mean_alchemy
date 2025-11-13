# Mean Alchemy

[🇺🇸 English Version](./README.md) | [🇨🇳 简体中文](./README.zh.md)

## 🧪 Overview

**Mean Alchemy** is a 2D educational RPG that blends data science and alchemy.  
Players step into the role of an apprentice alchemist seeking entry into an elite guild.  
Under the guidance of a reclusive mentor, players learn to harness *statistical reasoning* through experimentation, battle, and exploration.

Your task is to transmute elemental stones, construct your **Familiar**, and take on **bounties** through turn-based challenges that test your understanding of statistical concepts like **mean** and **standard deviation**.

The world of Mean Alchemy isn’t just fantasy—it’s a metaphor for data-driven thinking.  
Each decision, combination, and battle reflects how understanding variation and central tendency can empower informed action in uncertain environments.

## 🕹️ How to Play

### 🎯 Goal
- Collect and complete **bounties** to rise through the ranks of the Alchemist Guild.
- Transmute stones at the **Alchemy Table** to build your Familiar.
- Engage in **combat encounters** using your Familiar’s abilities, which depend on your statistical understanding of data.

### 🧭 Locations

#### **1. The Lab**
Your main workspace and hub.  
Here, you’ll:
- Mix and analyze **transmutation stones**.
- Power up your **Familiar** based on statistical combinations.
- Access the **Warp Gate** (once you’ve completed a valid transmutation and selected a bounty) to enter combat.

#### **2. The Bounty Board**
The place to accept new quests.  
- Review available **bounties** (quests).
- Select one to activate the **Warp Gate** for combat.
- You can return here anytime to replace or abandon a bounty.

#### **3. Combat Arena**
A turn-based battle space where your Familiar faces off against a bounty opponent.  
- Choose to **Attack** or **Defend**.
- Victory and defeat depend on your Familiar’s strength—derived from your data manipulation choices at the table.

### 🧱 Core Actions and Controls

| Action | Key / Control | Description |
|:--|:--|:--|
| **Move Player** | Arrow Keys / WASD | Move around the environment |
| **Interact / Select** | Mouse Left Click | Choose buttons, stones, or quests |
| **Transmute Stones** | Click numbered buttons | Combine stones to build a Familiar |
| **Abandon Bounty** | Click the red ❌ on active bounty card | Clears your current quest |
| **Warp to Combat** | Move into Warp Gate (once active) | Enter the battle scene |
| **Combat Actions** | On-screen buttons | Attack your opponent |
| **Return to Lab** | Click **Flee** in combat or win a battle | Go back to the Lab to prepare again |

### 💡 Learning Objectives

1. Students will be able to (SWBA) match a small dataset to its corresponding histogram
2. SWBA to construct histograms that accurately represent a given data set.
3. SWBA to explain how the mean and standard deviation relate to the shape and spread (s/s) of a given histogram.
4. SWBA to interpret the mean and standard deviation of a dataset to draw conclusions about the s/s of distribution.
5. SWBA to explain how outliers affect the mean and standard deviation of a data set.
6. SWBA to manipulate the values of a data set to produce a histogram with a given s/s.


## 🧩 Development Notes

- **Built with:** Unity (WebGL)
- **Back-end:** Supabase (for data logging and session management)
- **Logging:** All player actions, transmutations, and combat reports are stored as structured JSONB records for research on learning analytics.
- **Research Context:** Developed as part of a doctoral dissertation exploring *game-based measures of self-efficacy and statistical reasoning.*


## 🌐 Play Online

👉 [**Play Mean Alchemy**](https://mean-alchemy.netlify.app/)  

Current test code: test001 or use the one provided to you. 

* Enter code
* Click submit
* Click change scene

