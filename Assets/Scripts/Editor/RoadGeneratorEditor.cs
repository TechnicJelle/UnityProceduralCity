using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(RoadGenerator))]
	public class RoadGeneratorEditor : UnityEditor.Editor
	{
		private RoadGenerator _target;
		[CanBeNull] private Thread _thread;

		private int _takeOutPoint = 0;

		public override void OnInspectorGUI()
		{
			_target = (RoadGenerator)target;

			GUI.enabled = _thread is {IsAlive: true};
			if (GUILayout.Button("Stop Current Process!"))
			{
				_thread?.Abort();
				_thread = null;
				SceneView.RepaintAll();
				Repaint();
			}
			GUI.enabled = true;

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

			GUI.enabled = _target.HasPoints();
			if (GUILayout.Button("Clear Roads"))
			{
				_thread?.Abort();
				_thread = null;
				_target.ClearRoads();
				SceneView.RepaintAll();
				Repaint();
			}
			GUI.enabled = true;


			GUILayout.Label("Spreading Options", labelStyle);
			_target.middleSpawnFactor = EditorGUILayout.Slider(UppercaseWords(nameof(_target.middleSpawnFactor)), _target.middleSpawnFactor, 0.0f, 0.5f);
			_target.initialStartPoints = EditorGUILayout.IntField(UppercaseWords(nameof(_target.initialStartPoints)), _target.initialStartPoints);

			GUI.enabled = _thread is not {IsAlive: true};
			if (GUILayout.Button("Spread starting points"))
			{
				_thread = new Thread(() => _target.SpreadStartingPoints());
				_thread.Start();
			}
			GUI.enabled = true;


			GUILayout.Label("Stepping Options", labelStyle);
			_target.stepSize = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.stepSize)), _target.stepSize);
			_target.maxRotationAmountRadians = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.maxRotationAmountRadians)), _target.maxRotationAmountRadians);
			_target.newRoadChance = EditorGUILayout.Slider(UppercaseWords(nameof(_target.newRoadChance)), _target.newRoadChance, 0.0f, 1.0f);

			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			if (GUILayout.Button("Start stepping"))
			{
				_thread = new Thread(() => _target.DoStepping());
				_thread.Start();
			}
			GUI.enabled = true;

			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			if (GUILayout.Button("Double link"))
			{
				_thread = new Thread(() => _target.DoubleLink());
				_thread.Start();
			}
			GUI.enabled = true;


			GUILayout.Label("Merging Options", labelStyle);
			_target.mergeDistance = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.mergeDistance)), _target.mergeDistance);

			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			if (GUILayout.Button(UppercaseWords(nameof(_target.MergeByDistance))))
			{
				_thread = new Thread(() => _target.MergeByDistance());
				_thread.Start();
			}
			GUI.enabled = true;

			_target.acceptableStraightsMin = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.acceptableStraightsMin)), _target.acceptableStraightsMin), 0f, _target.acceptableStraightsMax);
			_target.acceptableStraightsMax = Mathf.Clamp(EditorGUILayout.FloatField(UppercaseWords(nameof(_target.acceptableStraightsMax)), _target.acceptableStraightsMax), _target.acceptableStraightsMin, 360f);
			EditorGUILayout.MinMaxSlider(ref _target.acceptableStraightsMin, ref _target.acceptableStraightsMax, 0f, 360f);
			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			if (GUILayout.Button(UppercaseWords(nameof(_target.MergeUnacceptableStraights))))
			{
				_thread = new Thread(() => _target.MergeUnacceptableStraights());
				_thread.Start();
			}
			GUI.enabled = true;


			GUILayout.Label("Take Out Options", labelStyle);
			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			_takeOutPoint = Mathf.Clamp(EditorGUILayout.IntField(UppercaseWords(nameof(_takeOutPoint)), _takeOutPoint), 0, _target.Points.Count - 1);
			if (GUILayout.Button("Take out point"))
			{
				_thread = new Thread(() => _target.TakeOutPoint(_takeOutPoint));
				_thread.Start();
			}
			GUI.enabled = true;


			GUILayout.Label("Verification Options", labelStyle);
			GUI.enabled = _target.HasPoints() && _thread is not {IsAlive: true};
			if (GUILayout.Button(UppercaseWords(nameof(_target.VerifyConnections))))
			{
				_thread = new Thread(() => _target.VerifyConnections());
				_thread.Start();
			}
			GUI.enabled = true;


			GUILayout.Label("Debug Drawing Options", labelStyle);
			_target.arrowHeadSize = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.arrowHeadSize)), _target.arrowHeadSize);
			_target.arrowHeadAngle = EditorGUILayout.FloatField(UppercaseWords(nameof(_target.arrowHeadAngle)), _target.arrowHeadAngle);
			_target.showPointsIndex = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showPointsIndex)), _target.showPointsIndex);
			_target.showStraightness = EditorGUILayout.Toggle(UppercaseWords(nameof(_target.showStraightness)), _target.showStraightness);


			GUILayout.Label("Prefab Options", labelStyle);
			_target.roadPrefab = (GameObject)EditorGUILayout.ObjectField(UppercaseWords(nameof(_target.roadPrefab)), _target.roadPrefab, typeof(GameObject), true);


			// if thread is going, repaint the scene every frame, so we can see its progress
			if (_thread is {IsAlive: true})
			{
				SceneView.RepaintAll();
				Repaint();
			}

			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
				EditorUtility.SetDirty(_target);
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
	}
}
