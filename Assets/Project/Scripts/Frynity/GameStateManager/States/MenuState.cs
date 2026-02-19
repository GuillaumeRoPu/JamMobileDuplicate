using System.Security.Cryptography;
using UnityEngine;

public class MenuState : GameState
{
    AudioSource music;
    public override void Enter()
    {
        Camera.main.orthographicSize = 5;
        Camera.main.transform.position = new Vector3(0, 0, -10);

        music = AudioManager.Of.PlayClipAt(AudioManager.Of.music[0], Vector3.zero);

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
        GameObject.Destroy(music.gameObject);
    }
}