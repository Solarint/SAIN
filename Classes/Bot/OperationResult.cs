namespace SAIN.SAINComponent.Classes;

public struct OperationResult
{
    public bool Success;
    public string Error;

    public OperationResult(bool success, string error = null)
    {
        Success = success;
        Error = error;
    }
}