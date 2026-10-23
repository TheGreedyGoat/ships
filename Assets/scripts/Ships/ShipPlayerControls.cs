using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ShipMovement))]
public class ShipPlayerControls : MonoBehaviour
{
    ShipMovement movement;
    InputAction steerAction;
    InputAction aimAction;

    Sail sail;


    Cannons cannons;

    private readonly List<(InputAction action, Action<InputAction.CallbackContext> handler)> _subs = new();


    void Start()
    {
        Subscribe();

    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }
    void Subscribe()
    {

        movement = GetComponent<ShipMovement>();
        sail = GetComponentInChildren<Sail>();
        cannons = GetComponentInChildren<Cannons>();

        var actions = InputSystem.actions;

        steerAction = actions.FindAction("Steer");
        steerAction.Enable();
        var sailsDown = actions.FindAction("Sails Down");
        var sailsUp = actions.FindAction("Sails Up");
        sailsDown.Enable();
        sailsUp.Enable();
        sailsUp.performed += sail.AddSail;
        sailsDown.performed += sail.RemoveSail;

        var fire = actions.FindAction("Fire Direct");
        fire.Enable();
        fire.performed += OnFire;

        aimAction = actions.FindAction("Aim");
        aimAction.Enable();
    }
    void Unsubscribe()
    {
        var actions = InputSystem.actions;

        steerAction.Disable();

        var sailsDown = actions.FindAction("Sails Down");
        var sailsUp = actions.FindAction("Sails Up");
        sailsDown.Disable();
        sailsUp.Disable();
        sailsUp.performed -= sail.AddSail;
        sailsDown.performed -= sail.RemoveSail;

        var fire = actions.FindAction("Fire Direct");
        fire.Disable();
        fire.performed -= OnFire;

    }


    void FixedUpdate()
    {

        if (steerAction.IsPressed()) movement.Steer(steerAction.ReadValue<float>());
        if (aimAction.IsPressed()) OnAim();
    }

    void OnFire(InputAction.CallbackContext ctx)
    {
        float value = (ctx.ReadValue<float>() + 1) * 0.5f;
        cannons.Fire(Mathf.RoundToInt(value));
    }

    void OnAim()
    {
        cannons.ShowAiming();
    }

}



