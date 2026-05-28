using System.Collections;
using UnityEngine;

namespace LevelComponents
{
    public class FallingPlatformComponent : MonoBehaviour
    {
        [SerializeField]
        private FallingPlatformObjectComponent platformObject;

        [SerializeField]
        private float fallDelay = 0.5f;

        [SerializeField]
        private float respawnDelay = 3f;

        private bool isTriggered = false;

        private void OnEnable()
        {
            if (platformObject != null)
            {
                platformObject.OnPlayerEntered += OnPlayerDetected;
            }
        }

        private void OnDisable()
        {
            if (platformObject != null)
            {
                platformObject.OnPlayerEntered -= OnPlayerDetected;
            }
        }

        private void OnPlayerDetected(FallingPlatformObjectComponent platform)
        {
            if (!isTriggered)
            {
                isTriggered = true;
                StartCoroutine(FallSequence());
            }
        }

        private IEnumerator FallSequence()
        {
            yield return new WaitForSeconds(fallDelay);

            platformObject.Fall();

            yield return new WaitForSeconds(respawnDelay);

            platformObject.ResetPosition();
            isTriggered = false;
        }
    }
}
