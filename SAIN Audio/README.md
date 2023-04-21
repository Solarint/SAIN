1.2
Completely overhauled the way bots hear sounds.

Sounds are now occluded based on environment.
Occlusion only takes place if the source of the sound is not directly visible.
For sounds that created by the player, a raycast is used to check all objects inbetween the player and bot hearing them, and the audible range is reduced based on the number of hits with dimishing results.
Suppressed gunshots are more affected by occlusion than un-suppressed shots.

Bots hear sounds from other bots based on if they are indoors or not. So bots will hear a sound that is from outside at a 30% reduced rate if they are inside a building.
If the player/bot is outside, and the bot is also outside, this effect is reduced. Same as if the player/bot and the enemy bot are in the same building.

AI who are heavily injured or sprinting now hear worse than if standing still.

Many small tweaks to how bots react to sounds.

1.1
Fixed rare error that can occur if a bot shoots right as they are killed.

1.0

Bots now hear gunshots at different ranges depending on the ammo caliber.
By default they hear every gunshot at the same range, and every suppressed shot at the same range - no matter the caliber or if it was subsonic.

Subsonic rounds now have a purpose!
They will be far less audible at range to bots.
This is done on the fly by checking the velocity statistic of the ammo caliber being fired, and applying the subsonic modifier if it is less than 343.2 meters per second.

Audible Ranges also account for rain intensity. So a heavy rain will have a reduction in how far they can hear gunshots.
A subsonic round in heavy rain will be extremely difficult for bots to hear.

Have fun going full splintercell on bots at night with a suppressed .45 pistol.
