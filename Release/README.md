# AI-Tweaks

IMPORTANT: if you are upgrading from 1.4, make sure you remove the old .dll plugin from Bepinex/plugins. I'm splitting up features to make them easier to manage and update, and everything will now go in a folder called "SAIN" in plugins.

TO INSTALL, place zip contents in main SPTarkov directory.

Version 1.5
Greatly Improved Dynamic Lean. It's now 99% consistent with only a few exceptions.
Bots now consistently reset their lean after a time when shooting a visible enemy.
Rebuilt the Semi-Auto and Swap Distance features entirely.
Swapdistance has been replaced by directly modifying the burst length that bots use depending on distance. It scales differently depending on weapon build and class, but bots will never fire full-auto past 100 meters.
Bots will still swap to semi-auto on their weapons past 100 meters, but everything else is handled differently.
Added Ergo and Ammo Recoil modifiers to fire-rate and full-auto calculations. It only plays a small part, but a part none-the-less.
Rebuild the recoil calculations into a coroutine. Bots will only need to calculate once on bot start, or when changing weapons. This was designed intentionally to work well with looting bots, so bots will recalculate when picking up a new weapon as well.
Made some small improvements to dodges (now called strafing to be more accurate)
Vastly simplified the f12 settings menu and removed a few options that nobody used most likely.
Set Scav Strafing and Scav Lean to off by default.
Fixed some niche bugs.

HUGE Thanks to DrakiaXYZ, gaylatea, kiobu-kouhai, JustNU, Props, SSH, Fontaine, and everyone else in the SPTarkov discord for their help in learning to put this together and their help with my many dumb questions. It would have taken ages to figure out on my own.

Current Features:

1. Fullauto distance scaling.
   Bots hold down the trigger longer at close range, and gradually reduce it depending on distance.

2. Distance Scaling for Semi-auto fire-rate for all bots.
   Bot fire-rate with semi-auto weapons depends on weapon build and bot type.
   Bots will gradually reduce fire-rate as distance to target increases.

3. Strafing at close range when fighting enemies.
   Bots will move around a lot more instead of standing still.

4. Dynamic Lean.
   Bots will calculate corners between them and their target, and lean peek around them based on the angle.
