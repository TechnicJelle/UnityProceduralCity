#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(RoadGenerator))]
	public class RoadGeneratorEditor : UnityEditor.Editor
	{
		private RoadGenerator? _target;

		public override void OnInspectorGUI()
		{
			_target = (RoadGenerator)target;

			EditorGUI.BeginChangeCheck();

			GUIStyle labelStyle = EditorStyles.boldLabel;
			labelStyle.margin = new RectOffset {top = 10};

			GUILayout.Label("Global Options", labelStyle);
			GUILayout.BeginHorizontal();
			_target.automaticSeed = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.automaticSeed)), _target.automaticSeed);
			GUI.enabled = !_target.automaticSeed;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 42;
			_target.seed = EditorGUILayout.IntField(UppercaseWords(nameof(_target.seed)), _target.seed);
			EditorGUIUtility.labelWidth = labelWidth;
			if (GUILayout.Button("Reset")) _target.ResetRng();
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.Label("Generation Options", labelStyle);
			_target.width = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.width)), _target.width);
			_target.height = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.height)), _target.height);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_target.collidersToBridgeOver)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_target.collidersToAvoid)));

			GUI.enabled = _target.HasPoints();
			if (GUILayout.Button(UppercaseWords(nameof(_target.ClearRoads))))
			{
				_target.ClearRoads();
				SceneView.RepaintAll();
				Repaint();
			}
			GUI.enabled = true;


			GUILayout.Label("Spreading Options", labelStyle);
			_target.middleSpawnFactor = EditorGUILayout.Slider(UppercaseWords(nameof(_target.middleSpawnFactor)), _target.middleSpawnFactor, 0.0f, 0.5f);
			_target.initialStartPoints = EditorGUILayout.IntField(UppercaseWords(nameof(_target.initialStartPoints)), _target.initialStartPoints);

			if (GUILayout.Button(UppercaseWords(nameof(_target.SpreadStartingPoints))))
			{
				//TODO: Thread this again
				_target.SpreadStartingPoints();
			}


			GUILayout.Label("Stepping Options", labelStyle);
			_target.stepDistance = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.stepDistance)), _target.stepDistance);
			_target.stepDistanceForBridges = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.stepDistanceForBridges)), _target.stepDistanceForBridges);
			_target.maxRotationAmountRadians = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.maxRotationAmountRadians)), _target.maxRotationAmountRadians);
			_target.newRoadChance = EditorGUILayout.Slider(UppercaseWords(nameof(_target.newRoadChance)), _target.newRoadChance, 0.0f, 1.0f);

			GUI.enabled = _target.HasPoints();
			if (GUILayout.Button("Start stepping"))
			{
				//TODO: Thread this again
				_target.DoStepping();
			}
			GUI.enabled = true;


			GUILayout.Label("Merging Options", labelStyle);
			_target.mergeDistance = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.mergeDistance)), _target.mergeDistance);

			GUI.enabled = _target.HasPoints();
			if (GUILayout.Button(UppercaseWords(nameof(_target.MergeByDistance))))
			{
				_target.MergeByDistance();
			}
			GUI.enabled = true;


			GUILayout.Label("Verification Options", labelStyle);
			GUI.enabled = _target.HasPoints();
			if (GUILayout.Button(UppercaseWords(nameof(_target.VerifyConnections))))
			{
				_target.VerifyConnections();
			}
			GUI.enabled = true;


			GUILayout.Label("Mesh Options", labelStyle);
			_target.meshRadius = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.meshRadius)), _target.meshRadius);
			_target.textureStretching = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.textureStretching)), _target.textureStretching);
			GUI.enabled = _target.HasRoadMesh();
			if (GUILayout.Button(UppercaseWords(nameof(_target.ClearRoadMesh))))
			{
				_target.ClearRoadMesh();
			}
			GUI.enabled = _target.HasRoads();
			if (GUILayout.Button(UppercaseWords(nameof(_target.GenerateRoadMesh))))
			{
				_target.GenerateRoadMesh();
			}
			GUI.enabled = true;


			GUILayout.Label("Buildings Options", labelStyle);
			_target.buildingAlongRoadChance = EditorGUILayout.Slider(UppercaseWords(nameof(_target.buildingAlongRoadChance)), _target.buildingAlongRoadChance, 0.0f, 1.0f);
			_target.minRoadLengthForBuilding = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.minRoadLengthForBuilding)), _target.minRoadLengthForBuilding), 0.0f, _target.stepDistance);

			GUILayout.BeginHorizontal();
			_target.buildingLengthFactorMin = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingLengthFactorMin)), _target.buildingLengthFactorMin), 0.0f, _target.buildingLengthFactorMax);
			_target.buildingLengthFactorMax = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingLengthFactorMax)), _target.buildingLengthFactorMax), _target.buildingLengthFactorMin, 1.0f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			_target.buildingWidthFactorMin = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingWidthFactorMin)), _target.buildingWidthFactorMin), 0.0f, _target.buildingWidthFactorMax);
			_target.buildingWidthFactorMax = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingWidthFactorMax)), _target.buildingWidthFactorMax), _target.buildingWidthFactorMin, 1.0f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			_target.buildingHeightFactorMin = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingHeightFactorMin)), _target.buildingHeightFactorMin), 0.0f, _target.buildingHeightFactorMax);
			_target.buildingHeightFactorMax = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.buildingHeightFactorMax)), _target.buildingHeightFactorMax), _target.buildingHeightFactorMin, 2.0f);
			GUILayout.EndHorizontal();

			GUI.enabled = _target.HasBuildings();
			if (GUILayout.Button(UppercaseWords(nameof(_target.ClearBuildings))))
			{
				_target.ClearBuildings();
			}
			GUI.enabled = true;
			GUI.enabled = _target.HasRoads();
			if (GUILayout.Button(UppercaseWords(nameof(_target.GenerateBuildingsAlongRoads))))
			{
				_target.GenerateBuildingsAlongRoads();
			}
			GUI.enabled = true;


			GUILayout.Label("Buildings Object Options", labelStyle);
			_target.buildingsMaterial = ObjectField(UppercaseWords(nameof(_target.buildingsMaterial)), _target.buildingsMaterial);

			GUI.enabled = _target.HasBuildingsObject();
			if (GUILayout.Button(UppercaseWords(nameof(_target.ClearBuildingsObject))))
			{
				_target.ClearBuildingsObject();
			}
			GUI.enabled = true;
			GUI.enabled = _target.HasBuildings();
			if (GUILayout.Button(UppercaseWords(nameof(_target.GenerateBuildingsObject))))
			{
				_target.GenerateBuildingsObject();
			}
			GUI.enabled = true;


			GUILayout.Label("Roofs Options", labelStyle);
			_target.roofsMaterial = ObjectField(UppercaseWords(nameof(_target.roofsMaterial)), _target.roofsMaterial);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_target.roofPrefabs)));

			GUI.enabled = _target.HasRoofsObject();
			if (GUILayout.Button(UppercaseWords(nameof(_target.ClearRoofsObject))))
			{
				_target.ClearRoofsObject();
			}
			GUI.enabled = true;
			GUI.enabled = _target.HasBuildings();
			if (GUILayout.Button(UppercaseWords(nameof(_target.GenerateRoofsObject))))
			{
				_target.GenerateRoofsObject();
			}
			GUI.enabled = true;


			GUILayout.Label("Debug Drawing Options", labelStyle);
			_target.showPointsSphere = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showPointsSphere)), _target.showPointsSphere);
			_target.sphereSizeDefault = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.sphereSizeDefault)), _target.sphereSizeDefault);
			_target.sphereSizeIncrease = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.sphereSizeIncrease)), _target.sphereSizeIncrease);
			_target.showPointsIndex = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showPointsIndex)), _target.showPointsIndex);
			_target.showRoadLines = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showRoadLines)), _target.showRoadLines);
			_target.showBuildingBoxes = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showBuildingBoxes)), _target.showBuildingBoxes);


			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
				EditorUtility.SetDirty(_target);
				serializedObject.ApplyModifiedProperties();
			}
		}

		private static string UppercaseWords(string input)
		{
			input = input.Trim('_');
			List<string> words = new();

			//split the string into words, by uppercase letters
			int start = 0;
			for(int i = 1; i < input.Length; i++)
			{
				char c = input[i];
				if (!char.IsUpper(c)) continue;
				words.Add(input.Substring(start, i - start));
				start = i;
			}

			//add the last word
			words.Add(input[start..]);

			//capitalize the first letter of each word
			for(int i = 0; i < words.Count; i++)
			{
				words[i] = char.ToUpper(words[i][0]) + words[i][1..];
			}

			return string.Join(" ", words);
		}

		private static T ObjectField<T>(string label, T? obj, bool allowObjectsFromScene = false) where T : Object
		{
			return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allowObjectsFromScene);
		}
	}
}
