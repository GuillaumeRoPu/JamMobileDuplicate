using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject _player;
    [SerializeField] GameObject _zombie;
    [SerializeField] GameObject _echo;
    [SerializeField] GameObject _witch;

    Entity _playerEntity;
    List<Entity> _zombieEntity = new List<Entity>();
    Entity _echoEntity = null;
    List<Entity> _witchEntity = new List<Entity>();

    public Entity Player => _playerEntity;
    public List<Entity> Zombies => _zombieEntity;
    public Entity Echo => _echoEntity;
    public List<Entity> Witches => _witchEntity;
    public bool CanPlay => !_playerEntity.IsMoving;

    [Header("Room Design")]
    [SerializeField] Sprite _sideInWallSprite;
    [SerializeField] Sprite _sideOutWallSprite;
    [SerializeField] Sprite[] _outWallSprite;
    public Sprite SideInWall => _sideInWallSprite;
    public Sprite SideOutWall => _sideOutWallSprite;
    public Sprite[] OutWall => _outWallSprite;

    [Header("Sound Data")]
    [SerializeField] SoundData _zombieSounds;
    public SoundData ZombieAudio => _zombieSounds;

    [Header("Manager")]
    [SerializeField] TurnManager _turnManager;
    [SerializeField] GridManager _gridManager;
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

        DontDestroyOnLoad(gameObject);

        InitializeStates();
    }

    private void Start()
    {
        Grid.Init();
        stateMachine.InitState(playState);
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
        _zombieEntity[_zombieEntity.Count - 1].SetCurrentTile(spawnPos);
        Grid.GetTileAtPosition(spawnPos).CellData.Entity = _zombieEntity[_zombieEntity.Count - 1];
    }

    public void SpawnEcho(Vector2Int spawnPos)
    {
        if (_echoEntity)
        {
            Debug.LogError("Echo déjà crée");
            return;
        }
        GameObject echo = Instantiate(_echo, (Vector2)spawnPos * Grid.TileSize, Quaternion.identity);
        _echoEntity = echo.GetComponent<Entity>();
        _echoEntity.SetCurrentTile(spawnPos);
        Grid.GetTileAtPosition(spawnPos).CellData.Entity = _echoEntity;
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
        _witchEntity[_witchEntity.Count - 1].SetCurrentTile(spawnPos);
        Grid.GetTileAtPosition(spawnPos).CellData.Entity = _witchEntity[_witchEntity.Count - 1];
    }
}
