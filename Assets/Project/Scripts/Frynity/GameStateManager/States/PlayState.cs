using Frynity.Gen;
using UnityEngine;

public class PlayState : GameState
{
    bool playSound;
    AudioSource music;

    public override void Enter()
    {
        Camera.main.orthographicSize = 3.5f;

        GameManager.Of.ShowPause(false);
        GameManager.Of.ShowGame(true);
        GameManager.Of.ShowMenu(false);

        if (GameManager.Of.Player != null) return;

        GameManager.Of.Grid.Init();

        foreach (RoomCells room in GameObject.FindObjectsByType<RoomCells>(FindObjectsSortMode.None))
        {
            room.CreateWalls();
        }

        //music = AudioManager.Of.PlayClipAt(AudioManager.Of.music[1], Vector3.zero);

        foreach (Vector2Int posTile in GameManager.Of.Grid.GridLevelData.allTilesPosition)
        {
            CellData.SpawnPointType spawn = CellData.SpawnPointType.None;
            GameManager.Of.Grid.GridLevelData.spawnPointsTiles.TryGetValue(posTile, out spawn);

            if (spawn.HasFlag(CellData.SpawnPointType.Player))
            {
                GameManager.Of.CreatePlayer(posTile);
                break;
            }
        }

        GameManager.Of.UpdateZOrder();

        Camera.main.transform.position = new Vector3(GameManager.Of.RoomTransform.position.x + GameManager.Of.Grid.TileSize/2, 
            GameManager.Of.RoomTransform.position.y + GameManager.Of.Grid.TileSize / 2, -10);


        GameManager.Of.Grid.EntityInARoom();


        GameManager.Of.Turn.Init();
        playSound = true;
        PlayRandomAmbiantSound();
    }

    public override void LogicsUpdate()
    {
        // Logique du jeu
    }

    public override void PhysicsUpdate()
    {
        // Physique du jeu
    }

    public override void Exit()
    {
        playSound = false;
        //GameObject.Destroy(music.gameObject);
    }

    async void PlayRandomAmbiantSound()
    {
        await Awaitable.WaitForSecondsAsync(Random.Range(10, 20));
        if (playSound)
        {
            for (int i = 0; i < GameManager.Of.Zombies.Count; i++)
            {
                AudioManager.Of.PlayDataAudio(GameManager.Of.ZombieAudio);
                await Awaitable.WaitForSecondsAsync(3f);

            }
            PlayRandomAmbiantSound();
        }
    }
}