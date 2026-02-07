using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject _player;
    [SerializeField] GameObject _zombie;
    Entity _playerEntity;
    List<Entity> _zombieEntity = new List<Entity>();
    public Entity Player => _playerEntity;
    public List<Entity> Zombies => _zombieEntity;
    public bool CanPlay => !_playerEntity.IsMoving;

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
}
