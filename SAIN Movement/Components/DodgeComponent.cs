//
//namespace SAIN.Movement.Components
//{
//    public class Dodge : MonoBehaviour
//    {
//        // Basic Dodge. (A-D Strafe Basically). Returns false if dodge point could not be found
//        public bool ExecuteDodge(BotOwner bot)
//        {
//            if (FindPoint.Dodge(bot, out Vector3 dodgePosition))
//            {
//                bot.GoToPoint(dodgePosition, false, -1, false, true, true);
//                return true;
//            }
//            return false;
//        }
//
//        // Fallback Dodge. Uses EFT Method to move away from a target. Returns false if dodge point could not be found
//        public bool ExecuteFallBack(BotOwner bot, out Vector3 fallbackposition)
//        {
//            if (FindPoint.Fallback(bot, out Vector3 FallbackPos))
//            {
//                fallbackposition = FallbackPos;
//                return true;
//            }
//            fallbackposition = Vector3.zero;
//            return false;
//        }
//
//        // Finds a suitable position to fallback to. Returns true if suitable position is found
//        private bool Fallback(BotOwner bot, out Vector3 position)
//        {
//            MethodInfo targetMethod = AccessTools.Method(typeof(GClass328), "method_1");
//            // DrakiaXYZ: To get the output of the 'out' parameter, we need to pass in an actual array of objects
//            object[] method_1_parameters = new object[] { null };
//            bool method_1_result = (bool)targetMethod.Invoke(bot, method_1_parameters);
//
//            // Returns Fallback position if its valid
//            if (method_1_result)
//            {
//                position = (Vector3)method_1_parameters[0];
//                return true;
//            }
//            position = Vector3.zero;
//            return false;
//        }
//
//        // Finds a suitable dodge Position when exchanging gunfire. Returns True if a point a suitable point is found
//        private bool Dodge(BotOwner bot, out Vector3 position)
//        {
//            // CONFIG VALUES
//            float ShuffleRange = 2f;
//            float ArcAngle = 30.0f; // The angle of the dodge arc in degrees
//
//            Vector3 BotPosition = bot.Transform.position;
//            Vector3 TargetPosition = bot.Memory.GoalEnemy.CurrPosition;
//
//            // Run a loop that takes a points from an arc we generate to see if we get a navmesh hit.
//            for (int i = 0; i < 3; i++)
//            {
//                FindPoint.FindArcPoint(BotPosition, TargetPosition, out Vector3 ArcPoint, ShuffleRange, ArcAngle, 0.25f, ShuffleRange);
//                if (NavMesh.SamplePosition(ArcPoint, out NavMeshHit navmeshhit, 1f, -1))
//                {
//                    position = navmeshhit.position;
//                    return true;
//                }
//            }
//            position = Vector3.zero;
//            return false;
//        }
//
//        // Returns Random Point on a arc to the left or right of the bot, based on the position of enemy.
//        private Vector3 FindArcPoint(Vector3 bot, Vector3 target, out Vector3 dodgePosition, float arcRadius, float arcAngle, float minDist, float maxDist)
//        {
//            //Random Direction
//            bool movingRight = UnityEngine.Random.value > 0.5f;
//
//            //Generate an arc that is perpendicular to the bot's target enemy
//            //Not sure I understand how this works, but it works?
//            Vector3 forward = target - bot;
//            forward.y = 0.0f;
//            forward.Normalize();
//
//            Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
//
//            float arcStart = movingRight ? -arcAngle / 2.0f : 180.0f - arcAngle / 2.0f;
//            float arcEnd = movingRight ? arcAngle / 2.0f : 180.0f + arcAngle / 2.0f;
//
//            arcRadius = Mathf.Clamp(arcRadius, minDist, maxDist);
//            float randomAngle = Random.Range(arcStart, arcEnd);
//            Vector3 direction = Quaternion.AngleAxis(randomAngle, Vector3.up) * right;
//            dodgePosition = bot + direction * arcRadius;
//
//            return dodgePosition;
//        }
//    }
//}
/*
            if (!bot.Memory.GoalEnemy.CanShoot && Ammo < 0.3f)
            {
                /*
                for (int i = 0; i < 3; i++)
                {
                    if (DebugDodge.Value)
                    {
                        Logger.LogDebug($"Fallback Dodge: [{bot.name}]: Is trying to fall back");
                    }

                    if (Dodge.ExecuteFallBack(bot, out Vector3 FallbackPosition))
                    {
                        // Bot Head Position to check if visible
                        Vector3 VisibleCheckPos = FallbackPosition;
                        float HeadOffset = bot.MyHead.position.y - bot.Transform.position.y;
                        VisibleCheckPos.y += HeadOffset;

                        Vector3 EnemyPos = bot.Memory.GoalEnemy.CurrPosition;
                        EnemyPos.y += HeadOffset;

                        if (DebugDodge.Value)
                        {
                            Logger.LogDebug($"Fallback Dodge: [{bot.name}]: Found Suitable fallback position");
                        }

                        if (!Physics.Raycast(VisibleCheckPos, EnemyPos, 100f, LayerMaskClass.HighPolyWithTerrainMask))
                        {

                            if (DebugDodge.Value)
                            {
                                Logger.LogDebug($"Fallback Dodge: [{bot.name}]: Suitable Fallback Position is out of view of enemy. Drawing White Sphere at fallback position and red sphere at enemy position");
                                Draw.Sphere(VisibleCheckPos, 0.5f, Color.white);
                                Draw.Sphere(EnemyPos, 0.5f, Color.red);
                            }

                            bot.WeaponManager.Reload.Reload();
                            bot.GoToPoint(FallbackPosition);
                            _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.dogFight });

                            return;
                        }
                    }
                }
                if (DebugDodge.Value)
                {
                    Logger.LogDebug($"Fallback Dodge: [{bot.name}] Failed to find a fallback position");
                }
            }

            if (bot.Memory.GoalEnemy.CanShoot && bot.WeaponManager.Reload.Reloading && Ammo > 0.35f)
            {
                bot.WeaponManager.Reload.TryStopReload();
            }
            
            if (!bot.Memory.GoalEnemy.CanShoot && bot.Medecine.FirstAid.IsBleeding && bot.Medecine.FirstAid.HaveSmth2Use)
            {
                bot.Medecine.FirstAid.SetRandomPartToHeal();
                bot.Medecine.FirstAid.TryApplyToCurrentPart(null, null);

                if (Dodge.ExecuteDodge(bot))
                {
                    _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.dogFight });

                    if (DebugDodge.Value)
                    {
                        Logger.LogInfo($"[{bot.name}] Is Bleeding and trying to heal");
                    }
                    return false;
                }
            }
            */

/*
                if ((!bot.Memory.GoalEnemy.CanShoot || !bot.Memory.GoalEnemy.IsVisible) && Ammo < 0.2f)
                {
                    if (Dodge.ExecuteDodge(bot))
                    {
                        if (!bot.WeaponManager.Reload.Reloading)
                        {
                            bot.WeaponManager.Reload.TryReload();
                        }

                        if (DebugDodge.Value)
                        {
                            Logger.LogInfo($"[{bot.name}] Dodged and is reloading");
                        }

                        _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.dogFight });
                        return false;
                    }
        }*/
