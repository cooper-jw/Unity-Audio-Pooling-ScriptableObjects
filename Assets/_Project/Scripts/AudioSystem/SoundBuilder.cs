using UnityEngine;

namespace AudioSystem
{
	public class SoundBuilder
	{
		private readonly SoundManager _soundManager;
		private Vector3 _position = Vector3.zero;
		private bool _randomPitch;

		public SoundBuilder(SoundManager soundManager)
		{
			_soundManager = soundManager;
		}

		public SoundBuilder WithPosition(Vector3 position)
		{
			_position = position;
			return this;
		}

		public SoundBuilder WithRandomPitch()
		{
			_randomPitch = true;
			return this;
		}

		public void Play(SoundData soundData)
		{
			if (soundData == null)
			{
				Debug.LogError("SoundData is null");
				return;
			}

			if (soundData.settings == null)
			{
				Debug.LogError($"SoundData {soundData.name} settings is null", soundData);
				return;
			}

			if (!_soundManager.CanPlaySound(soundData)) return;

			SoundEmitter soundEmitter = _soundManager.Get();
			soundEmitter.Initialize(soundData);
			Transform transform = soundEmitter.transform;
			transform.position = _position;
			transform.parent = _soundManager.transform;

			if (_randomPitch) soundEmitter.WithRandomPitch();

			if (soundData.frequentSound) soundEmitter.Node = _soundManager.FrequentSoundEmitters.AddLast(soundEmitter);

			soundEmitter.Play();
		}
	}
}