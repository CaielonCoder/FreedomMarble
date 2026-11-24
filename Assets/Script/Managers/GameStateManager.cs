using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class GameStateManager : MonoBehaviour
{
    public enum LevelState
    {
        Intro,
        Playing,
        Outro,
        TimeOver,
    }

    public event Action<LevelState> LevelStateChanged;
    public float TimeLeft { get; protected set; }

    private LevelState _state = LevelState.Intro;
    protected LevelState State 
    { 
        get { return _state; }
        set
        {
            if (_state != value)
            {
                _state = value;
                LevelStateChanged?.Invoke(_state);
            }
        }
    }

    public virtual void StartGame()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Level1");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        InitLevel();
    }

    public abstract void InitLevel();
}
