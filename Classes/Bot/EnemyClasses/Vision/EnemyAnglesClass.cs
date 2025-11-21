using SAIN.Helpers;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyAnglesClass
{
    private const float CALC_ANGLE_FREQ = 1f / 15f;
    private const float CALC_ANGLE_FREQ_AI = 1f / 4f;
    private const float CALC_ANGLE_FREQ_KNOWN = 1f / 30f;
    private const float CALC_ANGLE_FREQ_KNOWN_AI = 1f / 15f;
    private const float CALC_ANGLE_CURRENT_COEF = 0.5f;

    public bool CanBeSeen { get; private set; }
    public float MaxVisionAngle { get; private set; }
    public float AngleToEnemy { get; private set; }
    public float AngleToEnemyHorizontal { get; private set; }
    public float AngleToEnemyHorizontalSigned { get; private set; }
    public float AngleToEnemyVertical { get; private set; }
    public float AngleToEnemyVerticalSigned { get; private set; }

    public void CalcAngles(Enemy Enemy, float currentTime)
    {
        if (_calcAngleTime < currentTime)
        {
            float delay;
            if (Enemy.IsAI)
                delay = Enemy.EnemyKnown ? CALC_ANGLE_FREQ_KNOWN_AI : CALC_ANGLE_FREQ_AI;
            else
                delay = Enemy.EnemyKnown ? CALC_ANGLE_FREQ_KNOWN : CALC_ANGLE_FREQ;

            if (Enemy.IsCurrentEnemy)
                delay *= CALC_ANGLE_CURRENT_COEF;

            _calcAngleTime = currentTime + delay;

            MaxVisionAngle = Enemy.Bot.Info.FileSettings.Core.VisibleAngle / 2f;

            Vector3 lookDir = Enemy.Bot.PlayerComponent.CharacterController.CurrentControlLookDirection;
            Vector3 enemyDirNormal = Enemy.EnemyDirectionNormal;

            AngleToEnemy = Vector3.Angle(enemyDirNormal, lookDir);
            CanBeSeen = AngleToEnemy <= MaxVisionAngle;

            float verticalAngle = CalcVerticalAngle(enemyDirNormal, lookDir, out float yDiff);
            AngleToEnemyVertical = verticalAngle;
            AngleToEnemyVerticalSigned = yDiff >= 0 ? verticalAngle : -verticalAngle;

            float horizSigned = CalcHorizontalAngle(enemyDirNormal, lookDir);
            AngleToEnemyHorizontalSigned = horizSigned;
            AngleToEnemyHorizontal = Mathf.Abs(horizSigned);
        }
    }

    public static float CalcVerticalAngle(Vector3 enemyDirNormal, Vector3 lookDirection, out float yDiff)
    {
        Vector3 enemyElevDir = new(lookDirection.x, enemyDirNormal.y, lookDirection.z);
        yDiff = (enemyElevDir.y - lookDirection.y).Round100();
        if (yDiff == 0)
        {
            return 0;
        }
        float angle = Vector3.Angle(lookDirection, enemyElevDir);
        return angle;
    }

    public static float CalcHorizontalAngle(Vector3 enemyDirNormal, Vector3 lookDirection)
    {
        enemyDirNormal.y = 0;
        lookDirection.y = 0;
        float signedAngle = Vector3.SignedAngle(lookDirection, enemyDirNormal, Vector3.up);
        return signedAngle;
    }

    private float _calcAngleTime;
}