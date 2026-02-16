using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [SerializeField] InputActionReference _pressPositionInput;
    [SerializeField] InputActionReference _pressInput;
    [SerializeField] InputActionReference _tapInput;
    [SerializeField] InputActionReference _dragInput;

    [SerializeField, Foldout("Events")] UnityEvent<Vector2Int> _onPress;
    [SerializeField, Foldout("Events")] UnityEvent _onPressCanceled;
    [SerializeField, Foldout("Events")] UnityEvent _onTap;
    [SerializeField, Foldout("Events")] UnityEvent _onDrag;

    GameObject _grabObject;
    Vector2 _grabLocalPress;
    Vector2 _pressPosition;
    bool _isPressed;
    bool _posChange;

    void OnEnable()
    {
        _pressPositionInput.action.performed += OnMoveTouch;
        _pressInput.action.started += OnPress;
        _pressInput.action.canceled += OnPress;
        _tapInput.action.started += OnTap;
        _dragInput.action.performed += OnDrag;
    }

    void OnDisable()
    {
        _pressPositionInput.action.performed -= OnMoveTouch;
        _pressInput.action.started -= OnPress;
        _pressInput.action.canceled -= OnPress;
        _tapInput.action.started -= OnTap;
        _dragInput.action.performed -= OnDrag;
    }

    void Update()
    {
        if (_isPressed && _posChange)
        {
            _onPress?.Invoke(GameManager.Of.Grid.WorldPositionToTilePosition(_pressPosition));

            _posChange = false;
        }

        if ( _grabObject != null )
        {
            _grabObject.transform.position = _pressPosition + _grabLocalPress;
        }
    }

    void OnMoveTouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _pressPosition = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
            _posChange = true;
        }
    }

    void OnPress(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _isPressed = true;
            Debug.Log("TOUCH");
        }

        if (ctx.canceled)
        {
            _grabObject = null;
            _onPressCanceled?.Invoke();
        }
    }

    void OnTap(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _onTap?.Invoke();
            Debug.Log("TEST");
        }
    }

    void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
        }
    }
}
