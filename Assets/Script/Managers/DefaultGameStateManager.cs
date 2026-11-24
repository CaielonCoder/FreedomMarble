using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DefaultGameStateManager : GameStateManager
{
    private InGameUIController _UIController;
    private AudioManager _audioManager;
    private Goal _goal;

    private void Start()
    {
        _audioManager = Provider.Instance.Resolve<AudioManager>();
    }

    public override void StartGame()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Level1");
    }

    private IEnumerator UpdateTimer()
    {
        while (State == LevelState.Playing)
        {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0)
            {
                TimeLeft = 0;
                _audioManager.StopMusic();
                _audioManager.PlaySFX("TimeOver");
                State = LevelState.TimeOver;
                break;
            }
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        InitLevel();
    }

    public override void InitLevel()
    {
        TimeLeft = 60;
        State = LevelState.Intro;
        _audioManager.PlayMusic("GameplayMusic");
        _UIController = Provider.Instance.Resolve<InGameUIController>();
        _UIController.AnimationFinish += OnUIControllerAnimationFinish;
        _goal = Provider.Instance.Resolve<Goal>();
    }

    private void OnUIControllerAnimationFinish()
    {
        switch (State)
        {
            case LevelState.Intro:
                State = LevelState.Playing;
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
        _audioManager.PlaySFX("LevelComplete");
        State = LevelState.Outro;
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
