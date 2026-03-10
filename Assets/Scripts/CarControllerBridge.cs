using UnityEngine;
using System.Reflection;

/// <summary>
/// Calls Prometeo's own methods directly based on SwipeInputController output.
/// GoForward, GoReverse, Brakes, TurnLeft, TurnRight all use Prometeo's real physics.
/// </summary>
[RequireComponent(typeof(SwipeInputController))]
public class CarControllerBridge : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour prometeoCarController;

    [Header("Thresholds")]
    public float throttleThreshold = 0.05f;
    public float steerThreshold    = 0.05f;

    private SwipeInputController _swipe;
    private Component            _prometeo;
    private System.Type          _prometeoType;

    private MethodInfo _goForward;
    private MethodInfo _goReverse;
    private MethodInfo _brakes;
    private MethodInfo _turnLeft;
    private MethodInfo _turnRight;
    private MethodInfo _throttleOff;
    private MethodInfo _resetSteering;
    private MethodInfo _recoverTraction;

    private FieldInfo  _deceleratingCar;
    private FieldInfo  _useTouchControls;

    void Awake()
    {
        _swipe = GetComponent<SwipeInputController>();

        _prometeo = prometeoCarController != null
            ? prometeoCarController
            : FindPrometeo();

        if (_prometeo == null)
        {
            Debug.LogError("[Bridge] PrometeoCarController not found!");
            return;
        }

        _prometeoType = _prometeo.GetType();

        // Cache all methods
        _goForward      = GetMethod("GoForward");
        _goReverse      = GetMethod("GoReverse");
        _brakes         = GetMethod("Brakes");
        _turnLeft       = GetMethod("TurnLeft");
        _turnRight      = GetMethod("TurnRight");
        _throttleOff    = GetMethod("ThrottleOff");
        _resetSteering  = GetMethod("ResetSteeringAngle");
        _recoverTraction= GetMethod("RecoverTraction");

        // Cache fields
        _deceleratingCar = _prometeoType.GetField("deceleratingCar",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _useTouchControls = _prometeoType.GetField("useTouchControls",
            BindingFlags.Public | BindingFlags.Instance);

        // Disable Prometeo's own input
        if (_useTouchControls != null)
            _useTouchControls.SetValue(_prometeo, false);

        Debug.Log("[Bridge] Initialized — Prometeo methods ready.");
    }

    void Update()
    {
        if (_prometeo == null) return;

        float throttle = _swipe.ThrottleInput;
        float steer    = _swipe.SteerInput;
        float brake    = _swipe.BrakeInput;

        // ── Throttle / Brake / Reverse ─────────────────────────────────────
        if (throttle > throttleThreshold)
        {
            // FORWARD
            CancelDecelerate();
            _goForward?.Invoke(_prometeo, null);
            _recoverTraction?.Invoke(_prometeo, null);
        }
        else if (brake > 0.5f)
        {
            // BRAKING
            CancelDecelerate();
            _brakes?.Invoke(_prometeo, null);
        }
        else if (throttle < -throttleThreshold)
        {
            // REVERSE
            CancelDecelerate();
            _goReverse?.Invoke(_prometeo, null);
        }
        else
        {
            // COAST — let Prometeo decelerate naturally
            _throttleOff?.Invoke(_prometeo, null);
            StartDecelerate();
        }

        // ── Steering ────────────────────────────────────────────────────────
        if (steer < -steerThreshold)
            _turnLeft?.Invoke(_prometeo, null);
        else if (steer > steerThreshold)
            _turnRight?.Invoke(_prometeo, null);
        else
            _resetSteering?.Invoke(_prometeo, null);
    }

    void CancelDecelerate()
    {
        _prometeoType
            .GetMethod("CancelInvoke",
                BindingFlags.Public | BindingFlags.Instance,
                null, new System.Type[]{ typeof(string) }, null)
            ?.Invoke(_prometeo, new object[]{ "DecelerateCar" });

        if (_deceleratingCar != null)
            _deceleratingCar.SetValue(_prometeo, false);
    }

    void StartDecelerate()
    {
        bool already = _deceleratingCar != null &&
                       (bool)_deceleratingCar.GetValue(_prometeo);
        if (already) return;

        _prometeoType
            .GetMethod("InvokeRepeating",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new System.Type[]{ typeof(string), typeof(float), typeof(float) },
                null)
            ?.Invoke(_prometeo, new object[]{ "DecelerateCar", 0f, 0.1f });

        if (_deceleratingCar != null)
            _deceleratingCar.SetValue(_prometeo, true);
    }

    Component FindPrometeo()
    {
        foreach (Component c in GetComponents<Component>())
        {
            string n = c.GetType().Name.ToLower();
            if (n.Contains("prometeo") || n.Contains("carcontroller"))
                return c;
        }
        return null;
    }

    MethodInfo GetMethod(string name)
    {
        var m = _prometeoType.GetMethod(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (m == null) Debug.LogWarning($"[Bridge] Method not found: {name}");
        return m;
    }
}