using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SelectorButton : VisualElement
{
    [UxmlAttribute]
    public string Text { get => _label.text; set => _label.text = value; }

    public event Action NextPressed;
    public event Action PrevPressed;

    private Label _label;
    private Button _next;
    private Button _prev;

    public SelectorButton()
    {
        _label = new Label("TEXT");
        _label.name = "Text";
        _label.AddToClassList("unity-button");
        _label.style.backgroundColor = new Color(0, 0, 0, 0);
        Add(_label);

        _next = new Button();
        _next.text = ">";
        _next.style.position = Position.Absolute;
        _next.style.left = new Length(50, LengthUnit.Percent);
        _next.style.width = new Length(50, LengthUnit.Percent);
        _next.style.unityTextAlign = TextAnchor.MiddleRight;
        _next.style.backgroundColor = new Color(0, 0, 0, 0);
        Add(_next);

        _prev = new Button();
        _prev.text = "<";
        _prev.style.position = Position.Absolute;
        _prev.style.left = new Length(0, LengthUnit.Percent);
        _prev.style.width = new Length(50, LengthUnit.Percent);
        _prev.style.unityTextAlign = TextAnchor.MiddleLeft;
        _prev.style.backgroundColor = new Color(0, 0, 0, 0);
        Add(_prev);

        _next.clicked += OnNextPressed;
        _prev.clicked += OnPrevPressed;

        RegisterCallback<MouseMoveEvent>(MouseMoveEventHadler);
    }

    private void MouseMoveEventHadler(MouseMoveEvent evt)
    {
        //Debug.Log($"Mouse local pos {evt.localMousePosition}");
    }

    private void OnNextPressed()
    {
        NextPressed?.Invoke();
    }

    private void OnPrevPressed()
    {
        PrevPressed?.Invoke();
    }
}
