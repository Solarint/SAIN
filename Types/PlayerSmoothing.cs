using EFT;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAIN.Types.PlayerSmoothing;

public class PredictivePositionSmoother
{
    private Vector3 _smoothedPosition;
    private Vector3 _targetVelocity;
    private Vector3 _currentVelocity; // For SmoothDamp
    private Vector3 _velocitySmoothing; // For velocity estimation SmoothDamp
    private bool _initialized;

    // Smoothing parameters
    public float SmoothTime { get; set; } = 0.35f; // Time to reach target (lower = faster)

    public float PredictionStrength { get; set; } = 1f; // How much to compensate for lag
    public float VelocitySmoothTime { get; set; } = 0.1f; // Velocity estimation smoothing time
    public float MaxPredictionDistance { get; set; } = 5f; // Clamp prediction to reasonable bounds

    /// <summary>
    /// Current smoothed position
    /// </summary>
    public Vector3 Position => _smoothedPosition;

    /// <summary>
    /// Estimated target velocity
    /// </summary>
    public Vector3 Velocity => _targetVelocity;

    /// <summary>
    /// Initialize or reset the smoother with a starting position
    /// </summary>
    /// <param name="initialPosition">Starting position</param>
    public void Initialize(Vector3 initialPosition)
    {
        _smoothedPosition = initialPosition;
        _targetVelocity = Vector3.zero;
        _currentVelocity = Vector3.zero;
        _velocitySmoothing = Vector3.zero;
        _initialized = true;
    }

    /// <summary>
    /// Update the smoother with a new target position
    /// </summary>
    /// <param name="targetPosition">Current target position</param>
    /// <param name="targetVelocity">Current target position</param>
    /// <param name="deltaTime">Time since last update</param>
    /// <returns>New smoothed position</returns>
    public Vector3 Update(Vector3 targetPosition, Vector3 targetVelocity, float deltaTime)
    {
        if (!_initialized)
        {
            Initialize(targetPosition);
            return _smoothedPosition;
        }

        if (deltaTime <= 0f)
        {
            return _smoothedPosition;
        }

        // Calculate target velocity with SmoothDamp
        _targetVelocity = Vector3.SmoothDamp(_targetVelocity, targetVelocity,
            ref _velocitySmoothing, VelocitySmoothTime, Mathf.Infinity, deltaTime);

        // Predict target position with lag compensation
        var lagCompensation = SmoothTime * PredictionStrength;
        var predictedTarget = targetPosition + _targetVelocity * lagCompensation;

        // Clamp prediction
        var predictionOffset = predictedTarget - targetPosition;
        if (predictionOffset.magnitude > MaxPredictionDistance)
        {
            predictionOffset = predictionOffset.normalized * MaxPredictionDistance;
            predictedTarget = targetPosition + predictionOffset;
        }

        // Smooth towards predicted position using SmoothDamp
        _smoothedPosition = Vector3.SmoothDamp(_smoothedPosition, predictedTarget,
            ref _currentVelocity, SmoothTime, Mathf.Infinity, deltaTime);

        // Convergence guarantee
        var distanceToTarget = Vector3.Distance(_smoothedPosition, targetPosition);
        var velocityMagnitude = _targetVelocity.magnitude;

        if (!(distanceToTarget < 0.001f) || !(velocityMagnitude < 0.01f)) return _smoothedPosition;

        _smoothedPosition = targetPosition;
        _currentVelocity = Vector3.zero;

        return _smoothedPosition;
    }

    /// <summary>
    /// Force immediate convergence to target position
    /// </summary>
    /// <param name="targetPosition">Position to snap to</param>
    public void Snap(Vector3 targetPosition)
    {
        _smoothedPosition = targetPosition;
        _targetVelocity = Vector3.zero;
        _currentVelocity = Vector3.zero;
        _velocitySmoothing = Vector3.zero;
    }
}

public class SmoothingDebugger : MonoBehaviour
{
    public Player player;

    private GameObject _sphere1;
    private LineRenderer _line1;

    private GameObject _sphere2;
    private LineRenderer _line2;

    private Vector3 _naiveSmoothedPosition;
    private Vector3 _naiveSmoothedVelocity;

    private readonly PredictivePositionSmoother _positionSmoother = new();

    private void Awake()
    {
        // Create first sphere
        _sphere1 = CreateSphere(Color.magenta);
        _line1 = CreateLine(_sphere1, Color.gray, Color.magenta, 0.05f);

        // Create second sphere
        _sphere2 = CreateSphere(Color.green);
        _line2 = CreateLine(_sphere2, Color.gray, Color.green, 0.05f);
    }

    private void FixedUpdate()
    {
        var targetPosition = player.PlayerBones.Head.Original.position;
        var smoothedPos = _positionSmoother.Update(targetPosition, player.Velocity, Time.fixedDeltaTime);

        _naiveSmoothedPosition = Vector3.SmoothDamp(
            _naiveSmoothedPosition, targetPosition, ref _naiveSmoothedVelocity, 0.35f, Mathf.Infinity, Time.fixedDeltaTime
        );

        UpdatePositions(_naiveSmoothedPosition, targetPosition, _sphere1, _line1);
        UpdatePositions(smoothedPos, targetPosition, _sphere2, _line2);
    }

    private void UpdatePositions(Vector3 smoothedPosition, Vector3 targetPosition, GameObject sphere, LineRenderer line)
    {
        sphere.transform.position = smoothedPosition;
        line.SetPosition(0, targetPosition);
        line.SetPosition(1, smoothedPosition);
    }

    private LineRenderer CreateLine(GameObject sphere, Color startColor, Color endColor, float width)
    {
        var line = sphere.AddComponent<LineRenderer>();

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = startColor;
        line.endColor = endColor;

        line.startWidth = width;
        line.endWidth = width;

        SAIN.Logger.LogInfo($"Smoothing createline> posCount");

        line.positionCount = 2;

        return line;
    }

    private GameObject CreateSphere(Color color)
    {
        // Create sphere primitive
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material = new Material(Shader.Find("Sprites/Default")) {
            color = color
        };

        sphere.transform.localScale = Vector3.one * 0.1f;

        // Remove the collider
        var collider = sphere.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }

        // Set this GameObject as parent
        sphere.transform.SetParent(transform);

        return sphere;
    }
}