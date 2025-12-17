using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public interface IInputReader {
    Vector2 Direction { get; }
    void EnablePlayerActions();
}

[CreateAssetMenu(fileName = "InputReader", menuName = "3DP/InputReader")]
public class InputRead : ScriptableObject,IPlayerActions, IInputReader
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };
    public event UnityAction EnableMouseControlCamera = delegate { };
    public event UnityAction DisableMouseControlCamera = delegate { };
    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction<bool> Dash = delegate { };
    public event UnityAction Attack = delegate { };
    public event UnityAction<bool> Fire = delegate { };

    PlayerInputActions inputActions;
        
    public bool IsJumpKeyPressed() => inputActions.Player.Jump.IsPressed();
    
    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => inputActions.Player.Look.ReadValue<Vector2>();

    public void EnablePlayerActions() {
        if (inputActions == null) {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Enable();
    }
    
    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks( this);
        }
    }
    
    public void OnMove(InputAction.CallbackContext context) {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context) {
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }
    
    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase) 
        {
            case InputActionPhase.Started:
                Fire.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Fire.Invoke(false);
                break;
        }
    }

    public void OnMouseControlCamera(InputAction.CallbackContext context)
    {
        switch (context.phase) 
        {
            case InputActionPhase.Started:
                EnableMouseControlCamera.Invoke();
                break;
            case InputActionPhase.Canceled:
                DisableMouseControlCamera.Invoke();
                break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        switch (context.phase) 
        {
            case InputActionPhase.Started:
                Dash.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Dash.Invoke(false);
                break;
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) {
            Attack.Invoke();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase) 
        {
            case InputActionPhase.Started:
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Jump.Invoke(false);
                break;
        }
    }
    
    bool IsDeviceMouse(InputAction.CallbackContext context)=>
        context.control.device.layout == "Mouse";

    
    
}
