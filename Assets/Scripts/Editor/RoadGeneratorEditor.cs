using JetBrains.Annotations;
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(RoadGenerator))]
	public class RoadGeneratorEditor : UnityEditor.Editor
	{
		private RoadGenerator _target;
		[CanBeNull] private Thread _generation;

		public override void OnInspectorGUI()
		{
			_target = (RoadGenerator)target;

			// Draw default inspector
			base.OnInspectorGUI();

			// Draw a button to clear the roads
			if (GUILayout.Button("Clear Roads"))
			{
				_generation?.Abort();
				_generation = null;
				_target.ClearRoads();
				SceneView.RepaintAll();
				Repaint();
			}

			// Draw a button to generate the road
			GUI.enabled = _target.Completed != RoadGenerator.GenerationState.Generating;
			if (GUILayout.Button("Generate Roads"))
			{
				_generation = new Thread(() => _target.Generate());
				_generation.Start();
			}
			GUI.enabled = true;

			if (_generation is {IsAlive: true})
			{
				SceneView.RepaintAll();
				Repaint();
			}

			// Draw status indicator for Completed
			string statusText;
			Color statusColour;
			switch(_target.Completed)
			{
				case RoadGenerator.GenerationState.Empty:
					statusText = "Empty";
					statusColour = Color.red;
					break;
				case RoadGenerator.GenerationState.Generating:
					statusText = "Generating";
					statusColour = Color.yellow;
					break;
				case RoadGenerator.GenerationState.Finished:
					statusText = "Finished";
					statusColour = Color.green;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			EditorGUILayout.LabelField("Status: " + statusText,
				new GUIStyle
				{
					normal = {textColor = statusColour},
					fontSize = 14,
					margin = new RectOffset(0, 0, 10, 0),
				});
		}
	}
}
