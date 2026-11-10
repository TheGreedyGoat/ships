using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEditor.XR;
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

    ShipCannons cannons =>
        ship.gameObject.GetComponentInChildren<ShipCannons>();

    CameraMovement cam => CameraMovement.Instance;
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

        var aimL = actions.FindAction("AimLeft");
        Bind(
            aimL,
            InputMode.OnPerfomedAndReleased,
            UpdateMethod.Default,
            SetAimLeft
        );
        var aimR = actions.FindAction("AimRight");
        Bind(
            aimR,
            InputMode.OnPerfomedAndReleased,
            UpdateMethod.Default,
            SetAimRight
        );

        var mouseMoved = actions.FindAction("Rotate");
        Bind(
            mouseMoved,
            InputMode.OnPerformed,
            UpdateMethod.Default,
            OnMouseDelta
        );

        // var cameraMode = actions.FindAction("CameraMode");
        // Bind(
        //     cameraMode,
        //     InputMode.OnPerfomedAndReleased,
        //     UpdateMethod.Default,
        //     OnToggleCamMode
        // );

        var zoom = actions.FindAction("Zoom");

        Bind(zoom, InputMode.OnPerformed, UpdateMethod.Default, OnScroll);
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
        if (aimLeft)
        {
            cannons.ShowAim(0);
        }
        if (aimRight)
        {
            cannons.ShowAim(1);
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

    bool aimLeft = false,
        aimRight = false;
    bool aiming => aimLeft || aimRight;

    void SetAimLeft(InputAction action)
    {
        aimLeft = action.IsPressed();
        cannons.SetAimingMeshVisibility(0, aimLeft);
    }

    void SetAimRight(InputAction action)
    {
        aimRight = action.IsPressed();
        cannons.SetAimingMeshVisibility(1, aimRight);
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
        if (ship == null)
            return;

        var valueRaw = action.ReadValue<Vector2>();
        Vector2 value;
        // value.y *= invertAxisY ? -1 : 1;
        if (aiming)
        {
            value = valueRaw * GlobalValues.Instance.AimingSensitivity;
            value.y *= GlobalValues.Instance.InvertCannonPitch ? -1 : 1;
            if (aimLeft)
                cannons.Aim(value.y, 0);
            if (aimRight)
                cannons.Aim(value.y, 1);

            // lock pitch for aiming
            valueRaw.y = 0;
        }
        value = valueRaw * GlobalValues.Instance.CameraSensitivity;
        value.y *= GlobalValues.Instance.InvertCameraPitch ? -1 : 1;
        cam.RecieveRotationInput(value);
    }

    void OnScroll(InputAction action)
    {
        if (ship == null)
            return;
        cam.RecieveZoomInput(action.ReadValue<float>());
    }
}
