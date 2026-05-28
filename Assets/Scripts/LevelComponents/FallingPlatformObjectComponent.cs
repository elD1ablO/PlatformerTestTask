using System;
using LevelComponents;
using PlayerControl;
using UnityEngine;

public class FallingPlatformObjectComponent : BasePlayerTriggerComponent
{
    [SerializeField]
    private Rigidbody platformRigidbody;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public event Action<FallingPlatformObjectComponent> OnPlayerEntered;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (platformRigidbody == null)
        {
            platformRigidbody = GetComponent<Rigidbody>();
        }

        if (platformRigidbody != null)
        {
            platformRigidbody.isKinematic = true;
        }
    }

    protected override void OnPlayerEnterAction(IPlayerObject playerObject)
    {
        OnPlayerEntered?.Invoke(this);
    }

    public void Fall()
    {
        if (platformRigidbody != null)
        {
            platformRigidbody.isKinematic = false;
        }
    }

    public void ResetPosition()
    {
        if (platformRigidbody != null)
        {
            platformRigidbody.isKinematic = true;
            platformRigidbody.linearVelocity = Vector3.zero;
            platformRigidbody.angularVelocity = Vector3.zero;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
