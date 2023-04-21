# AI-Tweaks

TO INSTALL, place zip contents in main SPTarkov directory.

Version 1.1
Finally found the source of the bots not being able to see the player sometimes.
Reenabled GainSight feature. Bots will see targets at a slower rate when the weather is bad again.
Adjusted scaling and numbers for how weather influences vision. Added a minimum cap to make sure bots don't have incredibly poor vision when they shouldn't
New:
Added option to disable visible distance max caps for players who have global fog disabled and want the AI to be able to see from much further away. This changes the scaling of weather and adjusts minimum values to reflect it.

Completely redid the way time of day influences bot vision.

- I noticed that AI vision only changes very suddenly far after the sun had actually set. It should now gradually reduce at realistic times like how it does with the player.
- Still tweaking numbers, and its not 100% perfect yet. But its far better than default.

Version 1.0

Rain, clouds, and fog will now affect bot max visible distances and how fast they see enemies.

It ramps up as the weather gets worse. For Example: a light drizzle and partly cloudy weather will have almost no affect. But Heavy Fog / Overcast Skys / and heavy rain will drastically affect visible distances and vision speed.

Note: This needs more testing with mods that modify fog like Amand's graphics. Let me know if you notice anything weird.

Still need to make sure the resulting distances are at a good value in a ton of different situations, so let me know if the ai is completely hopeless in bad weather.
