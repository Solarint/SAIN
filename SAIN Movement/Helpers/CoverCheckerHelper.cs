using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;

namespace Movement.Helpers
{
    public class CoverChecker
    {
        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;

        public bool HeadCover { get; private set; }
        public bool ChestCover { get; private set; }
        public bool WaistCover { get; private set; }
        public bool ProneCover { get; private set; }
        public bool CanShoot { get; private set; }
        public Vector3? GoodCoverPosition { get; private set; }

        private Vector3 enemyGunPos;

        /// <summary>
        /// Constructor for CoverChecker class.
        /// </summary>
        /// <param name="bot">The BotOwner object.</param>
        /// <returns>A new CoverChecker object.</returns>
        public CoverChecker(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }

        /// <summary>
        /// Analyzes the cover position of the character and sets the appropriate cover values.
        /// </summary>
        /// <param name="coverPoint">The position of the cover.</param>
        /// <param name="head">Whether to analyze head cover.</param>
        /// <param name="chest">Whether to analyze chest cover.</param>
        /// <param name="waist">Whether to analyze waist cover.</param>
        /// <param name="prone">Whether to analyze prone cover.</param>
        /// <param name="reset">Whether to reset the previous saved cover values.</param>
        /// <returns>Whether the cover position is valid at all.</returns>
        public bool AnalyseCoverPosition(Vector3 coverPoint, bool head = true, bool chest = true, bool waist = true, bool prone = true, bool reset = true)
        {
            if (reset)
            {
                // Reset Saved Cover Values
                HeadCover = false;
                ChestCover = false;
                WaistCover = false;
                ProneCover = false;
                CanShoot = false;
            }

            enemyGunPos = BotOwner.Memory.GoalEnemy.Owner.GetPlayer.PlayerBones.WeaponRoot.position;

            if (head)
                Head(coverPoint, 0.1f);

            if (chest)
                Chest(coverPoint, 1f, 0.25f);

            if (waist)
                Waist(coverPoint, 0.6f, 0.25f);

            if (prone)
                Prone(coverPoint, 0.2f, 0.1f);

            if (!HeadCover && !ChestCover && !WaistCover && !ProneCover)
            {
                GoodCoverPosition = null;
                return false;
            }

            if (WaistCover && ProneCover)
            {
                GoodCoverPosition = coverPoint;
            }

            Shoot(coverPoint);

            return true;
        }

        /// <summary>
        /// Checks if a bot can shoot from a coverpoint
        /// </summary>
        /// <param name="coverPoint">The cover point to check from.</param>
        /// <returns>True if they can shoot.</returns>
        private bool Shoot(Vector3 coverPoint)
        {
            Vector3 myHeadPos = coverPoint;
            myHeadPos.y += BotOwner.LookSensor._headPoint.y;

            if (!CheckVisiblity(BotOwner.WeaponRoot.position, enemyGunPos, 0.01f))
            {
                CanShoot = true;
                return true;
            }
            else
            {
                CanShoot = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the head of the bot is covered by the given cover point.
        /// </summary>
        /// <param name="coverPoint">The cover point to check.</param>
        /// <param name="sphereSize">The size of the sphere to check.</param>
        /// <returns>True if the head is covered, false otherwise.</returns>
        private bool Head(Vector3 coverPoint, float sphereSize)
        {
            Vector3 myHeadPos = coverPoint;
            myHeadPos.y += BotOwner.LookSensor._headPoint.y;

            if (!CheckVisiblity(myHeadPos, enemyGunPos, sphereSize))
            {
                HeadCover = true;
                return true;
            }
            else
            {
                HeadCover = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the chest is visible to the enemy gun position.
        /// </summary>
        /// <param name="coverPoint">The cover point.</param>
        /// <param name="chestY">The chest Y.</param>
        /// <param name="sphereSize">The size of the sphere.</param>
        /// <returns>
        /// Returns true if the chest is not visible to the enemy gun position, false otherwise.
        /// </returns>
        private bool Chest(Vector3 coverPoint, float chestY, float sphereSize)
        {
            Vector3 myChestPos = coverPoint;
            myChestPos.y += chestY;

            if (!CheckVisiblity(myChestPos, enemyGunPos, sphereSize))
            {
                ChestCover = true;
                return true;
            }
            else
            {
                ChestCover = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the waist is covered by a sphere of given size.
        /// </summary>
        /// <param name="coverPoint">The point to check for waist cover.</param>
        /// <param name="waistY">The y-axis offset for the waist.</param>
        /// <param name="sphereSize">The size of the sphere to check for waist cover.</param>
        /// <returns>True if the waist is covered, false otherwise.</returns>
        private bool Waist(Vector3 coverPoint, float waistY, float sphereSize)
        {
            Vector3 myWaistPos = coverPoint;
            myWaistPos.y += waistY;

            if (!CheckVisiblity(myWaistPos, enemyGunPos, sphereSize))
            {
                WaistCover = true;
                return true;
            }
            else
            {
                WaistCover = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the cover point is visible to the enemy gun position.
        /// </summary>
        /// <param name="coverPoint">The cover point to check.</param>
        /// <param name="enemyGunPos">The enemy gun position.</param>
        /// <param name="sphereSize">The size of the sphere to check.</param>
        /// <returns>True if the cover point is visible, false otherwise.</returns>
        private bool Prone(Vector3 coverPoint, float proneY, float sphereSize)
        {
            Vector3 myPronePos = coverPoint;
            myPronePos.y += proneY;

            if (!CheckVisiblity(myPronePos, enemyGunPos, sphereSize))
            {
                ProneCover = true;
                return true;
            }
            else
            {
                ProneCover = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if a line between two points is visible, using a spherecast.
        /// </summary>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="sphereSize">The size of the sphere used for the spherecast.</param>
        /// <returns>True if the line is visible, false otherwise.</returns>
        private bool CheckVisiblity(Vector3 start, Vector3 end, float sphereSize)
        {
            Vector3 direction = end - start;
            Ray ray = new Ray(start, direction);
            float distance = Vector3.Distance(start, end);

            if (Physics.SphereCast(ray, sphereSize, out RaycastHit hit, distance))
            {
                Line(start, hit.point, 0.01f, Color.white, 5f);
                return false;
            }

            return true;
        }
    }
}