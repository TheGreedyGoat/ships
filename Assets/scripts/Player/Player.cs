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

    private bool isFirstPerson;

    //
    //  ######   ######## ######## ######## ######## ########   ######
    // ##    ##  ##          ##       ##    ##       ##     ## ##    ##
    // ##        ##          ##       ##    ##       ##     ## ##
    // ##   #### ######      ##       ##    ######   ########   ######
    // ##    ##  ##          ##       ##    ##       ##   ##         ##
    // ##    ##  ##          ##       ##    ##       ##    ##  ##    ##
    //  ######   ########    ##       ##    ######## ##     ##  ######
    //
    public PlayerCannons PlayerCannons => PlayerCannons.Instance;

    public Ship Ship
    {
        get => ship;
        private set => ship = value;
    }

    public ShipMovement Movement => _movement;
    public Sail Sail => _sail;

    public CameraMovement Cam => _cam;
    public ShipCannons Cannons => _cannons;

    public bool IsAiming => isFirstPerson;

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
        Cam.transform.localPosition = ship.Data.LocalCameraPos;
        PlayerCannons.SetupAimDisplays();
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

        var dFireL = actions.FindAction("DirectFireLeft");
        Bind(
            dFireL,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnDirectFireLeft
        );
        var dFireR = actions.FindAction("DirectFireRight");
        Bind(
            dFireR,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnDirectFireRight
        );

        var mouseMoved = actions.FindAction("Rotate");
        Bind(
            mouseMoved,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnMouseDelta
        );

        var zoom = actions.FindAction("Zoom");

        Bind(zoom, InputMode.OnPerformed, UpdateMethod.Default, OnScroll);

        var firstPerson = actions.FindAction("FirstPerson");
        Bind(
            firstPerson,
            InputMode.OnPerfomedAndReleased,
            UpdateMethod.Default,
            OnFirstPersonAim
        );

        var fpFire = actions.FindAction("FirstPersonFire");

        Bind(fpFire, InputMode.OnPerformed, UpdateMethod.Default, OnFPFire);
    }

    void UnbindAll()
    {
        foreach (var binding in bindings)
        {
            if (binding.PerformedHandler != null)
            {
                binding.Action.performed -= binding.PerformedHandler;
                if (binding.Mode == InputMode.OnPerfomedAndReleased)
                {
                    binding.Action.canceled -= binding.PerformedHandler;
                }
            }

            binding.Action.Disable();
        }

        bindings.Clear();
    }

    void OnDisable()
    {
        UnbindAll();
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

    void OnDirectFireLeft(InputAction action)
    {
        Cannons.Fire(0);
    }

    void OnDirectFireRight(InputAction action)
    {
        Cannons.Fire(1);
    }

    void OnFirstPersonAim(InputAction action)
    {
        isFirstPerson = action.IsPressed();
        if (!isFirstPerson)
        {
            Cannons.ResetCannonRotations();
        }
    }

    void OnFPFire(InputAction _)
    {
        if (isFirstPerson)
            Cannons.Fire(Cam.FacedBroadSide);
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
        int invertMod = GlobalValues.Instance.InvertCameraPitch ? -1 : 1;
        var valueRaw = action.ReadValue<Vector2>();
        Vector2 value;
        value = valueRaw * GlobalValues.Instance.CameraSensitivity;
        value.y *= invertMod;
        Cam.RecieveRotationInput(value);
        if (isFirstPerson)
            Cannons.Aim(
                Cam.FacedBroadSide,
                valueRaw.y
                    * GlobalValues.Instance.AimingSensitivity
                    * -invertMod
            );
    }

    void OnScroll(InputAction action)
    {
        if (Ship == null)
            return;
        Cam.RecieveZoomInput(action.ReadValue<float>());
    }
}
