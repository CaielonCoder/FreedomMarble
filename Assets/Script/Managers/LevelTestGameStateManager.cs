using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTestGameStateManager : GameStateManager
{
    public override void InitLevel()
    {
        State = LevelState.Playing;
    }
}
