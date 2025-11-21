using EFT.Interactive;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Memory;

public class SAINExtract
{
    public Vector3? ExfilPosition { get; set; }
    public ExfiltrationPoint ExfilPoint { get; set; }
    public EExtractReason ExtractReason { get; set; }
    public EExtractStatus ExtractStatus { get; set; }
}
