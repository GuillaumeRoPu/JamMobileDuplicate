using System.Security.Cryptography;
using UnityEngine;

public class MenuState : GameState
{

    public override void Enter()
    {
        Camera.main.orthographicSize = 5;
        Camera.main.transform.position = new Vector3(0, 0, -10);

        if (GameManager.Of.Music)
        {
            GameObject.Destroy(GameManager.Of.Music);
        }
        GameManager.Of.Music = AudioManager.Of.PlayClipAt(AudioManager.Of.music[0], Vector3.zero);

        GameManager.Of.ShowMenu(true);
        GameManager.Of.ShowSettings(false);
        GameManager.Of.ShowPause(false);
        GameManager.Of.ShowGame(false);
    }

    public override void LogicsUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void Exit()
    {
        GameObject.Destroy(GameManager.Of.Music.gameObject);
        GameManager.Of.Music = null;
    }
}