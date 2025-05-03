#nullable enable
using System.Drawing;
using System.IO;
using System.IO.Compression;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor
{
	// Inspired by https://github.com/justalexi/unity_krita_exporter

	[ScriptedImporter(1, "kra")]
	public class KritaImporter : ScriptedImporter
	{
		// A .kra file contains a file named "mergedimage.png"
		// which contains the rendered image as you see it on your canvas
		// (see https://docs.krita.org/en/general_concepts/file_formats/file_kra.html).
		public const string PNG_FILE_NAME_INSIDE_KRA_ARCHIVE = "mergedimage.png";

		public static ZipArchiveEntry GetPNG(ZipArchive zip, string assetPath)
		{
			// Get the asset .png file inside
			ZipArchiveEntry? png = zip.GetEntry(PNG_FILE_NAME_INSIDE_KRA_ARCHIVE);
			if (png == null)
			{
				throw new FileNotFoundException($"No {PNG_FILE_NAME_INSIDE_KRA_ARCHIVE} file found in {assetPath}");
			}

			return png;
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			// Open the .kra file as a zip
			using ZipArchive zip = ZipFile.OpenRead(assetPath);
			ZipArchiveEntry png = GetPNG(zip, ctx.assetPath);

			//get image info (width, height) to ensure mipmap generation works
			Image img = Image.FromStream(png.Open());

			// Load the .png file into a Texture2D
			Texture2D texture = new(img.Width, img.Height);
			using (Stream stream = png.Open())
			{
				byte[] bytes = new byte[png.Length];
				int result = stream.Read(bytes, 0, bytes.Length);
				if (result == 0)
				{
					Debug.LogError($"Could not read {png.Length} bytes from {png.FullName}");
					return;
				}
				if (result != png.Length)
				{
					Debug.LogError($"Could not read all {png.Length} bytes from {png.FullName}");
					return;
				}
				texture.LoadImage(bytes);
			}
			texture.Apply();

			// Save the texture, so it can be used in the editor
			ctx.AddObjectToAsset("texture", texture);
			ctx.SetMainObject(texture);
		}
	}
}
