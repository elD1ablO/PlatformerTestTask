using PlayerControl;
using UnityEngine;

namespace LevelComponents
{
    public abstract class BasePlayerTriggerComponent : MonoBehaviour
    {
        [SerializeField]
        private string playerTag = "Player";
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                if (other.TryGetComponent<IPlayerObject>(out IPlayerObject playerObject))
                {
                    OnPlayerEnterAction(playerObject);
                }
            }
        }
        
        protected abstract void OnPlayerEnterAction(IPlayerObject playerObject);
    }
}
