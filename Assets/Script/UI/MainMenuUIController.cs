using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour
{
    private VisualElement rootVE;

    private Button playButton;
    private Button quitButton;

    protected void Awake()
    {
        rootVE = GetComponent<UIDocument>().rootVisualElement;
    }

    protected void OnEnable()
    {
        playButton = rootVE.Query<Button>("Play");
        playButton.clicked += OnPlayButtonClicked;

        quitButton = rootVE.Query<Button>("Quit");
        quitButton.clicked += OnQuitButtonClicked;
        
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Practice");
    }

    private void OnQuitButtonClicked()
    {
        InputActionMap map = InputSystem.actions.FindActionMap("Player");
        if (map.enabled)
            map.Disable();
        else
            map.Enable();
    }   
}
