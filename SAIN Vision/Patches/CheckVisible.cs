using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SAIN_Helpers.DebugDrawer;

namespace Vision.Helpers
{
    public class CheckIfPartIsVisible
    {
        protected static ManualLogSource Logger { get; private set; }

        public bool DoCheck(BotOwner bot, KeyValuePair<BodyPartClass, GClass478> visiblePart, float seenCoef, bool onSense, bool onSenceGreen, float addVisibility)
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("AI Vision: ");

            this.botOwner_0 = bot;
            this.part = visiblePart;
            return GetResult(onSense, onSenceGreen, seenCoef, addVisibility);
        }

        private KeyValuePair<BodyPartClass, GClass478> part;
        private BotOwner botOwner_0;
        private GClass553 gclass;
        private GClass553 gclass2;
        private RaycastHit raycastHit;
        private RaycastHit raycastHit2;

        private bool GetResult(bool onSense, bool onSenceGreen, float seenCoef, float addVisibility)
        {
            Vector3 partPosition = part.Key.Position;
            Vector3 headPosition = botOwner_0.LookSensor._headPoint;
            Vector3 directionToEnemyPart = partPosition - headPosition;
            float enemyMagnitude = directionToEnemyPart.magnitude;

            if (CheckIfVisible(partPosition, addVisibility) == false)
                return false;

            if (NewRaycastCheck())
            {
                return true;
            }
            else return false;

            DoRaycasts(out bool hitObject1, out bool noHitOnBothRays, enemyMagnitude, partPosition, headPosition);

            seenCoef = GetObstructionDistance(out bool aiLookGrassorNoGrass, hitObject1, noHitOnBothRays, enemyMagnitude, seenCoef, partPosition, headPosition);

            return CanBotSeeEnemy(hitObject1, noHitOnBothRays, aiLookGrassorNoGrass, seenCoef, onSenceGreen, onSense);
        }

        private static readonly List<string> foliageType = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch",
        "timber", "spruce", "bush", "wood"};

        private bool NewRaycastCheck()
        {
            Vector3 partPosition = part.Key.Position;
            Vector3 headPosition = botOwner_0.LookSensor._headPoint;
            Vector3 directionToEnemyPart = partPosition - headPosition;

            Ray ray = new Ray(headPosition, directionToEnemyPart);

            if (Physics.Raycast(ray, out RaycastHit hit, directionToEnemyPart.magnitude, LayerMaskClass.HighPolyWithTerrainMaskAI))
            {
                if (Physics.Raycast(ray, out hit, directionToEnemyPart.magnitude, LayerMaskClass.AI))
                {
                    DebugDrawer.Line(partPosition, hit.point, 0.025f, Color.green, 0.25f);

                    return false;
                }

                LayerMask glass = LayerMask.GetMask(new string[] { "TransparentCollider" });

                if (Physics.Raycast(ray, out hit, directionToEnemyPart.magnitude, glass))
                {
                    Ray glassToEnemyPart = new Ray(hit.point, directionToEnemyPart);

                    float distanceFromGlass = (hit.point - partPosition).magnitude;

                    if (!Physics.Raycast(glassToEnemyPart, out hit, distanceFromGlass, LayerMaskClass.HighPolyWithTerrainMaskAI))
                    {
                        DebugDrawer.Line(partPosition, headPosition, 0.025f, Color.blue, 0.25f);

                        return true;
                    }
                }

                DebugDrawer.Line(partPosition, hit.point, 0.025f, Color.white, 0.25f);

                return false;
            }

            DebugDrawer.Line(partPosition, headPosition, 0.025f, Color.red, 0.25f);

            return true;
        }

        private bool CheckIfVisible(Vector3 partPosition, float addVisibility)
        {
            /*
            Vector3 headPoint = botOwner_0.LookSensor._headPoint;
            lastSeenOffset = Vector3.zero;
            if (part.Key.Collider != null)
            {
                if (part.Value.LastVisibilityCastSucceed)
                {
                    lastSeenOffset = part.Value.LastVisibilityCastOffsetLocal;
                }
                else
                {
                    lastSeenOffset = part.Key.Collider.GetRandomPointToCastLocal(headPoint);
                }
                Vector3 b = part.Key.Collider.transform.TransformVector(lastSeenOffset);
                partPosition += b;
            }
            */
            Vector3 headPoint = botOwner_0.LookSensor._headPoint;
            if (botOwner_0.LookSensor.VisibleDist + addVisibility < (partPosition - headPoint).magnitude)
            {
                return false;
            }
            return true;
        }

        private void DoRaycasts(out bool hitObject1, out bool noHitOnBothRays, float enemyMagnitude, Vector3 partPos, Vector3 botHead)
        {
            Vector3 direction = partPos - botHead;

            Ray ray = new Ray(botHead, direction);
            Ray ray2 = new Ray(partPos, -direction);

            LayerMask mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

            hitObject1 = !Physics.Raycast(ray, out raycastHit2, enemyMagnitude, mask);

            bool hitAIObject1 = !hitObject1 && Contains(LayerMaskClass.AI, raycastHit2.collider.gameObject.layer);
            bool hitAIObject2 = false;
            bool freelook;
            if (hitObject1)
            {
                hitAIObject2 = (!(noHitOnBothRays = !Physics.Raycast(ray2, out raycastHit, enemyMagnitude, mask)) && Contains(LayerMaskClass.AI, raycastHit.collider.gameObject.layer));

                freelook = noHitOnBothRays;
            }
            else if (hitAIObject1)
            {
                hitAIObject2 = (!(noHitOnBothRays = !Physics.Raycast(ray2, out raycastHit, enemyMagnitude, mask)) && Contains(LayerMaskClass.AI, raycastHit.collider.gameObject.layer));

                freelook = false;
            }
            else
            {
                noHitOnBothRays = false;
                freelook = false;
            }

            AddAILayerObjects(freelook, hitAIObject1, hitAIObject2, noHitOnBothRays);
        }

        private void AddAILayerObjects(bool freelook, bool hitAIObject1, bool hitAIObject2, bool noHitOnBothRays)
        {
            Dictionary<GameObject, GClass553> aiLookLayerObjects = botOwner_0.BotsController.AILayerLookObjetcs;
            bool addAILayerObject = false;
            if (!freelook)
            {
                if (hitAIObject1)
                {
                    if (!aiLookLayerObjects.TryGetValue(raycastHit2.collider.gameObject, out gclass))
                    {
                        gclass = new GClass553(raycastHit2.collider.gameObject);
                        aiLookLayerObjects.Add(raycastHit2.collider.gameObject, gclass);
                    }
                    addAILayerObject = true;
                }
                if (hitAIObject2)
                {
                    if (!aiLookLayerObjects.TryGetValue(raycastHit.collider.gameObject, out gclass))
                    {
                        gclass = new GClass553(raycastHit.collider.gameObject);
                        aiLookLayerObjects.Add(raycastHit.collider.gameObject, gclass);
                    }
                    addAILayerObject = true;
                }
            }
            if (addAILayerObject)
            {
                if (!noHitOnBothRays)
                {
                    if (hitAIObject2 && !aiLookLayerObjects.TryGetValue(raycastHit.collider.gameObject, out gclass2))
                    {
                        gclass2 = new GClass553(raycastHit.collider.gameObject);
                        aiLookLayerObjects.Add(raycastHit.collider.gameObject, gclass2);
                    }
                }
                else if (!aiLookLayerObjects.TryGetValue(raycastHit2.collider.gameObject, out gclass2))
                {
                    gclass2 = new GClass553(raycastHit2.collider.gameObject);
                    aiLookLayerObjects.Add(raycastHit2.collider.gameObject, gclass2);
                }
            }
        }

        private float GetObstructionDistance(out bool aiLookGrassorNoGrass, bool hitObject1, bool noHitOnBothRays, float magnitude, float seenCoef, Vector3 enemyPart, Vector3 headPosition)
        {
            float obstructionDistance = 0f;
            bool aiLookGrass = false;
            bool aiNotLookGrass = false;

            if (gclass != null)
            {
                if (gclass.LookObjectTypeAI == LookObjectTypeAI.grass)
                {
                    aiLookGrass = true;
                }
                else
                {
                    aiNotLookGrass = true;
                }
                if (gclass2 != null)
                {
                    if (part.Key.Owner.AIData.IsInTree)
                    {
                        obstructionDistance = (raycastHit2.point - enemyPart).magnitude;
                    }
                    else if (!noHitOnBothRays && !hitObject1)
                    {
                        obstructionDistance = (raycastHit2.point - raycastHit.point).magnitude;
                    }
                    else if (!hitObject1)
                    {
                        obstructionDistance = (enemyPart - raycastHit2.point).magnitude;
                    }
                    else
                    {
                        obstructionDistance = (raycastHit.point - headPosition).magnitude * botOwner_0.Settings.FileSettings.Look.INSIDE_BUSH_COEF;
                    }
                }
                else
                {
                    obstructionDistance = (raycastHit2.point - enemyPart).magnitude;
                }
                obstructionDistance = Mathf.Min(obstructionDistance, magnitude);
            }

            if (aiLookGrassorNoGrass = (aiLookGrass || aiNotLookGrass))
            {
                float dist = obstructionDistance;
                float num2 = GrassVisibility(dist, part.Key.Owner.AIData.GetFlare, ref hitObject1, botOwner_0);
                seenCoef /= num2;
            }

            CheckGreenType(obstructionDistance, aiLookGrass, aiLookGrassorNoGrass);

            return seenCoef;
        }

        private void CheckGreenType(float obstructionDistance, bool aiLookGrass, bool aiLookGrassorNoGrass)
        {
            if (part.Key.BodyPartType == BodyPartType.body)
            {
                part.Key.GrassDist = obstructionDistance;
                if (aiLookGrassorNoGrass)
                {
                    if (aiLookGrass)
                    {
                        part.Key.GreenType = 1;
                    }
                    else
                    {
                        part.Key.GreenType = 2;
                    }
                }
                else
                {
                    part.Key.GreenType = 0;
                }
            }
        }

        private static float GrassVisibility(float dist, bool flare, ref bool freeLook, BotOwner Owner)
        {
            float num = flare ? Owner.Settings.FileSettings.Look.MAX_VISION_GRASS_METERS_FLARE_OPT : Owner.Settings.FileSettings.Look.MAX_VISION_GRASS_METERS_OPT;
            float num2 = 1f - num * dist;
            float num3 = 1f + dist * 1.5f;
            float num4 = num2 / num3;
            freeLook = true;
            if (num4 <= 0.001f)
            {
                num4 = 0.001f;
                freeLook = false;
            }
            else if (num4 > 1f)
            {
                num4 = 1f;
            }
            return num4;
        }

        private static bool Contains(LayerMask mask, int layer)
        {
            return mask == (mask | 1 << layer);
        }

        private bool CanBotSeeEnemy(bool hitObject1, bool noHitOnBothRays, bool aiLookGrassorNoGrass, float seenCoef, bool onSenceGreen, bool onSense)
        {
            bool canSee = hitObject1 && noHitOnBothRays;
            bool onSenceGreen2 = aiLookGrassorNoGrass && onSenceGreen;
            bool result = part.Value.UpdateVision(seenCoef, canSee, onSense, onSenceGreen2, botOwner_0, part.Key.Owner.AIData.FlarePower);
            part.Value.LastVisibilityCastSucceed = canSee;
            return result;
        }
    }

    public class BotLineObject : MonoBehaviour
    {
        private BotOwner Bot;
        private Player LocalPlayer;
        public GameObject LineObject { get; private set; }
        public FollowLineScript LineScript { get; private set; }

        private void Awake()
        {
            Bot = GetComponent<BotOwner>();
            FindYourPlayer();
            StartCoroutine(LineTracker());
        }

        private void FindYourPlayer()
        {
            if (LocalPlayer == null)
            {
                LocalPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.Find(p => p.IsYourPlayer);
            }

            if (LineObject == null)
            {
                LineObject = FollowLine(LocalPlayer.gameObject, Bot.GetPlayer.gameObject, 0.02f, Color.white);
            }

            if (LineScript == null)
            {
                LineScript = LineObject.GetComponent<FollowLineScript>();
            }
        }

        private IEnumerator LineTracker()
        {
            while (true)
            {
                if (LineScript == null || LineObject == null || LocalPlayer == null)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                if (Bot.IsDead)
                {
                    Destroy(LineObject);
                    StopAllCoroutines();
                    yield break;
                }

                if (Bot.Memory.GoalEnemy != null && Bot.Memory.GoalEnemy.Person.GetPlayer.IsYourPlayer)
                {
                    if (Bot.Memory.GoalEnemy.CanShoot)
                    {
                        LineScript.SetColor(Color.red);
                    }
                    else if (Bot.Memory.GoalEnemy.IsVisible)
                    {
                        LineScript.SetColor(Color.yellow);
                    }
                    else
                    {
                        LineScript.SetColor(Color.white);
                    }
                }
                else if (Vector3.Distance(Bot.Transform.position, LocalPlayer.Transform.position) > 50f)
                {
                    LineScript.SetColor(Color.clear);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}