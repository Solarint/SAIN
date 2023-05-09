//
//namespace Movement.CoverFinders
//{
//    public class Dodge : MonoBehaviour
//    {
//        // Basic Dodge. (A-D Strafe Basically). Returns false if dodge point could not be found
//        public bool ExecuteDodge(BotOwner player)
//        {
//            if (FindStartPoints.Dodge(player, out Vector3 dodgePosition))
//            {
//                player.GoToPoint(dodgePosition, false, -1, false, true, true);
//                return true;
//            }
//            return false;
//        }
//
//        // Fallback Dodge. Uses EFT Method to move away from a target. Returns false if dodge point could not be found
//        public bool ExecuteFallBack(BotOwner player, out Vector3 fallbackposition)
//        {
//            if (FindStartPoints.Fallback(player, out Vector3 FallbackPos))
//            {
//                fallbackposition = FallbackPos;
//                return true;
//            }
//            fallbackposition = Vector3.zero;
//            return false;
//        }
//
//        // Finds a suitable position to fallback to. Returns true if suitable position is found
//        private bool Fallback(BotOwner player, out Vector3 position)
//        {
//            MethodInfo targetMethod = AccessTools.Method(typeof(GClass328), "method_1");
//            // DrakiaXYZ: To get the output of the 'out' parameter, we need to pass in an actual array of objects
//            object[] method_1_parameters = new object[] { null };
//            bool method_1_result = (bool)targetMethod.Invoke(player, method_1_parameters);
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
//        // Finds a suitable dodge PartPosition when exchanging gunfire. Returns True if a point a suitable point is found
//        private bool Dodge(BotOwner player, out Vector3 position)
//        {
//            // CONFIG VALUES
//            float ShuffleRange = 2f;
//            float ArcAngle = 30.0f; // The angle of the dodge arc in degrees
//
//            Vector3 BotPosition = player.Transform.position;
//            Vector3 Target = player.Memory.GoalEnemy.CurrPosition;
//
//            // Run a loop that takes a points from an arc we generate to see if we get a navmesh hit.
//            for (int i = 0; i < 3; i++)
//            {
//                FindStartPoints.FindArcPoint(BotPosition, Target, out Vector3 ArcPoint, ShuffleRange, ArcAngle, 0.25f, ShuffleRange);
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
//        // Returns Random Point on a arc to the left or right of the player, based on the position of enemy.
//        private Vector3 FindArcPoint(Vector3 player, Vector3 target, out Vector3 dodgePosition, float arcRadius, float arcAngle, float minDist, float maxDist)
//        {
//            //Random Direction
//            bool movingRight = UnityEngine.Random.value > 0.5f;
//
//            //Generate an arc that is perpendicular to the player's target enemy
//            //Not sure I understand how this works, but it works?
//            Vector3 forward = target - player;
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
//            dodgePosition = player + direction * arcRadius;
//
//            return dodgePosition;
//        }
//    }
//}
/*
            if (!player.Memory.GoalEnemy.CanShoot && Ammo < 0.3f)
            {
                /*
                for (int i = 0; i < 3; i++)
                {
                    if (DebugDodge.Value)
                    {
                        Logger.LogDebug($"Fallback Dodge: [{player.name}]: Is trying to fall back");
                    }

                    if (Dodge.ExecuteFallBack(player, out Vector3 FallbackPosition))
                    {
                        // Bot Head PartPosition to check if visible
                        Vector3 VisibleCheckPos = FallbackPosition;
                        float HeadOffset = player.MyHead.position.y - player.Transform.position.y;
                        VisibleCheckPos.y += HeadOffset;

                        Vector3 EnemyPos = player.Memory.GoalEnemy.CurrPosition;
                        EnemyPos.y += HeadOffset;

                        if (DebugDodge.Value)
                        {
                            Logger.LogDebug($"Fallback Dodge: [{player.name}]: Found Suitable fallback position");
                        }

                        if (!Physics.Raycast(VisibleCheckPos, EnemyPos, 100f, LayerMaskClass.HighPolyWithTerrainMask))
                        {

                            if (DebugDodge.Value)
                            {
                                Logger.LogDebug($"Fallback Dodge: [{player.name}]: Suitable Fallback PartPosition is out of view of enemy. Drawing White Sphere at fallback position and red sphere at enemy position");
                                Draw.Sphere(VisibleCheckPos, 0.5f, Color.white);
                                Draw.Sphere(EnemyPos, 0.5f, Color.red);
                            }

                            player.WeaponManager.ShouldBotReload.ShouldBotReload();
                            player.GoToPoint(FallbackPosition);
                            _DogFightStateSetter.Invoke(player, new object[] { BotDogFightStatus.dogFight });

                            return;
                        }
                    }
                }
                if (DebugDodge.Value)
                {
                    Logger.LogDebug($"Fallback Dodge: [{player.name}] Failed to find a fallback position");
                }
            }

            if (player.Memory.GoalEnemy.CanShoot && player.WeaponManager.ShouldBotReload.Reloading && Ammo > 0.35f)
            {
                player.WeaponManager.ShouldBotReload.TryStopReload();
            }
            
            if (!player.Memory.GoalEnemy.CanShoot && player.Medecine.FirstAid.IsBleeding && player.Medecine.FirstAid.HaveSmth2Use)
            {
                player.Medecine.FirstAid.SetRandomPartToHeal();
                player.Medecine.FirstAid.TryApplyToCurrentPart(null, null);

                if (Dodge.ExecuteDodge(player))
                {
                    _DogFightStateSetter.Invoke(player, new object[] { BotDogFightStatus.dogFight });

                    if (DebugDodge.Value)
                    {
                        Logger.LogInfo($"[{player.name}] Is Bleeding and trying to heal");
                    }
                    return false;
                }
            }
            */

/*
                if ((!player.Memory.GoalEnemy.CanShoot || !player.Memory.GoalEnemy.SightBlocked) && Ammo < 0.2f)
                {
                    if (Dodge.ExecuteDodge(player))
                    {
                        if (!player.WeaponManager.ShouldBotReload.Reloading)
                        {
                            player.WeaponManager.ShouldBotReload.TryReload();
                        }

                        if (DebugDodge.Value)
                        {
                            Logger.LogInfo($"[{player.name}] Dodged and is reloading");
                        }

                        _DogFightStateSetter.Invoke(player, new object[] { BotDogFightStatus.dogFight });
                        return false;
                    }
        }*/
