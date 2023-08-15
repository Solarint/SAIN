using EFT.EnvironmentEffect;
using SAIN.Components.BotController;
using SAIN.Components.MainPlayer;
using SAIN.Editor;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINCamoClass : BaseMainPlayer, iMainPlayer
    {
        public SAINCamoClass(SAINMainPlayerComponent playerComp) : base(playerComp)
        {
        }

        public void Start()
        {
            MainPlayerCollider = MainPlayer.GetComponent<Collider>();
            BushLayer = LayerMask.NameToLayer("Foliage");
            GrassLayer = LayerMask.NameToLayer("Grass");
        }

        public LayerMask GrassLayer = 0;
        public LayerMask BushLayer = 0;

        public void Update()
        {
            if (MainPlayer != null)
            {
                if (FreqencyTimer < Time.time)
                {
                    var botController = SAINPlugin.BotController;
                    if (botController != null)
                    {
                        TimeOfDay = botController.TimeVision.TimeOfDay;
                    }
                    var envManger = EnvironmentManager.Instance;
                    if (envManger != null)
                    {
                        EnvironmentType = envManger.GetEnvironmentByPos(MainPlayer.Position);
                    }
                    FreqencyTimer = Time.time + 0.5f;

                    for (int i = 0; i < BushColliders.Length; i++)
                    {
                        BushColliders[i] = null;
                    }
                    NearBush = Physics.OverlapSphereNonAlloc(MainPlayer.MainParts[BodyPartType.body].Position, 2f, BushColliders, LayerMaskClass.HighPolyWithTerrainMaskAI) > 0;

                    bool inBush = false;
                    for (int i = 0; i < BushColliders.Length; i++)
                    {
                        if (BushColliders[i] != null)
                        {
                            if ((BushColliders[i].transform.position - MainPlayer.Position).magnitude < 0.75f)
                            {
                                inBush = true;
                                break;
                            }
                        }
                    }
                    InsideBush = inBush;

                    for (int i = 0; i < GrassColliders.Length; i++)
                    {
                        GrassColliders[i] = null;
                    }
                    OnGrass = Physics.OverlapSphereNonAlloc(MainPlayer.Position, 0.5f, GrassColliders, GrassLayer) > 0;
                }
            }
        }

        public void OnDestroy()
        {
        }

        public bool IsBushBetween(Vector3 start)
        {
            Vector3 direction = MainPlayer.MainParts[BodyPartType.body].Position - start;
            return Physics.SphereCast(start, 0.1f, direction.normalized, out var hit, direction.magnitude, BushLayer);
        }

        public Collider MainPlayerCollider { get; private set; }

        private float FreqencyTimer;
        public TimeOfDayEnum TimeOfDay { get; private set; }
        public bool NearBush { get; private set; }
        public bool InsideBush { get; private set; }
        public bool OnGrass { get; private set; }
        public bool IsProne => MainPlayer.IsInPronePose;
        public EnvironmentType EnvironmentType { get; private set; }

        private Collider[] BushColliders = new Collider[5];
        private Collider[] GrassColliders = new Collider[1];

        public void OnGUI()
        {
            if (SAINPlugin.DebugModeEnabled)
            {
                GUIUtility.ScaleAroundPivot(RectLayout.ScaledPivot, Vector2.zero);
                for (int i = 0; i < BushColliders.Length; i++)
                {
                    var bush = BushColliders[i];

                    if (bush != null)
                    {
                        Vector3 bushPos = bush.transform.position;
                        float size = bush.bounds.size.magnitude;
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(bushPos + Vector3.up);
                        if (screenPos.z > 0)
                        {
                            GUIStyle guiStyle = GUI.skin.box;
                            GUIContent content = new GUIContent($"{bush.name} : {bush.material?.name} : {size} : {EnvironmentType}");
                            Rect guiRect = new Rect();
                            Vector2 guiSize = guiStyle.CalcSize(content);
                            guiRect.x = screenPos.x - (guiSize.x / 2);
                            guiRect.y = Screen.height - (screenPos.y + guiSize.y);
                            guiRect.size = guiSize;
                            GUI.Box(guiRect, content, guiStyle);
                        }
                        Color color = (bushPos - MainPlayer.Position).magnitude < 0.75f ? Color.blue : Color.green;
                        DebugGizmos.Sphere(bushPos, bush.bounds.size.magnitude, color, true, 1f);
                    }
                }
            }
        }
    }
}