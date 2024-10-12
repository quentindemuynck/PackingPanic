using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    [SerializeField]
    private float _movementSpeed = 1.0f;

    [SerializeField]
    private float _rotationSpeed = 2.0f;

    // Update is called once per frame
    public void Move(Vector3 direction)
    {
        Vector3 displacemntVector = direction.normalized;
        RotateToDirection(displacemntVector);
        displacemntVector *= _movementSpeed * Time.deltaTime;
        transform.position += displacemntVector;
    }

    private void RotateToDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return; 

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
}
