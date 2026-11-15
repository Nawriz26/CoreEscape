/*
**
** File              : NetworkPlayerController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 14, 2026
** Description       : Reads movement input only for the locally owned
**                     network player and moves the character.
**
*/

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 12.0f;
    [SerializeField] private float gravity = -20.0f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Only the client that owns this player can control it.
        if (!IsSpawned || !IsOwner)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed)
        {
            input.y += 1.0f;
        }

        if (keyboard.sKey.isPressed)
        {
            input.y -= 1.0f;
        }

        if (keyboard.dKey.isPressed)
        {
            input.x += 1.0f;
        }

        if (keyboard.aKey.isPressed)
        {
            input.x -= 1.0f;
        }

        input = Vector2.ClampMagnitude(input, 1.0f);

        Vector3 movementDirection =
            new Vector3(input.x, 0.0f, input.y);

        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(movementDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (characterController.isGrounded &&
            verticalVelocity < 0.0f)
        {
            verticalVelocity = -2.0f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            movementDirection * moveSpeed;

        velocity.y = verticalVelocity;

        characterController.Move(
            velocity * Time.deltaTime
        );
    }
}