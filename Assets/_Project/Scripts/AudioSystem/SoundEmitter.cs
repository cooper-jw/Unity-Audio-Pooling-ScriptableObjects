using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Utils.Extensions;
using Random = UnityEngine.Random;

namespace AudioSystem 
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour 
    {
        public SoundData Data { [UsedImplicitly] get; private set; }
        public LinkedListNode<SoundEmitter> Node { get; set; }

        private AudioSource _audioSource;
        private Coroutine _playingCoroutine;

        private void Awake()
        {
            _audioSource = gameObject.GetOrAdd<AudioSource>();
        }

        public void Initialize(SoundData data)
        {
            Data = data;
            
            Debug.Assert(data != null, "Sound emitter data is null.", this);
            Debug.Assert(data.settings != null, data.name + " settings is null.", data);
            
            SoundDataSettings settings = data.settings;
            AudioClip clip = data.GetClip();
            
            Debug.Assert(clip != null, data.name + " clip is null.", this);
            
            _audioSource.clip = data.GetClip();
            _audioSource.volume = data.volume;
            _audioSource.pitch = data.pitch;
            
            _audioSource.playOnAwake = false;
            
            _audioSource.outputAudioMixerGroup = settings.mixerGroup;
            _audioSource.loop = settings.loop;
            
            _audioSource.mute = settings.mute;
            _audioSource.bypassEffects = settings.bypassEffects;
            _audioSource.bypassListenerEffects = settings.bypassListenerEffects;
            _audioSource.bypassReverbZones = settings.bypassReverbZones;

            _audioSource.priority = settings.priority;
            _audioSource.panStereo = settings.panStereo;
            _audioSource.spatialBlend = settings.spatialBlend;
            _audioSource.reverbZoneMix = settings.reverbZoneMix;
            _audioSource.dopplerLevel = settings.dopplerLevel;
            _audioSource.spread = settings.spread;

            _audioSource.minDistance = settings.minDistance;
            _audioSource.maxDistance = settings.maxDistance;

            _audioSource.ignoreListenerVolume = settings.ignoreListenerVolume;
            _audioSource.ignoreListenerPause = settings.ignoreListenerPause;

            _audioSource.rolloffMode = settings.rolloffMode;

            if (settings.rolloffMode != AudioRolloffMode.Custom) 
                return;
            
            if (settings.customRolloffCurve is { length: > 0 })
                _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, settings.customRolloffCurve);

            if (settings.spatialBlendCurve is { length: > 0 })
                _audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, settings.spatialBlendCurve);

            if (settings.reverbZoneMixCurve is { length: > 0 })
                _audioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, settings.reverbZoneMixCurve);

            if (settings.spreadCurve is { length: > 0 })
                _audioSource.SetCustomCurve(AudioSourceCurveType.Spread, settings.spreadCurve);
        }

        public void Play()
        {
            if (_playingCoroutine != null) StopCoroutine(_playingCoroutine);

            _audioSource.Play();
            _playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        private IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            Stop();
        }

        public void Stop()
        {
            if (_playingCoroutine != null)
            {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }

            _audioSource.Stop();
            SoundManager.Instance.ReturnToPool(this);
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            _audioSource.pitch += Random.Range(min, max);
        }
    }
}