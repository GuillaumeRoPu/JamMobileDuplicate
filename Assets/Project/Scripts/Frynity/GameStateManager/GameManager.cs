using Frynity.Saves.Binary;
using GooglePlayGames;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject _player;
    [SerializeField] GameObject _zombie;
    [SerializeField] GameObject _echo;
    [SerializeField] GameObject _witch;

    List<Vector2Int> _playerMovement = new List<Vector2Int>();
    public List<Vector2Int> PlayerMovement => _playerMovement;
    public void AddMovement(Vector2Int playerMovement) {  PlayerMovement.Add(playerMovement); }

    Entity _playerEntity;
    List<Entity> _zombieEntity = new List<Entity>();
    Entity _echoEntity = null;
    List<Entity> _witchEntity = new List<Entity>();

    List<Entity> _deadEntity = new List<Entity>();
    public List<Entity> Deads => _deadEntity;

    public Entity Player => _playerEntity;
    public List<Entity> Zombies => _zombieEntity;
    public Entity Echo => _echoEntity;
    public List<Entity> Witches => _witchEntity;

    int _currentLevel;
    public void SetCurrentLevel(int level) { _currentLevel = level; } 

    [Header("Game Scene")]
    [SerializeField] GameObject _menu;
    [SerializeField] GameObject _game;
    [SerializeField] GameObject _pause;
    [SerializeField] GameObject _settings;
    public void ShowMenu(bool show) { _menu.SetActive(show); }
    public void ShowGame(bool show) { _game.SetActive(show); }
    public void ShowPause(bool show) { _pause.SetActive(show); }
    public void ShowSettings(bool show) { _settings.SetActive(show); }

    [Header("Object With Z Order")]
    [SerializeField] GameObject _rooms;
    public Transform RoomTransform => _rooms.transform;
    [SerializeField] List<GameObject> _other = new List<GameObject>();
    public void AddObjectRespectZOrder(GameObject obj) { _other.Add(obj); }

    [Header("Room Design")]
    [SerializeField] Sprite _sideInWallSprite;
    [SerializeField] Sprite _sideInDoorWallSprite;
    [SerializeField] Sprite _sideOutWallSprite;
    [SerializeField] Sprite[] _outWallSprite;
    [SerializeField] Sprite[] _outDoorWallSprite;
    public Sprite SideInWall => _sideInWallSprite;
    public Sprite SideInDoorWall => _sideInDoorWallSprite;
    public Sprite SideOutWall => _sideOutWallSprite;
    public Sprite[] OutWall => _outWallSprite;
    public Sprite[] OutDoorWall => _outDoorWallSprite;

    [Header("Sound Data")]
    [SerializeField] SoundData _zombieSounds;
    public SoundData ZombieAudio => _zombieSounds;

    [Header("Animation")]
    [SerializeField] Animator _doorAnim;

    [Header("Manager")]
    [SerializeField] TurnManager _turnManager;
    [SerializeField] GridManager _gridManager;

    [SerializeField, Foldout("Events")] UnityEvent _whenGameOver;
    public TurnManager Turn => _turnManager;
    public GridManager Grid => _gridManager;

    public GameStateMachine stateMachine { get; private set; }
    public IntroState introState {get; private set;}
    public MenuState menuState {get; private set;}
    public PlayState playState {get; private set;}
    public CinematicState cinematicState {get; private set;}
    public PauseState pauseState {get; private set;}

    private static GameManager _instance;
    public static GameManager Of { get { return _instance; }}

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);


        InitializeStates();
    }

    private void Start()
    {

        stateMachine.InitState(menuState);
    }

    private void Update()
    {
        stateMachine.currentState.PhysicsUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.currentState.LogicsUpdate();
    }

    private void InitializeStates()
    {
        stateMachine = new GameStateMachine();
        introState = new IntroState();
        menuState = new MenuState();
        playState = new PlayState();
        cinematicState = new CinematicState();
        pauseState = new PauseState();
    }

    public void CheckEntityAlive()
    {
        for (int i = 0; i < _zombieEntity.Count; i++)
        {
            if (_zombieEntity[i] == null)
            {
                PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_humans_are_superior, 100f, null);
                _zombieEntity.RemoveAt(i);
                i--;
                continue;
            }
        }

        for (int i = 0; i < _witchEntity.Count; i++)
        {
            if (_witchEntity[i] == null)
            {
                PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_at_the_stake, 100f, null);
                _witchEntity.RemoveAt(i);
                i--;
                continue;
            }
        }

        _deadEntity.Clear();
    }

    public void CreatePlayer(Vector2Int tile)
    {
        GameObject player = Instantiate(_player, (Vector2)tile * Grid.TileSize, Quaternion.identity);
        _playerEntity = player.GetComponent<Entity>();
        _playerEntity.SetCurrentTile(tile);
        Grid.GetTileAtPosition(tile).CellData.Entity = _playerEntity;
    }

    public void SpawnZombie(Vector2Int spawnPos)
    {
        GameObject zombie = Instantiate(_zombie, (Vector2)spawnPos * Grid.TileSize, Quaternion.identity);
        _zombieEntity.Add(zombie.GetComponent<Entity>());
        _zombieEntity[^1].SetTurnBeforeMove(2);
        _zombieEntity[^1].SetCurrentTile(spawnPos);
        Grid.GetTileAtPosition(spawnPos).CellData.Entity = _zombieEntity[^1];
    }

    public void SpawnEcho(Vector2Int playerPos)
    {
        if (_echoEntity)
        {
            Debug.LogError("Echo d�j� cr�e");
            return;
        }
        Vector2Int pos = playerPos + Vector2Int.left;
        if (_playerMovement.Count > 0) pos = _playerMovement[^1];

        GameObject echo = Instantiate(_echo, (Vector2)pos * Grid.TileSize, Quaternion.identity);
        _echoEntity = echo.GetComponent<Entity>();
        _echoEntity.SetCurrentTile(pos);
        Grid.GetTileAtPosition(pos).CellData.Entity = _echoEntity;
    }

    public void DestroyEcho()
    {
        Destroy(_echoEntity.gameObject);
        _echoEntity = null;
    }

    public void SpawnWitch(Vector2Int spawnPos)
    {
        GameObject witch = Instantiate(_witch, (Vector2)spawnPos * Grid.TileSize, Quaternion.identity);
        _witchEntity.Add(witch.GetComponent<Entity>());
        _witchEntity[^1].SetCurrentTile(spawnPos);
        Grid.GetTileAtPosition(spawnPos).CellData.Entity = _witchEntity[^1];
    }

    public void UpdateZOrder(bool changeStatic = true)
    {
        if (changeStatic)
        {
            foreach (SpriteRenderer renderer in _rooms.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingOrder = (int)renderer.transform.position.y * -5;
                //if (renderer.gameObject.name == "Wall") renderer.sortingOrder += 0;
            }

            for (int i = 0; i < _other.Count; i++)
            {
                if (_other[i] == null)
                {
                    _other.RemoveAt(i);
                    i--;
                    continue;
                }

                SpriteRenderer renderer = _other[i].GetComponentInChildren<SpriteRenderer>();
                renderer.sortingOrder = (int)renderer.transform.position.y * -5 - 3;
            }
        }

        if (_playerEntity)
        {
            _playerEntity.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder =
            (int)((Vector2)_playerEntity.CurrentTile.CellData.GridPosition * Grid.TileSize).y * -5 - 2;
        }

        foreach (var zombie in _zombieEntity)
        {
            zombie.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder =
            (int)((Vector2)zombie.CurrentTile.CellData.GridPosition * Grid.TileSize).y * -5 - 1;
        }

        foreach (var witch in _witchEntity)
        {
            witch.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder =
            (int)((Vector2)witch.CurrentTile.CellData.GridPosition * Grid.TileSize).y * -5 - 1;
        }

        if (_echoEntity)
        {
            _echoEntity.gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder =
            (int)((Vector2)_echoEntity.CurrentTile.CellData.GridPosition * Grid.TileSize).y * -5 - 1;
        }
    }

    public void KillAnEntity(Entity entity)
    {
        if (entity == null) return;

        Destroy(entity.gameObject);
        _deadEntity.Add(entity);
    }


    public void SetPause(bool pause)
    {
        stateMachine.ChangeState(pause ? pauseState : playState);
    }

    public void QuitToHome()
    {
        EndLevel();
        _whenGameOver?.Invoke();
        stateMachine.ChangeState(menuState);
        _doorAnim.SetTrigger("Open");
    }
    public void RetryLevel()
    {
        EndLevel();
        stateMachine.ChangeState(playState);
    }

    public async void WinLevel()
    {
        await Awaitable.WaitForSecondsAsync(4);

        if (_currentLevel == SaveManager.Of.Level.LevelUnlock)
        {
            SaveManager.Of.Level.LevelUnlock++;
            SaveManager.Of.SaveGame();
        }
        QuitToHome();
    }

    public async void LoseLevel()
    {
        await Awaitable.WaitForSecondsAsync(4);

        QuitToHome();
    }

    public void EndLevel()
    {
        Turn.ResetFeedback();

        for (int i = 0; i < _zombieEntity.Count; i++)
        {
            if (_zombieEntity[i])
                Destroy(_zombieEntity[i].gameObject);
        }
        _zombieEntity.Clear();

        for (int i = 0; i < _witchEntity.Count; i++)
        {
            if (_witchEntity[i])
                Destroy(_witchEntity[i].gameObject);
        }
        _witchEntity.Clear();

        if (_echoEntity)
            Destroy(_echoEntity.gameObject);

        Destroy(_playerEntity.gameObject);

        Grid.EraseAll();
    }
}