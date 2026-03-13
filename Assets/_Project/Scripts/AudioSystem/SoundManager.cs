using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace AudioSystem
{
	public class SoundManager : PersistentSingleton<SoundManager>
	{
		private IObjectPool<SoundEmitter> _soundEmitterPool;
		private readonly List<SoundEmitter> _activeSoundEmitters = new();
		
		public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();

		[SerializeField]
		private SoundEmitter soundEmitterPrefab;
		[SerializeField]
		private bool collectionCheck = true;
		[SerializeField]
		private int defaultCapacity = 10;
		[SerializeField]
		private int maxPoolSize = 100;
		[SerializeField]
		private int maxSoundInstances = 30;

		private void Start()
		{
			InitializePool();
		}

		public SoundBuilder CreateSoundBuilder()
		{
			return new SoundBuilder(this);
		}

		public bool CanPlaySound(SoundData data)
		{
			if (!data.frequentSound) return true;

			if (FrequentSoundEmitters.Count < maxSoundInstances) return true;

			try
			{
				FrequentSoundEmitters.First.Value.Stop();
				return true;
			}
			catch
			{
				Debug.Log("SoundEmitter is already released");
			}

			return false;
		}

		public SoundEmitter Get()
		{
			return _soundEmitterPool.Get();
		}

		public void ReturnToPool(SoundEmitter soundEmitter)
		{
			_soundEmitterPool.Release(soundEmitter);
		}

		public void StopAll()
		{
			LinkedList<SoundEmitter> tempList = new(_activeSoundEmitters);

			foreach (SoundEmitter soundEmitter in tempList) soundEmitter.Stop();

			FrequentSoundEmitters.Clear();
		}

		private void InitializePool()
		{
			_soundEmitterPool = new ObjectPool<SoundEmitter>(
				CreateSoundEmitter,
				OnTakeFromPool,
				OnReturnedToPool,
				OnDestroyPoolObject,
				collectionCheck,
				defaultCapacity,
				maxPoolSize);
		}

		private SoundEmitter CreateSoundEmitter()
		{
			SoundEmitter soundEmitter = Instantiate(soundEmitterPrefab);
			soundEmitter.gameObject.SetActive(false);
			return soundEmitter;
		}

		private void OnTakeFromPool(SoundEmitter soundEmitter)
		{
			soundEmitter.gameObject.SetActive(true);
			_activeSoundEmitters.Add(soundEmitter);
		}

		private void OnReturnedToPool(SoundEmitter soundEmitter)
		{
			if (soundEmitter.Node != null)
			{
				FrequentSoundEmitters.Remove(soundEmitter.Node);
				soundEmitter.Node = null;
			}

			soundEmitter.gameObject.SetActive(false);
			_activeSoundEmitters.Remove(soundEmitter);
		}

		private void OnDestroyPoolObject(SoundEmitter soundEmitter)
		{
			Destroy(soundEmitter.gameObject);
		}
	}
}