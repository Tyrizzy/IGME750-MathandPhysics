using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float Mass;
    [SerializeField] private float objectSpeed;
    [SerializeField] private float rotSpeed;

    [SerializeField] private float friction;
    [SerializeField] private float drag;
    [SerializeField] private float angularDrag;

    private Vector3 objectMove;
    private Vector3 objectRot;
    private Vector3 velocity;

    private void Start()
    {
        Mass = gameObject.GetComponent<OBB>().objectMass;
    }

    private void FixedUpdate()
    {
        // Calculate momentum based on mass and speed
        Vector3 momentum = objectMove * Mass;

        // Apply force to velocity
        velocity += momentum * Time.deltaTime / Mass;

        // Apply drag to velocity
        velocity -= velocity * drag * Time.deltaTime;

        // Apply friction
        Vector3 fric = -velocity * friction * Mass;
        velocity += fric * Time.deltaTime;

        // Apply physics
        transform.position += velocity * objectSpeed * Time.deltaTime;

        // Calculate rotation using quaternion
        Quaternion deltaRotation = Quaternion.Euler(objectRot * rotSpeed * Time.deltaTime);
        transform.rotation *= deltaRotation;

        // Apply angular drag to rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, angularDrag * Time.deltaTime);
    }

    // WE CAN FIX CHANGE MOVEMENT, VERY SLIDY LIKE WE ARE DRIFTING
    private void Update()
    {
        // Input handling for movement and rotation
        objectMove = transform.forward * Input.GetAxisRaw("Vertical");
        objectRot = new Vector3(0, Input.GetAxisRaw("Horizontal"), 0);
    }

}
