#!/usr/bin/env dart

import "dart:io";

void main() {
  final assetsDir = Directory("Assets");
  final bakedFiles = assetsDir
      .listSync(recursive: true)
      .whereType<File>()
      .where((file) => file.path.contains("_Bake_"));
  final Set<String> directoryStrings = bakedFiles
      .map((file) => file.parent.path)
      .toSet();
  for (final dir in directoryStrings) {
    final Directory dirPath = Directory(dir);
    final File diffuseFile = dirPath
        .listSync()
        .whereType<File>()
        .firstWhere((file) => file.path.endsWith("_Diffuse.png"));
    final File smoothnessFile = dirPath
        .listSync()
        .whereType<File>()
        .firstWhere((file) => file.path.endsWith("_Smoothness.png"));
    final String combinedFileName = diffuseFile.uri.pathSegments.last
        .replaceAll("_Diffuse", "_Combined");
    final File combinedFile = File("${dirPath.path}/$combinedFileName");
    print("Processing '${dirPath.path}'");
    print("  ➡️  Diffuse: '${diffuseFile.path}'");
    print("  ➡️  Smoothness: '${smoothnessFile.path}'");

    final ProcessResult result = Process.runSync(
      "magick",
      [
        diffuseFile.path,
        smoothnessFile.path,
        "-alpha",
        "off",
        "-compose",
        "CopyOpacity",
        "-composite",
        combinedFile.path,
      ],
    );
    if (result.exitCode != 0) {
      print("Error: ${result.stderr}");
    } else {
      print("  ✅ Success: '${combinedFile.path}'");
    }
  }
}
