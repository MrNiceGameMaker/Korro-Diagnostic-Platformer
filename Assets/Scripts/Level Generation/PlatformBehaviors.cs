using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace KorroAI.LevelSystem
{
    public abstract class BasePlatform : MonoBehaviour
    {
        protected Renderer platformRenderer;
        protected Collider platformCollider;
        protected Material platformMaterial;
        protected Vector3 initialPosition;
        protected Color initialColor;

        protected virtual void Awake()
        {
            platformRenderer = GetComponent<Renderer>();
            platformCollider = GetComponent<Collider>();
            
            if (platformRenderer != null)
            {
                platformMaterial = platformRenderer.material;
                initialColor = platformMaterial.color;
            }
            initialPosition = transform.position;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize() { }

        public virtual void ResetPlatform()
        {
            transform.position = initialPosition;
            if (platformMaterial != null) platformMaterial.color = initialColor;
            if (platformCollider != null) platformCollider.enabled = true;
        }

        // --- השינוי הגדול: פונקציה שהשחקן קורא לה ---
        public virtual void OnPlayerTouch(PlayerController player) { }
    }

    public class StandardPlatform : BasePlatform
    {
        protected override void Initialize()
        {
            base.Initialize();
            if (platformMaterial != null) platformMaterial.color = Color.white;
        }
    }

    public class MovingPlatform : BasePlatform
    {
        [SerializeField] private Vector2 rangeRange = new Vector2(2.0f, 4.0f);
        [SerializeField] private Vector2 timeOffsetRange = new Vector2(0f, Mathf.PI * 2f);

        private float speed;
        private float range;
        private float randomOffset;
        private Vector3 lastPosition;
        private Vector3 currentVelocity;

        public void InitializeWithSpeed(Vector2 speedRange)
        {
            speed = Random.Range(speedRange.x, speedRange.y);
            range = Random.Range(rangeRange.x, rangeRange.y);
            randomOffset = Random.Range(timeOffsetRange.x, timeOffsetRange.y);
        }

        protected override void Awake()
        {
            base.Awake();
            lastPosition = transform.position;
            currentVelocity = Vector3.zero;
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (platformMaterial != null) platformMaterial.color = Color.red;
        }

        private void Update()
        {
            float xOffset = Mathf.Sin(Time.time * speed + randomOffset) * range;
            Vector3 newPosition = initialPosition;
            newPosition.x = initialPosition.x + xOffset;
            
            currentVelocity = (newPosition - lastPosition) / Time.deltaTime;
            lastPosition = newPosition;
            
            transform.position = newPosition;
        }

        public override void OnPlayerTouch(PlayerController player)
        {
            player.SetPlatformVelocity(currentVelocity);
        }
    }

    public class GhostPlatform : BasePlatform
    {
        [SerializeField] private float fadeDelay = 0.5f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float respawnDelay = 2f;

        private bool isTriggered = false;

        protected override void Initialize()
        {
            base.Initialize();
            if (platformMaterial != null)
            {
                // חשוב: וודא שהחומר ביוניטי מוגדר כ-Transparent ולא Opaque!
                platformMaterial.color = new Color(0, 0, 1, 0.5f);
                initialColor = platformMaterial.color;
            }
        }

        public override void OnPlayerTouch(PlayerController player)
        {
            if (!isTriggered)
            {
                isTriggered = true;
                StartCoroutine(FadeAndRespawnSequence());
            }
        }

        private IEnumerator FadeAndRespawnSequence()
        {
            yield return new WaitForSeconds(fadeDelay);

            float elapsed = 0f;
            Color startColor = platformMaterial != null ? platformMaterial.color : initialColor;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
                if (platformMaterial != null)
                {
                    Color c = startColor;
                    c.a = alpha;
                    platformMaterial.color = c;
                }
                yield return null;
            }

            if (platformCollider != null) platformCollider.enabled = false;

            yield return new WaitForSeconds(respawnDelay);
            ResetPlatform();
            isTriggered = false;
        }
    }

    public class BoostPlatform : BasePlatform
    {
        [SerializeField] private float boostForceMultiplier = 2.5f;

        protected override void Initialize()
        {
            base.Initialize();
            if (platformMaterial != null) platformMaterial.color = Color.green;
        }

        public override void OnPlayerTouch(PlayerController player)
        {
            Debug.Log("BOOST! Impulse Triggered");
            var source = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
            if (source != null)
            {
                source.GenerateImpulseWithForce(0.5f);
            }
            player.ApplyJumpForce(boostForceMultiplier);
        }
    }

    public class IcePlatform : BasePlatform
    {
        [SerializeField] private float iceTraction = 0.2f;

        protected override void Initialize()
        {
            base.Initialize();
            if (platformMaterial != null) platformMaterial.color = Color.cyan;
        }

        public override void OnPlayerTouch(PlayerController player)
        {
            player.SetTraction(iceTraction);
        }
    }
}