using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UIDocument _hud;
    [SerializeField]
    private UIDocument _levelCompletePanel;
    [SerializeField]
    private UIDocument _levelStartPanel;
    [SerializeField]
    private Rigidbody _marble;
    [SerializeField]
    private Goal _goal;

    private VisualElement _rootVE;
    private Label _timeLabel;
    private Label _scoreLabel;

    private float _time = 60;
    private int _score = 0;
    private float _accumulatedVelocity = 0;

    private enum LevelState
    {
        Intro,
        Playing,
        Outro
    };
    private LevelState _state = LevelState.Intro;

    protected void Start()
    {
        _rootVE = _hud.rootVisualElement;
        _timeLabel = _rootVE.Q<Label>("Time");
        _scoreLabel = _rootVE.Q<Label>("Score");
        _state = LevelState.Intro;
        _goal.GoalReached += OnGoalReached;
        StartCoroutine(LevelStartAnimation());
    }

    private void OnGoalReached()
    {
        _state = LevelState.Outro;
        _hud.gameObject.SetActive(false);
        _levelCompletePanel.gameObject.SetActive(true);
        StartCoroutine(LevelCompleteAnimation());
    }

    protected void Update()
    {
        if (_state == LevelState.Playing)
        {
            _time -= Time.deltaTime;
            _timeLabel.text = _time.ToString("00.");

            _accumulatedVelocity += _marble.linearVelocity.magnitude;
        }
    }

    private IEnumerator UpdateScore()
    {
        while (_state == LevelState.Playing)
        {
            _score += Mathf.FloorToInt(_accumulatedVelocity / 10.0f);
            _accumulatedVelocity = 0;
            _scoreLabel.text = _score.ToString();

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator LevelStartAnimation()
    {
        Time.timeScale = 0;
        Label startTime = _levelStartPanel.rootVisualElement.Q<Label>("Time");
        startTime.text = _time.ToString("F0");
        _timeLabel.text = "0";

        yield return new WaitForSecondsRealtime(2f);

        float animationTime = 0;
        while (animationTime < 1)
        {
            startTime.text = (_time * (1-animationTime)).ToString("F0");
            _timeLabel.text = (_time * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.unscaledDeltaTime;
        }

        _timeLabel.text = _time.ToString("F0");
        startTime.text = "0";
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1;
        _levelStartPanel.gameObject.SetActive(false);
        _state = LevelState.Playing;
        StartCoroutine(UpdateScore());
    }

    private IEnumerator LevelCompleteAnimation()
    {
        Label finalScore = _levelCompletePanel.rootVisualElement.Q<Label>("Score");
        Label bonusType = _levelCompletePanel.rootVisualElement.Q<Label>("BonusType");

        bonusType.text = "SCORE";
        finalScore.text = _score.ToString();

        yield return new WaitForSeconds(2f);

        bonusType.text = "GOAL REACHED BONUS";
        yield return new WaitForSeconds(0.5f);

        float animationTime = 0;
        while (animationTime < 1)
        {
            finalScore.text = (_score + 1000 * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.deltaTime;
        }

        _score += 1000;
        finalScore.text = _score.ToString();
        yield return new WaitForSeconds(1f);

        bonusType.text = $"{_time:F0} SEC LEFT BONUS";
        yield return new WaitForSeconds(0.5f);

        animationTime = 0;
        while (animationTime < 1)
        {
            finalScore.text = (_score + _time * 200 * animationTime).ToString("F0");
            yield return null;
            animationTime += Time.deltaTime;
        }

        _score += Mathf.CeilToInt(_time * 200);
        finalScore.text = _score.ToString();

        yield return new WaitForSeconds(0.5f);
        bonusType.text = $"SCORE";
    }
}
