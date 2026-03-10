using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float roundDuration = 60f;

    [Header("HUD References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI fpsText;

    [Header("Screens")]
    public GameObject hudScreen;
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;

    public bool  IsPlaying { get; private set; }
    public int   Score     { get; private set; }
    public float TimeLeft  { get; private set; }

    private float _fpsTimer;
    private int   _fpsFrames;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Auto-find UI references if not assigned in Inspector
        AutoFindReferences();
    }

    void AutoFindReferences()
    {
        // Find all TMP texts in scene by name if slots are empty
        if (scoreText == null)     scoreText     = FindTMP("ScoreText");
        if (timerText == null)     timerText     = FindTMP("TimerText");
        if (fpsText == null)       fpsText       = FindTMP("FPSText");
        if (finalScoreText == null) finalScoreText = FindTMP("FinalScoreText");

        // Find screens by name if slots are empty
        if (hudScreen == null)  hudScreen  = FindGO("HUDScreen");
        if (endScreen == null)  endScreen  = FindGO("EndScreen");

        // Log what was found
        Debug.Log($"[GameManager] scoreText={scoreText != null} timerText={timerText != null} " +
                  $"fpsText={fpsText != null} hudScreen={hudScreen != null} endScreen={endScreen != null}");
    }

    TextMeshProUGUI FindTMP(string goName)
    {
        var go = GameObject.Find(goName);
        return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
    }

    GameObject FindGO(string goName)
    {
        return GameObject.Find(goName);
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        // Force all text objects active
        SetTextActive(scoreText);
        SetTextActive(timerText);
        SetTextActive(fpsText);

        StartRound();
    }

    void SetTextActive(TextMeshProUGUI tmp)
    {
        if (tmp != null)
        {
            tmp.gameObject.SetActive(true);
            tmp.enabled = true;
        }
    }

    public void StartRound()
    {
        Score     = 0;
        TimeLeft  = roundDuration;
        IsPlaying = true;

        if (hudScreen != null) hudScreen.SetActive(true);
        if (endScreen != null) endScreen.SetActive(false);

        UpdateScoreHUD();
        UpdateTimerHUD();
    }

    void Update()
    {
        // FPS — always runs
        _fpsFrames++;
        _fpsTimer += Time.unscaledDeltaTime;
        if (_fpsTimer >= 0.5f)
        {
            int fps = Mathf.RoundToInt(_fpsFrames / _fpsTimer);
            _fpsFrames = 0;
            _fpsTimer  = 0f;
            if (fpsText != null)
                fpsText.text = "FPS: " + fps;
        }

        if (!IsPlaying) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f) { TimeLeft = 0f; EndRound(); }
        UpdateTimerHUD();
    }

    public void AddScore(int amount = 1)
    {
        if (!IsPlaying) return;
        Score += amount;
        UpdateScoreHUD();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void EndRound()
    {
        IsPlaying = false;
        if (hudScreen != null) hudScreen.SetActive(false);
        if (endScreen != null) endScreen.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Score;
    }

    void UpdateScoreHUD()
    {
        if (scoreText != null) scoreText.text = Score.ToString();
    }

    void UpdateTimerHUD()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(TimeLeft).ToString();
    }
}