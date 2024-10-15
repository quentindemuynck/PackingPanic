using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    [SerializeField]
    private float _acceleration = 3.0f;

    [SerializeField]
    private float _rotationSpeed = 2.0f;

    [SerializeField]
    private float _maxSpeed = 5.0f; 

    [SerializeField]
    private float _decelerationRate = 1.0f; 

    private Rigidbody rb;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called to move the player in a direction
    public void Move(Vector3 direction)
    {
        Vector3 displacementVector = direction.normalized;

        // Apply acceleration and update velocity
        displacementVector *= _acceleration * Time.deltaTime;
        rb.velocity += displacementVector;

        // Clamp the velocity to the maximum speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, _maxSpeed);
    }

    // Called every frame to ensure the player slows down gradually
    private void Update()
    {
        SlowDownOverTime();
        RotateToDirection(rb.velocity.normalized);
    }

    // Gradually reduce the player's velocity over time
    private void SlowDownOverTime()
    {
        if (rb.velocity.magnitude > 0)
        {
            // Reduce velocity by a deceleration rate
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, _decelerationRate * Time.deltaTime);
        }
    }

    // Rotate the player towards the movement direction
    private void RotateToDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
}
