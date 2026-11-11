using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.InputSystem;

class Player : MonoSingleton<Player>
{
    private Ship ship;
    private ShipMovement _movement => Ship.GetComponent<ShipMovement>();
    private Sail _sail => Ship.gameObject.GetComponentInChildren<Sail>();
    private ShipCannons _cannons =>
        Ship.gameObject.GetComponentInChildren<ShipCannons>();
    private CameraMovement _cam => CameraMovement.Instance;

    private bool isAiming;

    //
    //  ######   ######## ######## ######## ######## ########   ######
    // ##    ##  ##          ##       ##    ##       ##     ## ##    ##
    // ##        ##          ##       ##    ##       ##     ## ##
    // ##   #### ######      ##       ##    ######   ########   ######
    // ##    ##  ##          ##       ##    ##       ##   ##         ##
    // ##    ##  ##          ##       ##    ##       ##    ##  ##    ##
    //  ######   ########    ##       ##    ######## ##     ##  ######
    //

    public Ship Ship
    {
        get => ship;
        private set => ship = value;
    }

    public ShipMovement Movement => _movement;
    public Sail Sail => _sail;

    public CameraMovement Cam => _cam;
    public ShipCannons Cannons => _cannons;

    public bool IsAiming => isAiming;

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

    public void SetPlayerShip(Ship ship)
    {
        this.ship = ship;
        Cam.transform.parent = ship.transform;
        Cam.transform.localPosition = ship.data.LocalCameraPos;
    }

    //
    //  ######   #######  ##    ## ######## ########   #######  ##        ######
    // ##    ## ##     ## ###   ##    ##    ##     ## ##     ## ##       ##    ##
    // ##       ##     ## ####  ##    ##    ##     ## ##     ## ##       ##
    // ##       ##     ## ## ## ##    ##    ########  ##     ## ##        ######
    // ##       ##     ## ##  ####    ##    ##   ##   ##     ## ##             ##
    // ##    ## ##     ## ##   ###    ##    ##    ##  ##     ## ##       ##    ##
    //  ######   #######  ##    ##    ##    ##     ##  #######  ########  ######
    //

    bool invertAxisY = true;

    enum InputMode
    {
        OnPerformed,
        WhilePressed,
        OnPerfomedAndReleased,
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
        if (mode == InputMode.OnPerfomedAndReleased)
        {
            binding.PerformedHandler = _ => callback(action);
            action.performed += binding.PerformedHandler;
            action.canceled += binding.PerformedHandler;
        }

        bindings.Add(binding);
    }

    //
    //  ######  ##     ## ########   ######   ######  ########  #### ########  ######## ####  #######  ##    ##  ######
    // ##    ## ##     ## ##     ## ##    ## ##    ## ##     ##  ##  ##     ##    ##     ##  ##     ## ###   ## ##    ##
    // ##       ##     ## ##     ## ##       ##       ##     ##  ##  ##     ##    ##     ##  ##     ## ####  ## ##
    //  ######  ##     ## ########   ######  ##       ########   ##  ########     ##     ##  ##     ## ## ## ##  ######
    //       ## ##     ## ##     ##       ## ##       ##   ##    ##  ##           ##     ##  ##     ## ##  ####       ##
    // ##    ## ##     ## ##     ## ##    ## ##    ## ##    ##   ##  ##           ##     ##  ##     ## ##   ### ##    ##
    //  ######   #######  ########   ######   ######  ##     ## #### ##           ##    ####  #######  ##    ##  ######
    //

    void SubscribeActions()
    {
        var actions = InputSystem.actions;
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

        var mouseMoved = actions.FindAction("Rotate");
        Bind(
            mouseMoved,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnMouseDelta
        );

        var zoom = actions.FindAction("Zoom");

        Bind(zoom, InputMode.OnPerformed, UpdateMethod.Default, OnScroll);

        var aim = actions.FindAction("Aim");
        Bind(aim, InputMode.OnPerfomedAndReleased, UpdateMethod.Default, OnAim);
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
        if (Ship == null)
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
        if (Ship == null)
            return;
        float value = (action.ReadValue<float>() + 1) * 0.5f;
        Cannons.FireNextDeck(Mathf.RoundToInt(value));
    }

    void OnAim(InputAction action)
    {
        isAiming = action.IsPressed();
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
        if (Ship == null)
            return;
        Movement.Steer(action.ReadValue<float>());
    }

    void OnSailsUpDown(InputAction action)
    {
        if (Sail == null)
            return;
        Sail.SailsUpDown(action.ReadValue<float>());
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
    // ##     ##  #######  ##     ##  ######  ########
    // ###   ### ##     ## ##     ## ##    ## ##
    // #### #### ##     ## ##     ## ##       ##
    // ## ### ## ##     ## ##     ##  ######  ######
    // ##     ## ##     ## ##     ##       ## ##
    // ##     ## ##     ## ##     ## ##    ## ##
    // ##     ##  #######   #######   ######  ########
    //

    void OnMouseDelta(InputAction action)
    {
        if (Ship == null)
            return;

        var valueRaw = action.ReadValue<Vector2>();
        Vector2 value;
        value = valueRaw * GlobalValues.Instance.CameraSensitivity;
        value.y *= GlobalValues.Instance.InvertCameraPitch ? -1 : 1;
        Cam.RecieveRotationInput(value);
    }

    void OnScroll(InputAction action)
    {
        if (Ship == null)
            return;
        Cam.RecieveZoomInput(action.ReadValue<float>());
    }
}
