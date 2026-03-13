// ReSharper disable CheckNamespace

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AudioSystem.Editors
{
	[CustomEditor(typeof(SoundDataSettings))]
	public class SoundDataSettingsEditor : Editor
	{
		private SerializedProperty _mixerGroupProperty;
		private SerializedProperty _loopProperty;

		private SerializedProperty _muteProperty;
		private SerializedProperty _bypassEffectsProperty;
		private SerializedProperty _bypassListenerEffectsProperty;
		private SerializedProperty _bypassReverbZonesProperty;

		private SerializedProperty _priorityProperty;
		private SerializedProperty _panStereoProperty;
		private SerializedProperty _spatialBlendProperty;
		private SerializedProperty _reverbZoneMixProperty;
		private SerializedProperty _dopplerLevelProperty;
		private SerializedProperty _spreadProperty;

		private SerializedProperty _minDistanceProperty;
		private SerializedProperty _maxDistanceProperty;

		private SerializedProperty _ignoreListenerVolumeProperty;
		private SerializedProperty _ignoreListenerPauseProperty;

		private SerializedProperty _rolloffModeProperty;
		
		private SerializedProperty _customRolloffCurveProperty;
		private SerializedProperty _spatialBlendCurveProperty;
		private SerializedProperty _spreadCurveProperty;
		private SerializedProperty _reverbZoneMixCurveProperty;
		
		private GameObject _dummyAudioObject;
		private AudioSource _dummyAudioSource;
		private Editor _audioSourceEditor;
		private MethodInfo _audio3DGuiMethod;
		private bool _expanded3D = true;

		private void OnEnable()
		{
			_mixerGroupProperty = serializedObject.FindProperty("mixerGroup");
			_loopProperty = serializedObject.FindProperty("loop");

			_muteProperty = serializedObject.FindProperty("mute");
			_bypassEffectsProperty = serializedObject.FindProperty("bypassEffects");
			_bypassListenerEffectsProperty = serializedObject.FindProperty("bypassListenerEffects");
			_bypassReverbZonesProperty = serializedObject.FindProperty("bypassReverbZones");

			_priorityProperty = serializedObject.FindProperty("priority");
			_panStereoProperty = serializedObject.FindProperty("panStereo");
			_spatialBlendProperty = serializedObject.FindProperty("spatialBlend");
			_reverbZoneMixProperty = serializedObject.FindProperty("reverbZoneMix");
			_dopplerLevelProperty = serializedObject.FindProperty("dopplerLevel");
			_spreadProperty = serializedObject.FindProperty("spread");

			_minDistanceProperty = serializedObject.FindProperty("minDistance");
			_maxDistanceProperty = serializedObject.FindProperty("maxDistance");

			_ignoreListenerVolumeProperty = serializedObject.FindProperty("ignoreListenerVolume");
			_ignoreListenerPauseProperty = serializedObject.FindProperty("ignoreListenerPause");

			_rolloffModeProperty = serializedObject.FindProperty("rolloffMode");
			
			_customRolloffCurveProperty = serializedObject.FindProperty("customRolloffCurve");
			_spatialBlendCurveProperty = serializedObject.FindProperty("spatialBlendCurve");
			_spreadCurveProperty = serializedObject.FindProperty("spreadCurve");
			_reverbZoneMixCurveProperty = serializedObject.FindProperty("reverbZoneMixCurve");

			_dummyAudioObject = new GameObject("DummyAudioSource_Preview")
			{
				hideFlags = HideFlags.HideInHierarchy
			};
			_dummyAudioSource = _dummyAudioObject.AddComponent<AudioSource>();
			
			_audioSourceEditor = CreateEditor(_dummyAudioSource);
			_audio3DGuiMethod = _audioSourceEditor.GetType().GetMethod(
				"Audio3DGUI", 
				BindingFlags.NonPublic | BindingFlags.Instance
			);
		}
		
		private void OnDisable()
		{
			if (_audioSourceEditor != null) DestroyImmediate(_audioSourceEditor);
			if (_dummyAudioObject != null) DestroyImmediate(_dummyAudioObject);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
			
			EditorGUILayout.PropertyField(_mixerGroupProperty);
			EditorGUILayout.PropertyField(_loopProperty);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Source Flags", EditorStyles.boldLabel);
			
			EditorGUILayout.PropertyField(_muteProperty);
			EditorGUILayout.PropertyField(_bypassEffectsProperty);
			EditorGUILayout.PropertyField(_bypassListenerEffectsProperty);
			EditorGUILayout.PropertyField(_bypassReverbZonesProperty);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
			
			DrawIntSliderWithLabels(_priorityProperty, 0, 256, "Priority", "High", "Low");
			DrawSliderWithLabels(_panStereoProperty, -1f, 1f, "Stereo Pan", "Left", "Right");
			DrawSliderWithLabels(_spatialBlendProperty, 0f, 1f, "Spatial Blend", "2D", "3D");

			EditorGUILayout.Slider(_reverbZoneMixProperty, 0f, 1.1f, new GUIContent("Reverb Zone Mix"));

			EditorGUILayout.Space();

			if (_spatialBlendProperty.floatValue > 0f)
			{
				_expanded3D = EditorGUILayout.Foldout(_expanded3D, "3D Sound Settings", true);

				if (_expanded3D)
				{
					EditorGUI.indentLevel++;
					DrawAudioSource3DSettings();
					EditorGUI.indentLevel--;
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Listener", EditorStyles.boldLabel);
			
			EditorGUILayout.PropertyField(_ignoreListenerVolumeProperty);
			EditorGUILayout.PropertyField(_ignoreListenerPauseProperty);

			EditorGUILayout.Space(10);
			if (GUILayout.Button("Reset to Defaults"))
			{
				ResetToDefaults();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawAudioSource3DSettings()
		{
			if (_audio3DGuiMethod == null || _dummyAudioSource == null) return;

			// Sync SoundDataSettings to Dummy AudioSource
			_dummyAudioSource.dopplerLevel = _dopplerLevelProperty.floatValue;
			_dummyAudioSource.spread = _spreadProperty.floatValue;
			_dummyAudioSource.minDistance = _minDistanceProperty.floatValue;
			_dummyAudioSource.maxDistance = _maxDistanceProperty.floatValue;
			_dummyAudioSource.rolloffMode = (AudioRolloffMode)_rolloffModeProperty.enumValueIndex;
			_dummyAudioSource.spatialBlend = _spatialBlendProperty.floatValue;
			_dummyAudioSource.reverbZoneMix = _reverbZoneMixProperty.floatValue;

			if (_customRolloffCurveProperty.animationCurveValue != null)
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, 
					_customRolloffCurveProperty.animationCurveValue);
			if (_spatialBlendCurveProperty.animationCurveValue != null)
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, 
					_spatialBlendCurveProperty.animationCurveValue);
			if (_spreadCurveProperty.animationCurveValue != null)
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.Spread, 
					_spreadCurveProperty.animationCurveValue);
			if (_reverbZoneMixCurveProperty.animationCurveValue != null)
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, 
					_reverbZoneMixCurveProperty.animationCurveValue);

			// Invoke native Audio3DGUI
			_audioSourceEditor.serializedObject.Update();
			_audio3DGuiMethod.Invoke(_audioSourceEditor, null);
			_audioSourceEditor.serializedObject.ApplyModifiedProperties();

			// Sync Dummy AudioSource back to SoundDataSettings
			_dopplerLevelProperty.floatValue = _dummyAudioSource.dopplerLevel;
			_spreadProperty.floatValue = _dummyAudioSource.spread;
			_minDistanceProperty.floatValue = _dummyAudioSource.minDistance;
			_maxDistanceProperty.floatValue = _dummyAudioSource.maxDistance;
			_rolloffModeProperty.enumValueIndex = (int)_dummyAudioSource.rolloffMode;

			_customRolloffCurveProperty.animationCurveValue = 
				_dummyAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
			_spatialBlendCurveProperty.animationCurveValue = 
				_dummyAudioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
			_spreadCurveProperty.animationCurveValue = 
				_dummyAudioSource.GetCustomCurve(AudioSourceCurveType.Spread);
			_reverbZoneMixCurveProperty.animationCurveValue = 
				_dummyAudioSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix);
		}
		
		private void DrawSliderWithLabels(SerializedProperty property, float min, float max, 
			string title, string leftLabel, string rightLabel)
		{
			Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 14f);
			
			Rect sliderRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.Slider(sliderRect, property, min, max, new GUIContent(title));

			DrawSliderLabels(rect, leftLabel, rightLabel);
		}

		private void DrawIntSliderWithLabels(SerializedProperty property, int min, int max, 
			string title, string leftLabel, string rightLabel)
		{
			Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 14f);
			
			Rect sliderRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.IntSlider(sliderRect, property, min, max, new GUIContent(title));

			DrawSliderLabels(rect, leftLabel, rightLabel);
		}

		private void DrawSliderLabels(Rect totalRect, string leftLabel, string rightLabel)
		{
			Rect labelsRect = new(
				totalRect.x + EditorGUIUtility.labelWidth, 
				totalRect.y + EditorGUIUtility.singleLineHeight, 
				totalRect.width - EditorGUIUtility.labelWidth - 55f, 
				14f
			);

			GUIStyle leftStyle = new(EditorStyles.miniLabel) { alignment = TextAnchor.UpperLeft };
			GUIStyle rightStyle = new(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight };

			GUI.Label(labelsRect, leftLabel, leftStyle);
			GUI.Label(labelsRect, rightLabel, rightStyle);
		}
		
		private void ResetToDefaults()
		{
			GameObject tempObj = new("TempAudioSource");
			AudioSource tempSource = tempObj.AddComponent<AudioSource>();

			_mixerGroupProperty.objectReferenceValue = tempSource.outputAudioMixerGroup;
			_loopProperty.boolValue = tempSource.loop;

			_muteProperty.boolValue = tempSource.mute;
			_bypassEffectsProperty.boolValue = tempSource.bypassEffects;
			_bypassListenerEffectsProperty.boolValue = tempSource.bypassListenerEffects;
			_bypassReverbZonesProperty.boolValue = tempSource.bypassReverbZones;

			_priorityProperty.intValue = tempSource.priority;
			_panStereoProperty.floatValue = tempSource.panStereo;
			_spatialBlendProperty.floatValue = tempSource.spatialBlend;
			_reverbZoneMixProperty.floatValue = tempSource.reverbZoneMix;
			_dopplerLevelProperty.floatValue = tempSource.dopplerLevel;
			_spreadProperty.floatValue = tempSource.spread;

			_minDistanceProperty.floatValue = tempSource.minDistance;
			_maxDistanceProperty.floatValue = tempSource.maxDistance;

			_ignoreListenerVolumeProperty.boolValue = tempSource.ignoreListenerVolume;
			_ignoreListenerPauseProperty.boolValue = tempSource.ignoreListenerPause;

			_rolloffModeProperty.enumValueIndex = (int)tempSource.rolloffMode;

			// Reset custom curves
			_customRolloffCurveProperty.animationCurveValue = 
				tempSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
			_spatialBlendCurveProperty.animationCurveValue = 
				tempSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
			_spreadCurveProperty.animationCurveValue = 
				tempSource.GetCustomCurve(AudioSourceCurveType.Spread);
			_reverbZoneMixCurveProperty.animationCurveValue = 
				tempSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix);
			
			if (_dummyAudioSource != null)
			{
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, 
					tempSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, 
					tempSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.Spread, 
					tempSource.GetCustomCurve(AudioSourceCurveType.Spread));
				_dummyAudioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, 
					tempSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
			}

			DestroyImmediate(tempObj);
		}
	}
}