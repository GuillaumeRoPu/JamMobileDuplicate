using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject _player;
    Entity _playerEntity;
    public GameObject Player => _player;

    [Header("Manager")]
    [SerializeField] TurnManager _turnManager;
    [SerializeField] GridManager _gridManager;
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
        GameObject player = Instantiate(Player, (Vector2)tile * Grid.TileSize, Quaternion.identity);
        _playerEntity = player.GetComponent<Entity>();
        _playerEntity.SetCurrentTile(tile);
        Grid.GetTileAtPosition(tile).CellData.Entity = _playerEntity;
    }

    public void SetPlayerDestination(Vector2Int tile)
    {
        Debug.Log(tile);
        if (GridManager.Tiles.ContainsKey(tile))
            _playerEntity.GetComponent<AIPathfinding>().CreatePath(GridManager.Tiles[tile]);
    }
}
