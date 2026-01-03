using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using KorroAI.Architecture;

namespace KorroAI.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("--- Mixer ---")]
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("--- Music Playlist ---")]
        [SerializeField] private SoundDataSO[] musicPlaylist; // הרשימה שלך
        [SerializeField] private float crossfadeDuration = 2.0f; // זמן המעבר בשניות
        
        [Header("--- SFX Events ---")]
        [SerializeField] private VoidEventChannelSO jumpEvent;
        [SerializeField] private VoidEventChannelSO coinEvent;
        [SerializeField] private VoidEventChannelSO dieEvent;
        [SerializeField] private VoidEventChannelSO levelCompletedEvent;
        [SerializeField] private VoidEventChannelSO pauseEvent;
        [SerializeField] private VoidEventChannelSO playerDiedChannel;
        [SerializeField] private IntEventChannelSO healthChangedEvent;
        [SerializeField] private VoidEventChannelSO keyCollectedEvent;

        [Header("--- SFX Data ---")]
        [SerializeField] private SoundDataSO jumpSound;
        [SerializeField] private SoundDataSO coinSound;
        [SerializeField] private SoundDataSO dieSound;
        [SerializeField] private SoundDataSO winSound;
        [SerializeField] private SoundDataSO hitSound;
        [SerializeField] private SoundDataSO keySound;

        [Header("--- Pooling Settings ---")]
        [SerializeField] private SoundEmitter emitterPrefab;
        [SerializeField] private int initialPoolSize = 10;

        // --- משתנים לניהול מוזיקה כפולה (בשביל Crossfade) ---
        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private bool isSourceAPlaying = false; // מי המקור הפעיל כרגע?
        private int currentTrackIndex = 0;
        private Coroutine musicCoroutine;
        private int currentHealth = 3;

        // --- משתנים ל-SFX ---
        private Queue<SoundEmitter> pool = new Queue<SoundEmitter>();

        private void Awake()
        {
            // 1. Singleton - נשאר בחיים במעבר סצנות
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // יצירה דינמית של מקורות המוזיקה (כדי שלא תצטרך ליצור ידנית)
                musicSourceA = gameObject.AddComponent<AudioSource>();
                musicSourceB = gameObject.AddComponent<AudioSource>();
                ConfigureMusicSource(musicSourceA);
                ConfigureMusicSource(musicSourceB);

                InitializePool();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // מתחילים לנגן רק אם יש שירים ברשימה
            if (musicPlaylist.Length > 0)
            {
                // התחלה משיר אקראי
                currentTrackIndex = Random.Range(0, musicPlaylist.Length);
                PlayMusicPlaylist();
            }
        }

        private void ConfigureMusicSource(AudioSource source)
        {
            source.loop = false; // אנחנו מנהלים את הלופ ידנית
            source.spatialBlend = 0f; // 2D
            source.playOnAwake = false;
            if (musicGroup != null) source.outputAudioMixerGroup = musicGroup;
        }

        // --- לוגיקת הפלייליסט וה-Crossfade ---

        private void PlayMusicPlaylist()
        {
            if (musicCoroutine != null) StopCoroutine(musicCoroutine);
            musicCoroutine = StartCoroutine(PlaylistRoutine());
        }

        private IEnumerator PlaylistRoutine()
        {
            // לולאה אינסופית שמנגנת שירים
            while (true)
            {
                SoundDataSO currentSong = musicPlaylist[currentTrackIndex];
                
                // קובעים מי המקור הפנוי ומי המקור שמנגן כרגע
                AudioSource activeSource = isSourceAPlaying ? musicSourceA : musicSourceB;
                AudioSource nextSource = isSourceAPlaying ? musicSourceB : musicSourceA;

                // טעינת השיר למקור הבא
                nextSource.clip = currentSong.GetRandomClip();
                nextSource.volume = 0f; // מתחילים מווליום 0 לפייד-אין
                nextSource.Play();

                // ביצוע ה-Crossfade
                float timer = 0f;
                float startVolume = activeSource.volume; // הווליום הנוכחי של המקור שמסיים
                float targetVolume = currentSong.volume; // הווליום הרצוי של השיר החדש

                while (timer < crossfadeDuration)
                {
                    timer += Time.deltaTime;
                    float t = timer / crossfadeDuration;

                    // Fade In לחדש
                    nextSource.volume = Mathf.Lerp(0f, targetVolume, t);
                    
                    // Fade Out לישן (אם הוא מנגן)
                    if (activeSource.isPlaying)
                    {
                        activeSource.volume = Mathf.Lerp(startVolume, 0f, t);
                    }

                    yield return null;
                }

                // סיום המעבר
                nextSource.volume = targetVolume;
                activeSource.Stop();
                activeSource.volume = 0f;

                // החלפת תפקידים
                isSourceAPlaying = !isSourceAPlaying;

                // חישוב מתי השיר הזה נגמר (כדי להתחיל את הפייד הבא לפני הסוף)
                // מחכים את אורך השיר פחות זמן הפייד
                float waitTime = nextSource.clip.length - crossfadeDuration;
                if (waitTime < 0) waitTime = 0; // הגנה לשירים קצרים מדי

                yield return new WaitForSeconds(waitTime);

                // קידום האינדקס לשיר הבא (בצורה מעגלית)
                currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Length;
            }
        }

        // --- אירועים ו-SFX (נשאר זהה) ---

        private void OnEnable()
        {
            if (jumpEvent) jumpEvent.OnEventRaised += () => PlaySound(jumpSound);
            if (coinEvent) coinEvent.OnEventRaised += () => PlaySound(coinSound);
            if (dieEvent) dieEvent.OnEventRaised += () => PlaySound(dieSound); // לא עוצרים מוזיקה במוות!
            if (levelCompletedEvent) levelCompletedEvent.OnEventRaised += () => PlaySound(winSound);
            if (healthChangedEvent) healthChangedEvent.OnEventRaised += OnHealthChanged;
            if (keyCollectedEvent) keyCollectedEvent.OnEventRaised += () => PlaySound(keySound);
        }

        private void OnDisable()
        {
            if (jumpEvent) jumpEvent.OnEventRaised -= () => PlaySound(jumpSound);
            if (healthChangedEvent) healthChangedEvent.OnEventRaised -= OnHealthChanged;
            if (keyCollectedEvent) keyCollectedEvent.OnEventRaised -= () => PlaySound(keySound);
            // ... הסרת שאר ההרשמות
        }

        private void OnHealthChanged(int newHealth)
        {
            if (newHealth < currentHealth)
            {
                PlaySound(hitSound);
            }
            currentHealth = newHealth;
        }

        // --- לוגיקת ה-Pool (ללא שינוי) ---

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++) CreateNewEmitter();
        }

        private SoundEmitter CreateNewEmitter()
        {
            SoundEmitter newEmitter = Instantiate(emitterPrefab, transform);
            newEmitter.gameObject.SetActive(false);
            pool.Enqueue(newEmitter);
            return newEmitter;
        }

        public void PlaySound(SoundDataSO data)
        {
            if (data == null) return;
            SoundEmitter emitter = GetEmitter();
            if (sfxGroup != null) emitter.SetGroup(sfxGroup);
            emitter.Play(data);
            pool.Enqueue(emitter);
        }

        private SoundEmitter GetEmitter()
        {
            if (pool.Count > 0)
            {
                SoundEmitter e = pool.Dequeue();
                if (!e.gameObject.activeSelf) return e;
            }
            return CreateNewEmitter();
        }
    }
}