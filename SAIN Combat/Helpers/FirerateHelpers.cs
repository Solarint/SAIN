namespace Combat.Helpers
{
    public static class Firerate
    {
        // Calculates what a bot's baseline firerate should be based on weapon stats
        public static float GetFirerate(string weapclass, string ammocaliber, out float firerate)
        {
            // Calculates what the firerate should be based on weapon class, ammo type, our final modifier
            WeaponClassFirerate(weapclass, ammocaliber, out firerate);

            return firerate;
        }
        // Selects a the time for 1 second of wait time for every x meters
        public static float WeaponClassFirerate(string weaponclass, string ammotype, out float firerate)
        {
            // Higher is faster firerate
            switch (weaponclass)
            {
                case "assaultCarbine":
                case "assaultRifle":
                case "machinegun":
                    firerate = 120f;
                    break;

                case "smg":
                    firerate = 130f;
                    break;

                case "pistol":
                    firerate = 90f;
                    break;

                case "marksmanRifle":
                    firerate = 90f;
                    break;

                case "sniperRifle":
                    firerate = 70f;
                    // VSS and VAL Exception
                    if (ammotype == "Caliber9x39")
                    {
                        firerate = 120f;
                    }
                    break;

                case "shotgun":
                case "grenadeLauncher":
                case "specialWeapon":
                    firerate = 70f;
                    break;

                default:
                    firerate = 120f;
                    break;
            }
            return firerate;
        }
        // Sets the final firerate based on our selected min and max values, and scales the modifier between the two.
        public static float FirerateScaling(float firerate, out float scaledfirerate)
        {

            return scaledfirerate = 1f;
        }
    }
}