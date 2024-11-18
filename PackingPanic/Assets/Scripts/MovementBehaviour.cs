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

    private float _initialMaxSpeed;

    [SerializeField]
    private float _decelerationRate = 1.0f;

    private float _movementMultiplier = 1f;

    private Rigidbody rb;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _initialMaxSpeed = _maxSpeed;
    }

    
    public void Move(Vector3 direction)
    {
        Vector3 displacementVector = direction.normalized;

        displacementVector *= _acceleration * Time.deltaTime * _movementMultiplier;
        rb.velocity += displacementVector;

        
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, _maxSpeed);
    }

    // Called every frame to ensure the player slows down gradually
    private void Update()
    {
        SlowDownOverTime();
        RotateToDirection(rb.velocity.normalized);
    }

    
    private void SlowDownOverTime()
    {
        if (rb.velocity.magnitude > 0)
        {
          
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, _decelerationRate * Time.deltaTime);
        }
    }

    
    public void RotateToDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SetMovementMultiplier(float multiplier)
    {
        _movementMultiplier = multiplier;
        _maxSpeed = _initialMaxSpeed * multiplier;
    }
}
