using UnityEngine;
using KorroAI.Architecture;

namespace KorroAI.LevelSystem
{
    public class Coin : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.5f;

        [Header("Visual")]
        [SerializeField] private Transform visualModel;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO coinCollectedChannel;

        private Vector3 startVisualLocalPosition;

        private void Start()
        {
            if (visualModel == null)
            {
                if (transform.childCount > 0)
                {
                    visualModel = transform.GetChild(0);
                }
                else
                {
                    Debug.LogError($"Coin on {gameObject.name} has no visualModel assigned and no child objects found!");
                    enabled = false;
                    return;
                }
            }

            startVisualLocalPosition = visualModel.localPosition;
        }

        private void Update()
        {
            if (visualModel == null) return;

            visualModel.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            
            float newY = startVisualLocalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            visualModel.localPosition = new Vector3(startVisualLocalPosition.x, newY, startVisualLocalPosition.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (coinCollectedChannel != null)
                {
                    coinCollectedChannel.RaiseEvent();
                }
                Destroy(gameObject);
            }
        }
    }
}