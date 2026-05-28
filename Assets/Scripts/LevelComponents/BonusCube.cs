using System.Collections;
using DamageNumbersPro;
using UnityEngine;

public class BonusCube : MonoBehaviour
{
    [Header("Hit Detection")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _bottomHitTolerance = 0.18f;
    [SerializeField] private float _hitCooldown = 0.25f;

    [Header("Damage Numbers Pro")]
    [SerializeField] private DamageNumber _damageNumberPrefab;
    [SerializeField] private Transform _damageNumberSpawnPoint;
    [SerializeField] private int _bonusValue = 1;

    [Header("Impact")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private float _punchDownDistance = 0.12f;
    [SerializeField] private float _impactDuration = 0.06f;
    [SerializeField] private float _returnDuration = 0.12f;
    [SerializeField] private AnimationCurve _returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Optional FX")]
    [SerializeField] private ParticleSystem _hitParticles;

    private Collider _collider;
    private Vector3 _startLocalPosition;
    private Coroutine _impactCoroutine;
    private float _lastHitTime = -999f;

    private void Awake()
    {
        _collider = GetComponent<Collider>();

        if (_visualRoot == null)
        {
            _visualRoot = transform;
        }

        _startLocalPosition = _visualRoot.localPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleHit(collision.collider, GetLowestContactPoint(collision));
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleHit(other, other.bounds.center);
    }

    private void TryHandleHit(Collider playerCollider, Vector3 hitPoint)
    {
        if (!IsPlayer(playerCollider))
        {
            return;
        }

        if (Time.time < _lastHitTime + _hitCooldown)
        {
            return;
        }

        if (!IsHitFromBottom(playerCollider, hitPoint))
        {
            return;
        }

        _lastHitTime = Time.time;

        SpawnBonusNumber();
        PlayImpact();
    }

    private bool IsPlayer(Collider other)
    {
        bool layerMatches = (_playerLayer.value & (1 << other.gameObject.layer)) != 0;        

        return layerMatches;
    }

    private bool IsHitFromBottom(Collider playerCollider, Vector3 hitPoint)
    {
        if (_collider == null)
        {
            return false;
        }

        float cubeBottomY = _collider.bounds.min.y;
        float playerTopY = playerCollider.bounds.max.y;

        bool playerIsUnderCube = playerCollider.bounds.center.y < _collider.bounds.center.y;
        bool hitNearBottom = hitPoint.y <= cubeBottomY + _bottomHitTolerance;
        bool headReachedBottom = playerTopY >= cubeBottomY - _bottomHitTolerance;

        return playerIsUnderCube && hitNearBottom && headReachedBottom;
    }

    private Vector3 GetLowestContactPoint(Collision collision)
    {
        Vector3 lowestPoint = collision.contacts[0].point;

        for (int i = 1; i < collision.contactCount; i++)
        {
            if (collision.contacts[i].point.y < lowestPoint.y)
            {
                lowestPoint = collision.contacts[i].point;
            }
        }

        return lowestPoint;
    }

    private void SpawnBonusNumber()
    {
        if (_damageNumberPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = _damageNumberSpawnPoint != null
            ? _damageNumberSpawnPoint.position
            : transform.position + Vector3.up * 0.8f;

        _damageNumberPrefab.Spawn(spawnPosition, _bonusValue);
    }
    public void TryHitFromCharacterController(Collider playerCollider)
    {
        Vector3 playerCenter = playerCollider.bounds.center;

        bool playerIsBelow =
            playerCenter.y < _collider.bounds.min.y;

        if (!playerIsBelow)
        {
            return;
        }

        TryHandleHit(playerCollider, playerCollider.bounds.max);
    }

    private void PlayImpact()
    {
        if (_impactCoroutine != null)
        {
            StopCoroutine(_impactCoroutine);
        }

        _impactCoroutine = StartCoroutine(ImpactRoutine());

        if (_hitParticles != null)
        {
            _hitParticles.Play();
        }

    }

    private IEnumerator ImpactRoutine()
    {
        Vector3 downPosition = _startLocalPosition + Vector3.down * _punchDownDistance;

        float timer = 0f;

        while (timer < _impactDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _impactDuration);
            _visualRoot.localPosition = Vector3.Lerp(_startLocalPosition, downPosition, t);
            yield return null;
        }

        timer = 0f;

        while (timer < _returnDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _returnDuration);
            float curveT = _returnCurve.Evaluate(t);
            _visualRoot.localPosition = Vector3.Lerp(downPosition, _startLocalPosition, curveT);
            yield return null;
        }

        _visualRoot.localPosition = _startLocalPosition;
        _impactCoroutine = null;
    }
}