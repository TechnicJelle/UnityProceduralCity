using System.IO;
using System.IO.Compression;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor
{
	//inspired by https://github.com/justalexi/unity_krita_exporter

	[ScriptedImporter(1, "kra")]
	public class KritaImporter : ScriptedImporter
	{
		// A .kra file contains a file named "mergedimage.png"
		// which contains the rendered image as you see it on your canvas
		// (see https://docs.krita.org/en/general_concepts/file_formats/file_kra.html).
		private const string PNG_FILE_NAME_INSIDE_KRA_ARCHIVE = "mergedimage.png";

		public override void OnImportAsset(AssetImportContext ctx)
		{
			//Get directory of the .kra file
			FileInfo file = new(ctx.assetPath);
			DirectoryInfo directory = file.Directory;
			if (directory == null)
			{
				Debug.LogError($"Could not get directory of {ctx.assetPath}");
				return;
			}

			//Open the .kra file as a zip and get the PNG file inside
			ZipArchive zip = ZipFile.OpenRead(ctx.assetPath);
			ZipArchiveEntry png = zip.GetEntry(PNG_FILE_NAME_INSIDE_KRA_ARCHIVE);
			if (png == null)
			{
				Debug.LogError($"No {PNG_FILE_NAME_INSIDE_KRA_ARCHIVE} file found in {ctx.assetPath}");
				return;
			}

			//Unpack the PNG file
			string pngName = Path.GetFileNameWithoutExtension(ctx.assetPath) + ".png";
			using Stream stream = png.Open();
			string pngPath = Path.Combine(directory.FullName, pngName);
			Debug.Log($"Unpacking {ctx.assetPath}/{PNG_FILE_NAME_INSIDE_KRA_ARCHIVE} to {pngPath}");
			using FileStream fileStream = new(pngPath, FileMode.Create);
			stream.CopyTo(fileStream);
		}
	}
}
