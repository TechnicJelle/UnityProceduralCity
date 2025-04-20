using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(RoadGenerator))]
	public class RoadGeneratorEditor : UnityEditor.Editor
	{
		private RoadGenerator _target;

		public override void OnInspectorGUI()
		{
			_target = (RoadGenerator)target;

			// Draw default inspector
			base.OnInspectorGUI();

			// Draw a button to clear the roads
			if (GUILayout.Button("Clear Roads"))
			{
				_target.ClearRoads();
			}

			// Draw a button to generate the road
			if (GUILayout.Button("Generate Roads"))
			{
				_target.StartCoroutine(_target.Generate());
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
