using UnityEngine;

public class PlayState : GameState
{
    bool playSound;
    public override void Enter()
    {
        AudioManager.Of.PlayClipAt(AudioManager.Of.music[0], Vector3.zero);
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

        AudioManager.Of.PlayClipAt(AudioManager.Of.fx[0], Vector3.zero, true);

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