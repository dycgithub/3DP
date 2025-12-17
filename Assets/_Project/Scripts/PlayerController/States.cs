
using UnityEngine;

public class GroundedState : IState {
    readonly PlayerControllerAdvanced controller;
    
    public GroundedState(PlayerControllerAdvanced controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnGroundContactRegained();
    }

    public void Update()
    {
        //
    }

    public void FixedUpdate()
    {
        //
    }

    public void OnExit()
    {
        //
    }
}

public class FallingState : IState {
    readonly PlayerControllerAdvanced controller;

    public FallingState(PlayerControllerAdvanced controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnFallStart();
    }

    public void Update()
    {
        //
    }

    public void FixedUpdate()
    {
        //
    }

    public void OnExit()
    {
        //
    }
}

public class SlidingState : IState {
    readonly PlayerControllerAdvanced controller;

    public SlidingState(PlayerControllerAdvanced controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnGroundContactLost();
    }

    public void Update()
    {
        //
    }

    public void FixedUpdate()
    {
        //
    }

    public void OnExit()
    {
        //
    }
}

public class RisingState : IState {
    readonly PlayerControllerAdvanced controller;

    public RisingState(PlayerControllerAdvanced controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnGroundContactLost();
    }

    public void Update()
    {
        //
    }

    public void FixedUpdate()
    {
        //
    }

    public void OnExit()
    {
        //
    }
}

public class JumpingState : IState {
    readonly PlayerControllerAdvanced controller;

    public JumpingState(PlayerControllerAdvanced controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnGroundContactLost();
        controller.OnJumpStart();
    }

    public void Update()
    {
        //
    }

    public void FixedUpdate()
    {
        //
    }

    public void OnExit()
    {
        //
    }
}

public class FightState:IState {
    
    readonly PlayerControllerAdvanced controller;
    readonly PlayerAttacker attacker;

    public FightState(PlayerControllerAdvanced controller, PlayerAttacker attacker)
    {
        this.controller = controller;
        this.attacker = attacker;
    }
    
    public void OnEnter()
    {

    }

    public void Update()
    {
       
    }

    public void FixedUpdate()
    {
        
    }

    public void OnExit()
    {

    }
}