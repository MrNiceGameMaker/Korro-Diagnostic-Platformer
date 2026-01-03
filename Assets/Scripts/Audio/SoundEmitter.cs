using System.Collections;
using UnityEngine;
using UnityEngine.Audio; // <--- חשוב! הוספנו את זה בשביל המיקסר

namespace KorroAI.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        private AudioSource audioSource;
        private Coroutine playingCoroutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // --- הוספנו את הפונקציה הזו שהייתה חסרה ---
        public void SetGroup(AudioMixerGroup group)
        {
            if (audioSource != null)
            {
                audioSource.outputAudioMixerGroup = group;
            }
        }
        // -------------------------------------------

        public void Play(SoundDataSO data)
        {
            if (data == null) return;

            // טעינת ההגדרות מה-Scriptable Object
            audioSource.clip = data.GetRandomClip();
            audioSource.volume = data.volume;
            audioSource.pitch = Random.Range(data.minPitch, data.maxPitch);
            audioSource.spatialBlend = data.spatialBlend;
            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;

            // הפעלה
            gameObject.SetActive(true);
            audioSource.Play();

            // החזרה לבריכה כשהסאונד נגמר
            if (playingCoroutine != null) StopCoroutine(playingCoroutine);
            playingCoroutine = StartCoroutine(WaitForSoundToEnd(audioSource.clip.length));
        }

        public void Stop()
        {
            audioSource.Stop();
            gameObject.SetActive(false); // כיבוי מחזיר אותו ל"בריכה"
        }

        private IEnumerator WaitForSoundToEnd(float duration)
        {
            // המתנה באורך הקליפ
            yield return new WaitForSeconds(duration);
            Stop();
        }
    }
}