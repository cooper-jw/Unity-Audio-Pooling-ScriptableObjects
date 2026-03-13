using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
	[CreateAssetMenu(fileName = "SoundDataSettings", menuName = "Audio/Sound Data Settings")]
	public class SoundDataSettings : ScriptableObject
	{
		public AudioMixerGroup mixerGroup;
		public bool loop;

		public bool mute;
		public bool bypassEffects;
		public bool bypassListenerEffects;
		public bool bypassReverbZones;

		public int priority = 128;
		public float panStereo;
		public float spatialBlend;
		public float reverbZoneMix = 1f;
		public float dopplerLevel = 1f;
		public float spread;

		public float minDistance = 1f;
		public float maxDistance = 500f;

		public bool ignoreListenerVolume;
		public bool ignoreListenerPause;

		public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
		
		public AnimationCurve customRolloffCurve;
		public AnimationCurve spatialBlendCurve;
		public AnimationCurve spreadCurve;
		public AnimationCurve reverbZoneMixCurve;
	}
}