using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MarbleVisualController : MonoBehaviour
{
    [SerializeField]
    private GameObject _currentVisuals;
    [SerializeField]
    private MarbleVisualsDB _visualsDB;

    private int _currentVisualIndex = 0;

    private const string VISUAL_INDEX_NAME = "MarbleVisualsIndex";

    private void Awake()
    {
        LoadCurrentVisuals();
    }

    public void ChangeToNext()
    {
        _currentVisualIndex++;
        if (_currentVisualIndex >= _visualsDB.MarbleVisuals.Length) _currentVisualIndex = 0;
        SaveCurrentVisuals();
        LoadCurrentVisuals();
    }

    public void ChangeToPrev()
    {
        _currentVisualIndex--;
        if (_currentVisualIndex < 0) _currentVisualIndex = _visualsDB.MarbleVisuals.Length - 1;
        SaveCurrentVisuals();
        LoadCurrentVisuals();
    }

    private void SaveCurrentVisuals()
    {
        PlayerPrefs.SetInt(VISUAL_INDEX_NAME, _currentVisualIndex);
    }

    private void LoadCurrentVisuals()
    {
        _currentVisualIndex = PlayerPrefs.GetInt(VISUAL_INDEX_NAME, 0);
        AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(_visualsDB.MarbleVisuals[_currentVisualIndex]);
        op.WaitForCompletion();
        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            if (_currentVisuals != null) Destroy(_currentVisuals);
            _currentVisuals = Instantiate(op.Result, transform);
        }
    }
}
