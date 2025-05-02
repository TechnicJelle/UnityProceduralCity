#nullable enable
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(KritaImporter))]
	public class KritaImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Extract PNG"))
			{
				string assetPath = AssetDatabase.GetAssetPath(target);

				// Open the .kra file as a zip
				using ZipArchive zip = ZipFile.OpenRead(assetPath);
				ZipArchiveEntry png = KritaImporter.GetPNG(zip, assetPath);

				// Get directory of the .kra file
				string? directory = Path.GetDirectoryName(assetPath);
				if (directory == null)
				{
					throw new DirectoryNotFoundException($"Could not get directory of {assetPath}");
				}

				//Unpack the .png file
				string pngName = Path.GetFileNameWithoutExtension(assetPath) + ".png";
				using Stream stream = png.Open();
				string pngPath = Path.Combine(directory, pngName);
				Debug.Log($"Unpacking {assetPath}/{KritaImporter.PNG_FILE_NAME_INSIDE_KRA_ARCHIVE} to {pngPath}");
				using FileStream fileStream = new(pngPath, FileMode.Create);
				stream.CopyTo(fileStream);

				// Force Unity to reimport the asset
				AssetDatabase.ImportAsset(Path.GetRelativePath(".", pngPath), ImportAssetOptions.ForceUpdate);

				// Select the newly unpacked .png file in the file browser
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
			}

			ApplyRevertGUI();
		}
	}
}
