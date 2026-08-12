using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    public event Action AnimationFinish;

    [SerializeField]
    private UIDocument _hud;
    [SerializeField]
    private UIDocument _levelCompletePanel;
    [SerializeField]
    private UIDocument _levelStartPanel;
    [SerializeField]
    private UIDocument _timeOverPanel;

    private Label _timeLabel;
    private Label _scoreLabel;
    private ScoreManager _scoreManager;
    private GameStateManager _gameStateManager;

    private void Start()
    {
        VisualElement hudRoot = _hud.rootVisualElement;
        _timeLabel = hudRoot.Q<Label>("Time");
        _scoreLabel = hudRoot.Q<Label>("Score");
        _gameStateManager = Provider.Instance.Resolve<GameStateManager>();
        _scoreManager = Provider.Instance.Resolve<ScoreManager>();
        StartCoroutine(LevelStartAnimation());

        _gameStateManager.LevelStateChanged += OnLevelStateChanged;
        enabled = false;
    }

    private void OnDestroy()
    {
        _gameStateManager.LevelStateChanged -= OnLevelStateChanged;
    }

    private void OnLevelStateChanged(GameStateManager.LevelState state)
    {
        switch (state)
        {
            case GameStateManager.LevelState.Outro:
                StartCoroutine(LevelCompleteAnimation()); 
                break;
            case GameStateManager.LevelState.TimeOver:
                StartCoroutine(TimeOverAnimation());
                break;
        }
    }

    protected void Update()
    {
        _timeLabel.text = _gameStateManager.TimeLeft.ToString("00.");
        _scoreLabel.text = _scoreManager.Score.ToString();
    }

    private IEnumerator LevelStartAnimation()
    {
        yield return null;
        _scoreLabel.text = _scoreManager.Score.ToString();
        Time.timeScale = 0;
        Label startTime = _levelStartPanel.rootVisualElement.Q<Label>("Time");
        startTime.text = _gameStateManager.TimeLeft.ToString("F0");
        _timeLabel.text = "0";

        yield return new WaitForSecondsRealtime(2f);

        float animationTime = 0;
        while (animationTime < 1)
        {
            startTime.text = (_gameStateManager.TimeLeft * (1-animationTime)).ToString("F0");
            _timeLabel.text = (_gameStateManager.TimeLeft * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.unscaledDeltaTime;
        }

        _timeLabel.text = _gameStateManager.TimeLeft.ToString("F0");
        startTime.text = "0";
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1;
        _levelStartPanel.gameObject.SetActive(false);
        enabled = true;
        AnimationFinish?.Invoke();
    }

    private IEnumerator TimeOverAnimation()
    {
        _hud.gameObject.SetActive(false);
        _timeOverPanel.gameObject.SetActive(true);
        Label finalScore = _timeOverPanel.rootVisualElement.Q<Label>("Score");
        finalScore.text = _scoreManager.Score.ToString();
        yield return new WaitForSeconds(5);
        AnimationFinish?.Invoke();
    }

    private IEnumerator LevelCompleteAnimation()
    {
        _levelCompletePanel.gameObject.SetActive(true);
        Label finalScore = _levelCompletePanel.rootVisualElement.Q<Label>("Score");
        Label bonusType = _levelCompletePanel.rootVisualElement.Q<Label>("BonusType");

        bonusType.text = "SCORE";
        finalScore.text = _scoreManager.Score.ToString();

        yield return new WaitForSeconds(2f);

        bonusType.text = "GOAL REACHED BONUS";
        yield return new WaitForSeconds(0.5f);

        float animationTime = 0;
        while (animationTime < 1)
        {
            finalScore.text = (_scoreManager.Score + _scoreManager.ScoreFinishBonus * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.deltaTime;
        }

        int score = _scoreManager.Score + _scoreManager.ScoreFinishBonus;
        finalScore.text = score.ToString();
        yield return new WaitForSeconds(1f);

        bonusType.text = $"{_gameStateManager.TimeLeft:F0} SEC LEFT BONUS";
        yield return new WaitForSeconds(0.5f);

        animationTime = 0;
        while (animationTime < 1)
        {
            finalScore.text = (score + _scoreManager.ScoreTimeBonus * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.deltaTime;
        }

        score += _scoreManager.ScoreTimeBonus;
        finalScore.text = score.ToString();

        yield return new WaitForSeconds(0.5f);
        bonusType.text = $"FINAL SCORE";

        yield return new WaitForSeconds(3f);

        AnimationFinish?.Invoke();
    }

}
