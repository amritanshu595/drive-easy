using UnityEngine;
using TMPro;

/// <summary>
/// Lightweight FPS display. Attach to the FPS label UI element (TextMeshProUGUI).
/// Alternatively, GameManager already handles FPS — use this only if you want
/// a standalone component on the Text object itself.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    [Tooltip("Update interval in seconds")]
    public float updateInterval = 0.5f;

    private TextMeshProUGUI _label;
    private float _elapsed;
    private int   _frames;

    void Awake() => _label = GetComponent<TextMeshProUGUI>();

    void Update()
    {
        _elapsed += Time.unscaledDeltaTime;
        _frames  += 1;

        if (_elapsed >= updateInterval)
        {
            float fps = _frames / _elapsed;
            _label.text = $"FPS: {fps:0}";
            _elapsed = 0f;
            _frames  = 0;
        }
    }
}
