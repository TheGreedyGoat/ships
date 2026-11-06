using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

class Player : MonoSingleton<Player>
{
    void Start()
    {
        SubscribeActions();
    }

    public static void Clear()
    {
        if (InstanceOrNull != null)
        {
            if (Application.isPlaying)
                Destroy(Instance);
            else
                DestroyImmediate(Instance);
        }
    }

    public Ship ship;

    ShipMovement movement => ship.GetComponent<ShipMovement>();

    Sail sail => ship.gameObject.GetComponentInChildren<Sail>();

    Cannons2 cannons => ship.gameObject.GetComponentInChildren<Cannons2>();

    CameraMovement cam => CameraMovement.Instance;
    bool invertAxisY = true;

    enum InputMode
    {
        OnPerformed,
        WhilePressed,
    }

    enum UpdateMethod
    {
        Default,
        Fixed,
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

    void Bind(
        InputAction action,
        InputMode mode,
        UpdateMethod method,
        Action<InputAction> callback
    )
    {
        action.Enable();

        var binding = new InputBinding
        {
            Action = action,
            Mode = mode,
            method = method,
            Callback = callback,
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
        // movement = GetComponent<ShipMovement>();
        // cannons = GetComponentInChildren<Cannons>();

        var steer = actions.FindAction("Steer");
        Bind(steer, InputMode.WhilePressed, UpdateMethod.Fixed, OnSteer);

        var sails = actions.FindAction("Sails Up/Down");
        Bind(
            sails,
            InputMode.WhilePressed,
            UpdateMethod.Default,
            OnSailsUpDown
        );

        var fire = actions.FindAction("Fire Direct");
        Bind(fire, InputMode.OnPerformed, UpdateMethod.Default, OnFire);

        var aim = actions.FindAction("Aim");
        Bind(aim, InputMode.WhilePressed, UpdateMethod.Default, OnAim);

        var rotateCam = actions.FindAction("Rotate");
        Bind(
            rotateCam,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnMouseDelta
        );

        var zoom = actions.FindAction("Zoom");

        Bind(zoom, InputMode.OnPerformed, UpdateMethod.Default, OnScroll);
    }

    // void Start()
    // {

    //     SubscribeActions();
    // }

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
            if (
                binding.method == UpdateMethod.Default
                && binding.Mode == InputMode.WhilePressed
                && binding.Action.IsPressed()
            )
                binding.Callback(binding.Action);
        }
    }

    void FixedUpdate()
    {
        if (ship == null)
            return;
        foreach (var binding in bindings)
        {
            if (
                binding.method == UpdateMethod.Fixed
                && binding.Mode == InputMode.WhilePressed
                && binding.Action.IsPressed()
            )
                binding.Callback(binding.Action);
        }
    }

    //
    // ######## #### ########  ########
    // ##        ##  ##     ## ##
    // ##        ##  ##     ## ##
    // ######    ##  ########  ######
    // ##        ##  ##   ##   ##
    // ##        ##  ##    ##  ##
    // ##       #### ##     ## ########
    //

    void OnFire(InputAction action)
    {
        if (ship == null)
            return;
        float value = (action.ReadValue<float>() + 1) * 0.5f;
        cannons.FireNextDeck(Mathf.RoundToInt(value));
    }

    void OnAim(InputAction _)
    {
        if (ship == null)
            return;
        cannons.ShowAim();
    }

    //
    // ##     ##  #######  ##     ## ########
    // ###   ### ##     ## ##     ## ##
    // #### #### ##     ## ##     ## ##
    // ## ### ## ##     ## ##     ## ######
    // ##     ## ##     ##  ##   ##  ##
    // ##     ## ##     ##   ## ##   ##
    // ##     ##  #######     ###    ########
    //

    void OnSteer(InputAction action)
    {
        if (ship == null)
            return;
        movement.Steer(action.ReadValue<float>());
    }

    void OnSailsUpDown(InputAction action)
    {
        if (sail == null)
            return;
        sail.SailsUpDown(action.ReadValue<float>());
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

    //
    //  ######     ###    ##     ##
    // ##    ##   ## ##   ###   ###
    // ##        ##   ##  #### ####
    // ##       ##     ## ## ### ##
    // ##       ######### ##     ##
    // ##    ## ##     ## ##     ##
    //  ######  ##     ## ##     ##
    //

    void OnMouseDelta(InputAction action)
    {
        if (ship == null)
            return;
        var value = action.ReadValue<Vector2>();
        value.y *= invertAxisY ? -1 : 1;
        cam.RecieveRotationInput(value);
    }

    void OnScroll(InputAction action)
    {
        if (ship == null)
            return;
        cam.RecieveZoomInput(action.ReadValue<float>());
    }
}
