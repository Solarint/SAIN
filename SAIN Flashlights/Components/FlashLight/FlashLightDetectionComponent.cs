using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using System.Collections;
using UnityEngine;
using static SAIN.Flashlights.Config.DazzleConfig;

namespace SAIN.Flashlights.Components
{
    public class FlashLightDetection : MonoBehaviour
    {
        private Player player;
        protected static ManualLogSource Logger { get; private set; }
        private void Awake()
        {
            try
            {
                player = GetComponent<Player>();
            }
            catch
            {
                System.Console.WriteLine($"Could not get Player Component");
            }

            if (player != null)
            {
                try
                {
                    StartCoroutine(FlashLightCheck());
                }
                catch
                {
                    System.Console.WriteLine($"Could not start Flashlight Coroutine");
                }

                if (Logger == null)
                {
                    Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashLightDetection));
                }
            }
        }
        private bool LightIsOn = false;
        private IEnumerator FlashLightCheck()
        {
            while (true)
            {
                // Check if the bot is alive before continuing
                if (player.HealthController?.IsAlive == false || player == null)
                {
                    StopAllCoroutines();
                    yield break;
                }

                try
                {
                    if (player?.AIData?.UsingLight == true)
                    {
                        //Logger.LogDebug($"Light is on!");

                        LightIsOn = true;
                        CreateSussyPlace();
                    }
                    else if (LightIsOn)
                    {
                        LightIsOn = false;
                    }
                }
                catch
                {
                    Logger.LogError($"Could not create sus");
                }

                try
                {
                    DetectTheSussyPlace();
                }
                catch
                {
                    Logger.LogError($"Could not Check for sus");
                }

                // Overall Check Frequency
                yield return new WaitForSeconds(0.25f);
            }
        }
        public Vector3[] DetectionPoints;
        private void CreateSussyPlace()
        {
            if (player == null)
            {
                return;
            }

            Vector3 playerPosition = player.WeaponRoot.position;
            Vector3 playerLookDirection = player.LookDirection;
            int numberOfRaysHorizontal = 10;
            int numberOfRaysVertical = 10;
            float detectionAngleHorizontal = 20f;
            float detectionAngleVertical = 10f;
            float detectionDistance = 60f;

            // Initialize the DetectionPoints array
            DetectionPoints = new Vector3[numberOfRaysHorizontal * numberOfRaysVertical];

            int index = 0;
            for (int i = 0; i < numberOfRaysHorizontal; i++)
            {
                float angleHorizontal = Mathf.Lerp(-detectionAngleHorizontal / 2, detectionAngleHorizontal / 2, (float)i / (numberOfRaysHorizontal - 1));
                Quaternion rotationHorizontal = Quaternion.AngleAxis(angleHorizontal, player.Transform.up);

                for (int j = 0; j < numberOfRaysVertical; j++)
                {
                    float angleVertical = Mathf.Lerp(-detectionAngleVertical / 2, detectionAngleVertical / 2, (float)j / (numberOfRaysVertical - 1));
                    Quaternion rotationVertical = Quaternion.AngleAxis(angleVertical, player.Transform.right);

                    Vector3 rayDirection = rotationHorizontal * rotationVertical * playerLookDirection;

                    if (Physics.Raycast(playerPosition, rayDirection, out RaycastHit hit, detectionDistance, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        DetectionPoints[index] = hit.point;
                        StartCoroutine(ExpireDetectionPoints(index, 0.25f));
                        Helpers.DebugDrawer.Sphere(hit.point, 0.1f, Color.red, 0.25f);
                    }
                    else
                    {
                        DetectionPoints[index] = playerPosition + rayDirection * detectionDistance;
                        StartCoroutine(ExpireDetectionPoints(index, 0.25f));
                        Helpers.DebugDrawer.Sphere(DetectionPoints[index], 0.1f, Color.red, 0.25f);
                    }

                    index++;
                }
            }
        }
        private IEnumerator ExpireDetectionPoints(int index, float delay)
        {
            yield return new WaitForSeconds(delay);
            DetectionPoints[index] = Vector3.zero;
        }
        private void DetectTheSussyPlace()
        {
            if (!player.IsAI || player?.AIData?.BotOwner?.Memory?.GoalEnemy == null)
            {
                return;
            }

            //Logger.LogDebug($"Detecting Sus Points");
            try
            {
                FlashLightDetection flashpoint = player.AIData.BotOwner.Memory.GoalEnemy.Owner.gameObject.GetComponent<FlashLightDetection>();
                if (flashpoint?.DetectionPoints?.Length > 0 && flashpoint.LightIsOn)
                {
                    if (flashpoint.DetectionPoints[1] != Vector3.zero)
                    {
                        if (player.AIData.BotOwner.LookSensor.IsPointInVisibleSector(DetectionPoints[1]))
                        {
                            player.AIData.BotOwner.BotsGroup.AddPointToSearch(flashpoint.DetectionPoints[1], 10f, player.AIData.BotOwner, true);
                            Logger.LogDebug($"{player.name} is investigating flashlight beam. Very sus.");
                            Helpers.DebugDrawer.Sphere(DetectionPoints[1], 0.25f, Color.blue, 5f);
                        }
                    }
                }
            }
            catch { }
        }
    }
}