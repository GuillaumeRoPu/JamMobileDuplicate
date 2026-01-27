using UnityEngine;
using UnityEngine.InputSystem;

public class GridControlTemp : MonoBehaviour
{
    [SerializeField] InputActionReference _pressPositionInput;
    [SerializeField] InputActionReference _pressInput;
    [SerializeField] InputActionReference _tapInput;

    IHoverable _hoverObject;
    Vector2 _pressPosition;
    bool _isPressed;

    void OnEnable()
    {
        _pressPositionInput.action.performed += OnMoveTouch;
        _pressInput.action.started += OnPress;
        _pressInput.action.canceled += OnPress;
        _tapInput.action.started += OnTap;
    }

    void OnDisable()
    {
        _pressPositionInput.action.performed -= OnMoveTouch;
        _pressInput.action.started -= OnPress;
        _pressInput.action.canceled -= OnPress;
        _tapInput.action.started -= OnTap;
    }

    void Update()
    {
        if (_isPressed)
        {
            RaycastHit2D hit = Physics2D.Raycast(_pressPosition, Vector2.zero);
            if (hit)
            {
                IHoverable hover = hit.collider.GetComponent<IHoverable>();
                if (hover != null)
                {
                    if (_hoverObject != hover)
                    {
                        hover.OnHover();
                        if (_hoverObject != null)
                        {
                            _hoverObject.StopHover();
                        }
                        _hoverObject = hover;
                    }
                }
                else
                {
                    if (_hoverObject != null)
                    {
                        _hoverObject.StopHover();
                        _hoverObject = null;
                    }
                }
            }
            else
            {
                if (_hoverObject != null)
                {
                    _hoverObject.StopHover();
                    _hoverObject = null;
                }
            }
        }
    }

    void OnMoveTouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _pressPosition = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
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
            if(_hoverObject != null)
            {
                _hoverObject.StopHover();
            }
            _hoverObject = null;
            _isPressed = false;
        }
    }

    void OnTap(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Debug.Log("TEST");
        }
    }
}
