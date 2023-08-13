using Comfort.Common;
using EFT;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINMainPlayerComponent : MonoBehaviour
    {
        public void Awake()
        {
            MainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            CamoClass = new SAINCamoClass(this);
        }

        public SAINCamoClass CamoClass { get; private set; }

        private void Start()
        {
            CamoClass.Start();
        }

        private void Update()
        {
            CamoClass.Update();
        }

        private void OnDestroy()
        {
            CamoClass.OnDestroy();
        }

        private void OnGUI()
        {
            CamoClass.OnGUI();
        }

        public Player MainPlayer { get; private set; }
    }
}