using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField]
    private MarbleVisualController _marbleVisuals;

    private VisualElement _rootVE;
    private Button _playButton;
    private Button _quitButton;
    private SelectorButton _marbleSelector;


    protected void Awake()
    {
        _rootVE = GetComponent<UIDocument>().rootVisualElement;
        _playButton = _rootVE.Query<Button>("Play");
        _playButton.clicked += OnPlayButtonClicked;

        _quitButton = _rootVE.Query<Button>("Quit");
        _quitButton.clicked += OnQuitButtonClicked;

        _marbleSelector = _rootVE.Query<SelectorButton>("MarbleSelector");
        _marbleSelector.NextPressed += OnNextMarblePressed;
        _marbleSelector.PrevPressed += OnPrevMarblePressed;

        _rootVE.Q<Label>("HighScoreNumber").text = PlayerPrefs.GetInt("HighScore", 0).ToString();
    }

    private void OnPlayButtonClicked()
    {
        Provider.Instance.Resolve<GameStateManager>().StartGame();
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }   

    private void OnNextMarblePressed()
    {
        _marbleVisuals.ChangeToNext();
    }

    private void OnPrevMarblePressed()
    {
        _marbleVisuals.ChangeToPrev();
    }
}
