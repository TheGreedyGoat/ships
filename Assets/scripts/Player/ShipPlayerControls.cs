using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ShipMovement))]
public class ShipPlayerControls : MonoSingleton<ShipPlayerControls>
{
    ShipMovement movement;

    Sail sail;


    Cannons cannons;
    [SerializeField] CameraMovement cam;

    [SerializeField] bool invertAxisY = true;
    enum InputMode
    {
        OnPerformed,
        WhilePressed,
    }
    enum UpdateMethod
    {
        Default,
        Fixed
    }

    sealed class InputBinding
    {
        public InputAction Action;
        public InputMode Mode;
        public UpdateMethod method;
        public Action<InputAction> Callback;
        public Action<InputAction.CallbackContext> PerformedHandler;
    }

    readonly List<InputBinding> bindings = new();

    void Bind(InputAction action, InputMode mode, UpdateMethod method, Action<InputAction> callback)
    {
        action.Enable();

        var binding = new InputBinding
        {
            Action = action,
            Mode = mode,
            method = method,
            Callback = callback
        };

        if (mode == InputMode.OnPerformed)
        {
            binding.PerformedHandler = _ => callback(action);
            action.performed += binding.PerformedHandler;
        }

        bindings.Add(binding);
    }

    void SubscribeActions()
    {
        var actions = InputSystem.actions;
        movement = GetComponent<ShipMovement>();
        sail = GetComponentInChildren<Sail>();
        cannons = GetComponentInChildren<Cannons>();

        var steer = actions.FindAction("Steer");
        Bind(steer, InputMode.WhilePressed, UpdateMethod.Fixed, OnSteer);

        var sails = actions.FindAction("Sails Up/Down");
        Bind(sails, InputMode.OnPerformed, UpdateMethod.Default, OnSailsUpDown);

        var fire = actions.FindAction("Fire Direct");
        Bind(fire, InputMode.OnPerformed, UpdateMethod.Default, OnFire);

        var aim = actions.FindAction("Aim");
        Bind(aim, InputMode.WhilePressed, UpdateMethod.Default, OnAim);

        var rotateCam = actions.FindAction("Rotate");
        Bind(rotateCam, InputMode.OnPerformed, UpdateMethod.Default, OnMouseDelta);

        var zoom = actions.FindAction("Zoom");

        Bind(zoom, InputMode.OnPerformed, UpdateMethod.Default, OnScroll);
    }

    void Start()
    {
        SubscribeActions();
    }

    // 
    // ##     ## ########  ########     ###    ######## ########       ## ######## #### ##     ## ######## ########  ##     ## ########  ########     ###    ######## ######## 
    // ##     ## ##     ## ##     ##   ## ##      ##    ##            ##  ##        ##   ##   ##  ##       ##     ## ##     ## ##     ## ##     ##   ## ##      ##    ##       
    // ##     ## ##     ## ##     ##  ##   ##     ##    ##           ##   ##        ##    ## ##   ##       ##     ## ##     ## ##     ## ##     ##  ##   ##     ##    ##       
    // ##     ## ########  ##     ## ##     ##    ##    ######      ##    ######    ##     ###    ######   ##     ## ##     ## ########  ##     ## ##     ##    ##    ######   
    // ##     ## ##        ##     ## #########    ##    ##         ##     ##        ##    ## ##   ##       ##     ## ##     ## ##        ##     ## #########    ##    ##       
    // ##     ## ##        ##     ## ##     ##    ##    ##        ##      ##        ##   ##   ##  ##       ##     ## ##     ## ##        ##     ## ##     ##    ##    ##       
    //  #######  ##        ########  ##     ##    ##    ######## ##       ##       #### ##     ## ######## ########   #######  ##        ########  ##     ##    ##    ######## 
    // 


    void Update()
    {
        foreach (var binding in bindings)
        {
            if (binding.method == UpdateMethod.Default && binding.Mode == InputMode.WhilePressed && binding.Action.IsPressed())
                binding.Callback(binding.Action);
        }
    }
    void FixedUpdate()
    {
        foreach (var binding in bindings)
        {
            if (binding.method == UpdateMethod.Fixed && binding.Mode == InputMode.WhilePressed && binding.Action.IsPressed())
                binding.Callback(binding.Action);
        }
        // if (steerAction.IsPressed()) movement.Steer(steerAction.ReadValue<float>());
        // if (aimAction.IsPressed()) OnAim();
    }
    // 
    //  ######     ###    ##       ##       ########     ###     ######  ##    ##  ######  
    // ##    ##   ## ##   ##       ##       ##     ##   ## ##   ##    ## ##   ##  ##    ## 
    // ##        ##   ##  ##       ##       ##     ##  ##   ##  ##       ##  ##   ##       
    // ##       ##     ## ##       ##       ########  ##     ## ##       #####     ######  
    // ##       ######### ##       ##       ##     ## ######### ##       ##  ##         ## 
    // ##    ## ##     ## ##       ##       ##     ## ##     ## ##    ## ##   ##  ##    ## 
    //  ######  ##     ## ######## ######## ########  ##     ##  ######  ##    ##  ######  
    // 

    void OnFire(InputAction action)
    {
        float value = (action.ReadValue<float>() + 1) * 0.5f;
        cannons.Fire(Mathf.RoundToInt(value));
    }

    void OnAim(InputAction _)
    {
        cannons.ShowAiming();
    }

    void OnSteer(InputAction action)
    {
        movement.Steer(action.ReadValue<float>());
    }
    void OnSailsUpDown(InputAction action)
    {
        sail.sailState += (int)action.ReadValue<float>();
    }




    void UnbindAll()
    {
        foreach (var binding in bindings)
        {
            if (binding.PerformedHandler != null)
                binding.Action.performed -= binding.PerformedHandler;

            binding.Action.Disable();
        }

        bindings.Clear();
    }

    void OnDisable()
    {
        UnbindAll();
    }

    void OnMouseDelta(InputAction action)
    {
        var value = action.ReadValue<Vector2>();
        value.y *= invertAxisY ? -1 : 1;
        cam.RecieveRotationInput(value);

    }

    void OnScroll(InputAction action)
    {
        cam.RecieveZoomInput(action.ReadValue<float>());
    }

}



