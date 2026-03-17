using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UIDocument HUD;
    [SerializeField]
    private MarbleMovementController marble;

    private VisualElement _rootVE;
    private Label _timeLabel;

    private float _time = 60;

    protected void Start()
    {
        _rootVE = HUD.rootVisualElement;
        _timeLabel = _rootVE.Q<Label>("Time");
    }

    protected void Update()
    {
        _time -= Time.deltaTime;
        _timeLabel.text = _time.ToString("00.");

        
    }
}
