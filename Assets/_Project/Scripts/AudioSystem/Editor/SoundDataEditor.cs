// ReSharper disable CheckNamespace

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioSystem.Editors
{
	[CustomEditor(typeof(SoundData))]
	public class SoundDataEditor : Editor
	{
		private SerializedProperty _clipsProperty;
		private SerializedProperty _settingsProperty;
		private SerializedProperty _frequentSoundProperty;
		private SerializedProperty _volumeProperty;
		private SerializedProperty _pitchProperty;

		private Editor _settingsEditor;
		private bool _unlockSettings;
		
		private AudioSource _previewSource;
		private float _previewDistance = 10f;

		private void OnEnable()
		{
			_clipsProperty = serializedObject.FindProperty("clips");
			_clipsProperty.isExpanded = true;
			_settingsProperty = serializedObject.FindProperty("settings");
			_frequentSoundProperty = serializedObject.FindProperty("frequentSound");
			_volumeProperty = serializedObject.FindProperty("volume");
			_pitchProperty = serializedObject.FindProperty("pitch");
			
			GameObject previewObject = EditorUtility.CreateGameObjectWithHideFlags(
				"AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource));
			_previewSource = previewObject.GetComponent<AudioSource>();
		}

		private void OnDisable()
		{
			if (_previewSource != null)
			{
				DestroyImmediate(_previewSource.gameObject);
			}
			
			if (_settingsEditor == null) return;
			DestroyImmediate(_settingsEditor);
			_settingsEditor = null;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawSoundDataProperties();
			EditorGUILayout.Space();
			
			EditorGUILayout.Space();
			DrawPreviewButtons();
			EditorGUILayout.Space();

			DrawValidation();
			EditorGUILayout.Space();

			DrawSettingsPreview();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawSoundDataProperties()
		{
			EditorGUILayout.LabelField("Sound Data", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_clipsProperty, true);
			EditorGUILayout.PropertyField(_settingsProperty);
			EditorGUILayout.PropertyField(_frequentSoundProperty);
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Playback Settings", EditorStyles.boldLabel);
			
			EditorGUILayout.Slider(_volumeProperty, 0f, 1f, new GUIContent("Volume"));
			EditorGUILayout.Slider(_pitchProperty, -3f, 3f, new GUIContent("Pitch"));
		}
		
		private void DrawPreviewButtons()
		{
			SoundData soundData = (SoundData)target;
			AudioClip clip = soundData.GetClip();
			if (clip == null || _previewSource == null) return;
			
			EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
			
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Basic 2D"))
			{
				PlayPreview(false, false);
			}
			if (GUILayout.Button("With Settings"))
			{
				PlayPreview(true, false);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			_previewDistance = EditorGUILayout.FloatField(new GUIContent(
				"Distance", 
				"Distance to simulate."), 
				_previewDistance
			);
			if (GUILayout.Button("At Distance", GUILayout.Width(100)))
			{
				PlayPreview(true, true);
			}
			EditorGUILayout.EndHorizontal();

			using (new EditorGUI.DisabledScope(_previewSource == null || !_previewSource.isPlaying))
			{
				if (GUILayout.Button("Stop Preview"))
				{
					_previewSource.Stop();
				}
			}
		}

		private void PlayPreview(bool applySettings, bool applyDistance)
		{
			SoundData soundData = (SoundData)target;
			AudioClip clip = soundData.GetClip();

			if (clip == null || _previewSource == null) return;

			_previewSource.Stop();
			_previewSource.clip = clip;
			_previewSource.pitch = soundData.pitch;

			float simulatedDistanceVolume = 1f;

			if (applySettings && soundData.settings != null)
			{
				SoundDataSettings settings = soundData.settings;
				_previewSource.outputAudioMixerGroup = settings.mixerGroup;
				_previewSource.loop = settings.loop;
				_previewSource.mute = settings.mute;
				_previewSource.bypassEffects = settings.bypassEffects;
				_previewSource.bypassListenerEffects = settings.bypassListenerEffects;
				_previewSource.bypassReverbZones = settings.bypassReverbZones;
				_previewSource.priority = settings.priority;
				_previewSource.panStereo = settings.panStereo;
				_previewSource.reverbZoneMix = settings.reverbZoneMix;
				_previewSource.dopplerLevel = settings.dopplerLevel;
				_previewSource.spread = settings.spread;

				if (applyDistance)
				{
					_previewSource.spatialBlend = 0f;
					float min = settings.minDistance;
					float max = settings.maxDistance;
					float dist = Mathf.Clamp(_previewDistance, 0f, max);

					simulatedDistanceVolume = settings.rolloffMode switch
					{
						AudioRolloffMode.Custom => settings.customRolloffCurve.Evaluate(dist / max),
						AudioRolloffMode.Linear => 1f - Mathf.Clamp01((dist - min) / (max - min)),
						AudioRolloffMode.Logarithmic => min / Mathf.Max(dist, min),
						_ => throw new ArgumentOutOfRangeException()
					};
				}
				else
				{
					_previewSource.spatialBlend = settings.spatialBlend;
				}
			}
			else
			{
				// Reset to basic 2D defaults
				_previewSource.spatialBlend = 0f;
				_previewSource.bypassEffects = true;
				_previewSource.bypassListenerEffects = true;
				_previewSource.bypassReverbZones = true;
			}
			
			_previewSource.volume = soundData.volume * simulatedDistanceVolume;
			_previewSource.transform.position = Vector3.zero;

			_previewSource.Play();
		}

		private void DrawValidation()
		{
			SoundData soundData = (SoundData)target;

			if (soundData.settings == null)
			{
				EditorGUILayout.HelpBox("A SoundDataSettings asset is required.", MessageType.Warning);
			}

			if (soundData.clips == null || soundData.clips.Length == 0)
			{
				EditorGUILayout.HelpBox("At least one AudioClip should be assigned.", MessageType.Warning);
				return;
			}

			if (soundData.clips.All(t => t != null)) 
				return;
			
			EditorGUILayout.HelpBox("The clips array contains one or more null entries.", MessageType.Warning);
		}

		private void DrawSettingsPreview()
		{
			SoundData soundData = (SoundData)target;

			if (soundData.settings == null)
			{
				return;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Settings Preview", EditorStyles.boldLabel);
			
			// Toggle button to unlock editing
			_unlockSettings = GUILayout.Toggle(_unlockSettings, _unlockSettings ? 
				"Lock Settings" : "Edit Settings", "Button", GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();

			if (_unlockSettings)
			{
				EditorGUILayout.HelpBox(
					"Warning: Modifying SoundDataSettings here will affect ALL SoundData " +
					"assets that share this profile. Ensure this is your intended action.", 
					MessageType.Warning
				);
			}

			using (new EditorGUI.DisabledScope(!_unlockSettings))
			{
				CreateCachedEditor(soundData.settings, typeof(SoundDataSettingsEditor), ref _settingsEditor);

				if (_settingsEditor != null)
				{
					_settingsEditor.OnInspectorGUI();
				}
			}
		}
	}
}