using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    // Add your variables here

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
        stateMachine.InitState(introState);
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
}
