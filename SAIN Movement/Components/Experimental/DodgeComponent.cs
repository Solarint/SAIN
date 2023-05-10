//
//namespace Movement.CoverFinders
//{
//    public class Dodge : MonoBehaviour
//    {
//        // Basic Dodge. (A-D Strafe Basically). Returns false if dodge point could not be found
//        public bool ExecuteDodge(BotOwner Player)
//        {
//            if (GeneratePaths.Dodge(Player, out Vector3 dodgePosition))
//            {
//                Player.GoToPoint(dodgePosition, false, -1, false, true, true);
//                return true;
//            }
//            return false;
//        }
//
//        // Fallback Dodge. Uses EFT Method to move away from a target. Returns false if dodge point could not be found
//        public bool ExecuteFallBack(BotOwner Player, out Vector3 fallbackposition)
//        {
//            if (GeneratePaths.Fallback(Player, out Vector3 FallbackPos))
//            {
//                fallbackposition = FallbackPos;
//                return true;
//            }
//            fallbackposition = Vector3.zero;
//            return false;
//        }
//
//        // Finds a suitable position to fallback to. Returns true if suitable position is found
//        private bool Fallback(BotOwner Player, out Vector3 position)
//        {
//            MethodInfo targetMethod = AccessTools.Method(typeof(GClass328), "method_1");
//            // DrakiaXYZ: To get the output of the 'out' parameter, we need to pass in an actual array of objects
//            object[] method_1_parameters = new object[] { null };
//            bool method_1_result = (bool)targetMethod.Invoke(Player, method_1_parameters);
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
//        private bool Dodge(BotOwner Player, out Vector3 position)
//        {
//            // CONFIG VALUES
//            float ShuffleRange = 2f;
//            float ArcAngle = 30.0f; // The angle of the dodge arc in degrees
//
//            Vector3 BotPosition = Player.Transform.position;
//            Vector3 Target = Player.Memory.GoalEnemy.CurrPosition;
//
//            // Run a loop that takes a points from an arc we generate to see if we get a navmesh hit.
//            for (int i = 0; i < 3; i++)
//            {
//                GeneratePaths.FindArcPoint(BotPosition, Target, out Vector3 ArcPoint, ShuffleRange, ArcAngle, 0.25f, ShuffleRange);
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
//        // Returns Random Point on a arc to the left or right of the Player, based on the position of enemy.
//        private Vector3 FindArcPoint(Vector3 Player, Vector3 target, out Vector3 dodgePosition, float arcRadius, float arcAngle, float minDist, float maxDist)
//        {
//            //Random Direction
//            bool movingRight = UnityEngine.Random.value > 0.5f;
//
//            //Generate an arc that is perpendicular to the Player's target enemy
//            //Not sure I understand how this works, but it works?
//            Vector3 forward = target - Player;
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
//            dodgePosition = Player + direction * arcRadius;
//
//            return dodgePosition;
//        }
//    }
//}
/*
            if (!Player.Memory.GoalEnemy.CanShoot && Ammo < 0.3f)
            {
                /*
                for (int i = 0; i < 3; i++)
                {
                    if (DebugDodge.Value)
                    {
                        Logger.LogDebug($"Fallback Dodge: [{Player.name}]: Is trying to fall back");
                    }

                    if (Dodge.ExecuteFallBack(Player, out Vector3 FallbackPosition))
                    {
                        // Bot Head PartPosition to check if visible
                        Vector3 VisibleCheckPos = FallbackPosition;
                        float HeadOffset = Player.MyHead.position.y - Player.Transform.position.y;
                        VisibleCheckPos.y += HeadOffset;

                        Vector3 EnemyPos = Player.Memory.GoalEnemy.CurrPosition;
                        EnemyPos.y += HeadOffset;

                        if (DebugDodge.Value)
                        {
                            Logger.LogDebug($"Fallback Dodge: [{Player.name}]: Found Suitable fallback position");
                        }

                        if (!Physics.Raycast(VisibleCheckPos, EnemyPos, 100f, LayerMaskClass.HighPolyWithTerrainMask))
                        {

                            if (DebugDodge.Value)
                            {
                                Logger.LogDebug($"Fallback Dodge: [{Player.name}]: Suitable Fallback PartPosition is out of view of enemy. Drawing White Sphere at fallback position and red sphere at enemy position");
                                Draw.Sphere(VisibleCheckPos, 0.5f, Color.white);
                                Draw.Sphere(EnemyPos, 0.5f, Color.red);
                            }

                            Player.WeaponManager.ShouldBotReload.ShouldBotReload();
                            Player.GoToPoint(FallbackPosition);
                            _DogFightStateSetter.Invoke(Player, new object[] { BotDogFightStatus.dogFight });

                            return;
                        }
                    }
                }
                if (DebugDodge.Value)
                {
                    Logger.LogDebug($"Fallback Dodge: [{Player.name}] Failed to find a fallback position");
                }
            }

            if (Player.Memory.GoalEnemy.CanShoot && Player.WeaponManager.ShouldBotReload.Reloading && Ammo > 0.35f)
            {
                Player.WeaponManager.ShouldBotReload.TryStopReload();
            }
            
            if (!Player.Memory.GoalEnemy.CanShoot && Player.Medecine.FirstAid.IsBleeding && Player.Medecine.FirstAid.HaveSmth2Use)
            {
                Player.Medecine.FirstAid.SetRandomPartToHeal();
                Player.Medecine.FirstAid.TryApplyToCurrentPart(null, null);

                if (Dodge.ExecuteDodge(Player))
                {
                    _DogFightStateSetter.Invoke(Player, new object[] { BotDogFightStatus.dogFight });

                    if (DebugDodge.Value)
                    {
                        Logger.LogInfo($"[{Player.name}] Is Bleeding and trying to heal");
                    }
                    return false;
                }
            }
            */

/*
                if ((!Player.Memory.GoalEnemy.CanShoot || !Player.Memory.GoalEnemy.SightBlocked) && Ammo < 0.2f)
                {
                    if (Dodge.ExecuteDodge(Player))
                    {
                        if (!Player.WeaponManager.ShouldBotReload.Reloading)
                        {
                            Player.WeaponManager.ShouldBotReload.TryReload();
                        }

                        if (DebugDodge.Value)
                        {
                            Logger.LogInfo($"[{Player.name}] Dodged and is reloading");
                        }

                        _DogFightStateSetter.Invoke(Player, new object[] { BotDogFightStatus.dogFight });
                        return false;
                    }
        }*/
