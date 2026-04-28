using System.Collections.Generic;
using UnityEngine;

namespace EscapeFacility
{
    public class AudioManager : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const string AudioResourcesFolder = "Audio";

        private readonly Dictionary<MusicTheme, AudioClip> _musicClips = new Dictionary<MusicTheme, AudioClip>();

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private AudioClip _pickupClip;
        private AudioClip _doorClip;
        private AudioClip _failureClip;

        private MusicTheme _currentTheme = MusicTheme.None;

        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 0.18f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = 0.75f;

            _pickupClip = CreateToneClip("Pickup", 880f, 0.18f, 0.35f, 0.10f);
            _doorClip = CreateChirpClip("DoorOpen", 220f, 520f, 0.35f, 0.20f);
            _failureClip = CreateChirpClip("Failure", 480f, 90f, 0.40f, 0.22f);
        }

        public void PlayMusic(MusicTheme theme)
        {
            if (_currentTheme == theme)
            {
                RefreshMusicState();
                return;
            }

            _currentTheme = theme;

            if (theme == MusicTheme.None)
            {
                _musicSource.Stop();
                _musicSource.clip = null;
                return;
            }

            if (!_musicClips.TryGetValue(theme, out AudioClip musicClip))
            {
                musicClip = LoadCustomMusicClip(theme) ?? CreateMusicClip(theme);
                _musicClips[theme] = musicClip;
            }

            _musicSource.clip = musicClip;
            RefreshMusicState();
        }

        public void RefreshMusicState()
        {
            if (GameManager.Instance == null || !GameManager.Instance.MusicEnabled)
            {
                _musicSource.Stop();
                return;
            }

            if (_musicSource.clip != null && !_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }

        public void PlayPickup()
        {
            _sfxSource.PlayOneShot(_pickupClip, 0.6f);
        }

        public void PlayDoorOpen()
        {
            _sfxSource.PlayOneShot(_doorClip, 0.7f);
        }

        public void PlayFailure()
        {
            _sfxSource.PlayOneShot(_failureClip, 0.7f);
        }

        private static AudioClip CreateMusicClip(MusicTheme theme)
        {
            switch (theme)
            {
                case MusicTheme.Menu:
                    return CreateMelodyClip("MenuTheme", new[] { 261.63f, 329.63f, 392.00f, 329.63f }, 0.4f, 0.12f);
                case MusicTheme.Outdoor:
                    return CreateMelodyClip("OutdoorTheme", new[] { 220.00f, 277.18f, 329.63f, 392.00f, 329.63f, 277.18f }, 0.35f, 0.14f);
                case MusicTheme.Indoor:
                    return CreateMelodyClip("IndoorTheme", new[] { 146.83f, 164.81f, 196.00f, 164.81f, 123.47f, 164.81f }, 0.35f, 0.15f);
                case MusicTheme.EndVictory:
                    return CreateMelodyClip("VictoryTheme", new[] { 392.00f, 523.25f, 659.25f, 783.99f }, 0.28f, 0.13f);
                case MusicTheme.EndDefeat:
                    return CreateMelodyClip("DefeatTheme", new[] { 220.00f, 196.00f, 164.81f, 130.81f }, 0.45f, 0.13f);
                default:
                    return null;
            }
        }

        private static AudioClip LoadCustomMusicClip(MusicTheme theme)
        {
            string resourceName = theme switch
            {
                MusicTheme.Menu => "MenuTheme",
                MusicTheme.Outdoor => "MainTrack",
                MusicTheme.Indoor => "IndoorTheme",
                MusicTheme.EndVictory => "VictoryTheme",
                MusicTheme.EndDefeat => "DefeatTheme",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }

            return Resources.Load<AudioClip>($"{AudioResourcesFolder}/{resourceName}");
        }

        private static AudioClip CreateMelodyClip(string clipName, float[] notes, float stepDuration, float volume)
        {
            int samplesPerStep = Mathf.RoundToInt(SampleRate * stepDuration);
            float[] data = new float[samplesPerStep * notes.Length];

            for (int noteIndex = 0; noteIndex < notes.Length; noteIndex++)
            {
                float note = notes[noteIndex];

                for (int sample = 0; sample < samplesPerStep; sample++)
                {
                    float time = sample / (float)SampleRate;
                    float progress = sample / (float)samplesPerStep;
                    float envelope = Mathf.Clamp01(progress / 0.08f) * Mathf.Clamp01((1f - progress) / 0.15f);

                    float fundamental = Mathf.Sin(2f * Mathf.PI * note * time);
                    float harmonic = 0.35f * Mathf.Sin(2f * Mathf.PI * note * 2f * time);
                    float undertone = 0.15f * Mathf.Sin(2f * Mathf.PI * note * 0.5f * time);

                    data[noteIndex * samplesPerStep + sample] = (fundamental + harmonic + undertone) * envelope * volume;
                }
            }

            AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateToneClip(string clipName, float frequency, float duration, float volume, float attackTime)
        {
            int sampleCount = Mathf.RoundToInt(SampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float progress = i / (float)sampleCount;
                float attack = Mathf.Clamp01(time / attackTime);
                float release = Mathf.Clamp01((1f - progress) / 0.2f);
                float envelope = attack * release;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateChirpClip(string clipName, float startFrequency, float endFrequency, float duration, float volume)
        {
            int sampleCount = Mathf.RoundToInt(SampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float progress = i / (float)sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                float time = i / (float)SampleRate;
                float envelope = Mathf.Clamp01(progress / 0.06f) * Mathf.Clamp01((1f - progress) / 0.25f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
