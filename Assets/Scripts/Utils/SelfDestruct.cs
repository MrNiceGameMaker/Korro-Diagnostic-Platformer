using UnityEngine;

namespace KorroAI.Utils
{
    public class SelfDestruct : MonoBehaviour
    {
        [SerializeField] private float lifetime = 2.0f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}