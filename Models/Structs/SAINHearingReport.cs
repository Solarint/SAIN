using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Models.Structs;

public struct SAINHearingReport
{
    public Vector3 position;
    public SAINSoundType soundType;
    public EEnemyPlaceType placeType;
    public bool isDanger;
    public bool shallReportToSquad;
}
