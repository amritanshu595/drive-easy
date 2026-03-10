using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Touch controls for Android:
/// - Drag UP   = accelerate forward
/// - Drag DOWN = brake / reverse
/// - Drag LEFT/RIGHT = steer
/// - Diagonal = combined throttle + steer
/// </summary>
public class SwipeInputController : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float steerDeadzone          = 15f;
    public float throttleDeadzone       = 15f;
    public float steerFullLockPixels    = 100f;
    public float throttleFullLockPixels = 80f;
    public float steerSmoothing         = 0.06f;
    public float throttleSmoothing      = 0.05f;

    // Outputs read by CarControllerBridge
    [HideInInspector] public float SteerInput;    // -1 left, +1 right
    [HideInInspector] public float ThrottleInput; // +1 forward, -1 reverse
    [HideInInspector] public float BrakeInput;    // 1 = braking

    // Private state
    private Vector2 _touchStart;
    private bool    _touching;
    private int     _touchId = -1;
    private float   _steerTarget;
    private float   _throttleTarget;
    private float   _steerVel;
    private float   _throttleVel;
    private float   _reverseTimer;

    void Update()
    {
        bool keyboardUsed = HandleKeyboardInput();

        if (!keyboardUsed)
        {
#if UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // Smooth the outputs
        SteerInput    = Mathf.SmoothDamp(SteerInput,    _steerTarget,    ref _steerVel,    steerSmoothing);
        ThrottleInput = Mathf.SmoothDamp(ThrottleInput, _throttleTarget, ref _throttleVel, throttleSmoothing);
    }

    // ── Keyboard (Editor) ─────────────────────────────────────────────────────
    bool HandleKeyboardInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;

        float h = 0f, v = 0f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;

        if (h == 0f && v == 0f) { _reverseTimer = 0f; return false; }

        _steerTarget = h;
        ApplyVertical(v);
        return true;
    }

    // ── Touch (Android Device) ────────────────────────────────────────────────
    void HandleTouchInput()
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        foreach (var touch in ts.touches)
        {
            var phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began && !_touching)
            {
                _touching   = true;
                _touchId    = touch.touchId.ReadValue();
                _touchStart = touch.position.ReadValue();
            }

            if (touch.touchId.ReadValue() != _touchId) continue;

            if (phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                Vector2 current = touch.position.ReadValue();
                ApplyDelta(current - _touchStart);
            }

            if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                ReleaseFinger();
            }
        }
    }

    // ── Mouse (Editor) ────────────────────────────────────────────────────────
    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _touching   = true;
            _touchStart = mouse.position.ReadValue();
        }
        if (_touching && mouse.leftButton.isPressed)
            ApplyDelta(mouse.position.ReadValue() - _touchStart);
        if (_touching && mouse.leftButton.wasReleasedThisFrame)
            ReleaseFinger();
    }

    // ── Core logic ────────────────────────────────────────────────────────────
    void ApplyDelta(Vector2 delta)
    {
        // Horizontal steering
        if (Mathf.Abs(delta.x) > steerDeadzone)
            _steerTarget = Mathf.Clamp(delta.x / steerFullLockPixels, -1f, 1f);
        else
            _steerTarget = 0f;

        // Vertical throttle/brake/reverse
        float vertical = 0f;
        if (Mathf.Abs(delta.y) > throttleDeadzone)
            vertical = Mathf.Clamp(delta.y / throttleFullLockPixels, -1f, 1f);

        ApplyVertical(vertical);
    }

    void ApplyVertical(float v)
    {
        if (v > 0.05f)
        {
            // FORWARD
            _throttleTarget = v;
            BrakeInput      = 0f;
            _reverseTimer   = 0f;
        }
        else if (v < -0.05f)
        {
            // BRAKE first, then REVERSE after delay
            _reverseTimer += Time.deltaTime;

            if (_reverseTimer < 0.4f)
            {
                // Braking phase
                BrakeInput      = 1f;
                _throttleTarget = 0f;
            }
            else
            {
                // Reverse phase
                BrakeInput      = 0f;
                _throttleTarget = v; // negative value = reverse
            }
        }
        else
        {
            // No vertical input
            _throttleTarget = 0f;
            BrakeInput      = 0f;
            _reverseTimer   = 0f;
        }
    }

    void ReleaseFinger()
    {
        _touching       = false;
        _touchId        = -1;
        _steerTarget    = 0f;
        _throttleTarget = 0f;
        BrakeInput      = 0f;
        _reverseTimer   = 0f;
    }
}