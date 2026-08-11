using System;
using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Score { get; private set; }
    public int ScoreFinishBonus { get; private set; }
    public int ScoreTimeBonus { get; private set; }

    private const int TIME_BONUS_PER_SECOND = 70;

    private Rigidbody _marbleRigidbody;
    private float _accumulatedVelocity = 0;
    private bool _accumulateVelocityActive = false;
    private GameStateManager _gameStateManager;

    private void Start()
    {
        ScoreFinishBonus = 500;
        Score = 0;
        _gameStateManager = Provider.Instance.Resolve<GameStateManager>();
        _gameStateManager.LevelStateChanged += OnLevelStateChanged;
    }

    private void OnLevelStateChanged(GameStateManager.LevelState state)
    {
        switch (state)
        {
            case GameStateManager.LevelState.Intro:
                Score += ScoreFinishBonus + ScoreTimeBonus;
                break;
            case GameStateManager.LevelState.Playing:
                StartCoroutine(UpdateScore());
                break;
            case GameStateManager.LevelState.Outro:
                _accumulateVelocityActive = false;
                ScoreTimeBonus = Mathf.CeilToInt(_gameStateManager.TimeLeft * TIME_BONUS_PER_SECOND);
                break;
            case GameStateManager.LevelState.TimeOver:
                _accumulateVelocityActive = false;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_accumulateVelocityActive)
            _accumulatedVelocity += _marbleRigidbody.linearVelocity.magnitude;
    }

    private IEnumerator UpdateScore()
    {
        _marbleRigidbody = Provider.Instance.Resolve<MarbleMovementController>().GetComponent<Rigidbody>();
        _accumulateVelocityActive = true;
        while (_accumulateVelocityActive)
        {
            Score += Mathf.FloorToInt(_accumulatedVelocity / 10f);
            _accumulatedVelocity = 0;
            yield return new WaitForSeconds(1);
        }
    }
}
