using System.Collections.Generic;
using UnityEngine;

public class LevelBlockerCreator : MonoBehaviour
{
    [SerializeField]
    private LevelData _data;

    [SerializeField]
    private Blocker _blockerPrefab;

    private Transform _blockersRoot;

    private void CreateBlockers()
    {
        if (_blockersRoot == null)
        {
            GameObject go = new GameObject("Blockers");
            go.transform.parent = transform;
            _blockersRoot = go.transform;
        }

        ChunkData chunkData = _data.Chunks[0];
        int i = 0;
        for (; i < chunkData.GetBlockerCount(); i++)
        {
            Blocker blocker;
            if (i < _blockersRoot.childCount)
            {
                blocker = _blockersRoot.GetChild(i).GetComponent<Blocker>();
            }
            else
            {
                blocker = Instantiate(_blockerPrefab);
            }
            blocker.SetData(chunkData.GetBlockerData(i), _data);
            blocker.transform.parent = _blockersRoot;
            blocker.transform.localPosition = Vector3.zero;
        }
    }

#if UNITY_EDITOR
    public void OnDataUpdated()
    {
        CreateBlockers();
    }
#endif
}
