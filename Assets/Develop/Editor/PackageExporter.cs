using System.Collections.Generic;
using UnityEditor;

public class PackageExporter
{
    // Packages以下を指定する場合フォルダパスではなく Packages/{パッケージ名} なので注意
    private static readonly string _packagePath = "Packages/com.qoopen.particlephysics";
    private static readonly string _fileName = "ParticlePhysics";

    [MenuItem("Tools/ExportPackage")]
    // 必ずstaticにする
    private static void Export()
    {
        // 出力ファイル名
        var exportPath = $"./{_fileName}.unitypackage";

        var exportedPackageAssetList = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("", new[] { _packagePath }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            exportedPackageAssetList.Add(path);
        }

        AssetDatabase.ExportPackage(
            exportedPackageAssetList.ToArray(),
            exportPath,
            ExportPackageOptions.Recurse);
    }
}