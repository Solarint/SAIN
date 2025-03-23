# Solarint's AI Modifications

A Bepinex plugin for Single-player Escape From Tarkov that replaces the combat AI of almost all NPCs.

## Major features

- [Behavior and Decision System]: Bot decision trees replaced to immitate players with further depth and unpredictability.
- [Dynamic Cover System]: Bots find and analyze colliders in their proximity to create cover points on any map. 
	- All it needs is a suitable navmesh, a coordinate to take cover from, and colliders. 
	- No pre-placed cover positions are used. 
- [Multi-threaded Bot Vision Raycasting]: Bot vision is replaced with a multi-threaded function to improve performance, update rate, and accuracy. 
- [Advanced Bot Movement]: Bots can vault, lean around corners while peeking, strafe, stutter-sprint, and jump. 
- [Bot Personality System]: Different personalities of bots are assigned based on the quality/cost of their equipment and random chance. 
	- These personalities dramatically affect their behavior and available actions - such as intentionally ratting and hiding, or rushing as fast as they can.
- [In-Game GUI with Live Updates]: Press F6 by default to open up SAIN's in-game GUI bot editor allowing you to customize almost any aspect of bot behavior for any bot type or personality.
	- When a player hits "Save and Export" in the GUI. All changes to bots are sent and updated immedietly, unless otherwise specificied for certain options that require raid/game restart.
- [User Preset System]: All config options are tied to a "Preset" which can be shared and imported easily.
- [Squad Coordination and Barks]: Groups of bots will intentionally coordinate to flank/suppress and yell out orders that reflect what they are thinking/doing. 
- [Player Flashlight Detection]: Bots can see human player flashlights, giving them an estimated position when spotted. 
	- Bots are affected by lights being shined in their eyes from close range, affecting their accuracy and vision.
	- Bots can only see IR lights and lasers when using NVGs.
	- When shining a white-light on a bot using NVGs at close range, they are "Dazzled" more intensely.
- [Bot Suppression System]: Bots receive simulated suppression when bullets are flying near them, debuffing them in most of their stats - depending on the intensity and caliber of ammo fired.
- [Bot Equipment Effects]: 
	- Different gear and equipment will positively or negatively affect bot stats where it makes sense. 
		- Such as a heavy helmet making their hearing worse, or a magnified optic improving their accuracy at far distances, but worsening it at close range.
		- Their weapon build also has a dramatic affect on how they fire at an enemy, for example a bot using a meta-build M4A1 will be able to shoot full auto accurately from further away than a stock M4A1.
		- The effects are pulled directly from their weapon's stats so it will automatically change depending on balancing. If you remove all recoil for your guns, bots will also have no recoil.
- [Player Equipment Effects]: Some choices of gear by the player can positively or negatively affect their detection time and distance by AI dramatically.
- [Simulated Recoil for Bots]: Bots will be affected by simulated recoil depending on their weapon build, the type of weapon being fired, and their own skill level.
- [Improved Bot Vision]: More nuance and buffs/debuffs to roughly immitate how a real person spots enemies.
	- Things like movement speed and crouch height have more affect on stealth.
- [Sound Based Responses]: Most sounds that a player makes are now audible to AI, allowing them to make specific decisions if conditions are right.
	- Such as bots rushing an enemy if they hear them healing.
- [Bot Hearing Revamp]: Bots are much more nuanced in how they hear enemies, the distance a bot can hear things like footsteps is affected by their own health condition, movement, walls and obstacles between them, weather conditions.

## Requirements
- [BigBrain](https://hub.sp-tarkov.com/files/file/1219-bigbrain/) by DrakiaXYZ
- [Waypoints](https://hub.sp-tarkov.com/files/file/1119-waypoints-expanded-navmesh/) by DrakiaXYZ

## Installation
1. Confirm you have properly chosen the correct version of SAIN for the version of SPT you have installed. SAIN can only load for with the specific EFT version it was built for.
2. Install Dependencies listed above.
3. Extract zip file contents into your SPT install directory.
4. Done! If SAIN is installed and working, you can open the GUI in the main menu. (F6 by default)

## Contributing
SAIN is an open source project, and I welcome anyone who wants to redesign, tweak, or add any features in the codebase.

## How It Works
Escape from Tarkov's AI system works off a system with "Layers" of different behavior trees set at specific priorities. Using BigBrain, I have disabled and replaced bot "Combat Layers" with my own.
Each Bot receives a "SAINComponent" upon being spawned by the game, this runs in parellel to the default "BotOwner" component.
Similarly, for each of a bot's enemies, called "EnemyInfo"s - a SAIN "Enemy" instance is created that contains properties and data tracked and used by their SAINComponent. 
Bot Decisions are calculated from a class instance within their "BotComponent". The priority of each decision is based on the heirarchy within the function. Currently that priority is hardcoded, and needs a generic system to add or remove decisions, and configure priorities.

## Support SAIN's Development
SAIN is a project of countless unpaid hours over the past years. 
If you love the mod and want to financially support me, if can be done via my [Patreon Page](https://www.patreon.com/c/Solarint)
I'm currently unemployed and seeking work doing Game AI Design and scripting. 
I'm proficient in C# and Unity Scripting, self-taught with the assistance of the SPT community, and - based on SAIN's glowing reviews - I excel at designing scripts that make AI immersive and engaging to fight.

## SAIN's History
SAIN is a first for many things for me. 
I had 0 experience in coding or managing a project of this size when I started SAIN. 
It originally started as a simple patch in Tarkov's code to manually swap the firerate on Bot Weapons to Semi-auto for enemies past 50 meters.
I kept adding nuance and new features one by one until it grew into the slightly convoluted and overdesigned beast it is today!
Because making SAIN was how I learned C# and coding in general, the quality of code can vary wildly, but I've replaced *most* of the ancient code by now.
There are many classes that I created essentially just to see if I could, and I used them as learning experiences. 
SAIN is very jank, in the classic style of some of my favorite games like S.T.A.L.K.E.R. and ARMA.
It's also "Dynamic" in the sense that it is about 95% map/level agnostic, as long as the level has NavMesh to querry. 
It should work perfectly with any future level or location revamp released in the game.

## SAIN Design Principles
SAIN primary focus is to make challenging but fair bots by immitating player tactics in Tactical Shooters. 
It is designed in such a way that all bots are subject to strict limitations, and make decisions based upon what a player might reasonably be able to see/hear. 
With the exception of minor features that prioritize optimization and performance, bot behavior does not cheat nor receive any information on their enemy that couldn't reasonably be communicated between two players. 
With that limitation, bots can perform a large variety of decisions that players tend to do in player vs player fight. Many of these are inspired by my own experience playing tactical shooters for over a decade. 
SAIN is also built with user customization in mind. As an individual with limited resources, there are several features within SAIN that are set up with the intention that power users can tweak and share "Presets" - essentially mods for SAIN.
Most internal configuration for AI can be changed (writing documentation for all these is tedious work), and changes made within the GUI take effect right away to see the differences live.
