using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public enum LevelState
    {
        Intro,
        Playing,
        TimeOver,
        Outro,
    };

    public event Action<LevelState> LevelStateChanged;
    public float TimeLeft { get; private set; }

    private LevelState _state = LevelState.Intro;
    private InGameUIController _UIController;
    private Goal _goal;

    public void StartGame()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Practice");
    }

    private IEnumerator UpdateTimer()
    {
        while (_state == LevelState.Playing)
        {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0)
            {
                TimeLeft = 0;
                _state = LevelState.TimeOver;
                LevelStateChanged?.Invoke(_state);
                break;
            }
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        _state = LevelState.Intro;
        TimeLeft = 60;
        LevelStateChanged?.Invoke(_state);
        _UIController = Provider.Instance.Resolve<InGameUIController>();
        _UIController.AnimationFinish += OnUIControllerAnimationFinish;
        _goal = Provider.Instance.Resolve<Goal>();
    }

    private void OnUIControllerAnimationFinish()
    {
        switch (_state)
        {
            case LevelState.Intro:
                _state = LevelState.Playing;
                LevelStateChanged?.Invoke(_state);
                _goal.GoalReached += OnGoalReached;
                StartCoroutine(UpdateTimer());
                break;
            case LevelState.Outro:
                LoadNextLevel();
                break;
            case LevelState.TimeOver:
                SceneManager.LoadScene("MainMenu");
                break;

        }
    }

    private void OnGoalReached()
    {
        _state = LevelState.Outro;
        LevelStateChanged?.Invoke(_state);
    }

    private void LoadNextLevel()
    { 
        string nextLevelName = "MainMenu";
        switch (SceneManager.GetActiveScene().name)
        {
            case "Practice":
                nextLevelName = "Level1";                
                break;
        }
        SceneManager.LoadScene(nextLevelName);
    }
}
