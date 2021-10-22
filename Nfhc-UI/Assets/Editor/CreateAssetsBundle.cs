using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class CreateAssetBundles
    {
        private const string UNITY_DIRECTORY = "AssetBundles";
        private const string NFHC_DIRECTORY = @"..\NfhcModel\StreamingAssets\";

        [MenuItem("NFHC/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            try
            {
                if (Directory.Exists(UNITY_DIRECTORY))
                {
                    Directory.Delete(UNITY_DIRECTORY, true);
                }
                Directory.CreateDirectory(UNITY_DIRECTORY);
                BuildPipeline.BuildAssetBundles(UNITY_DIRECTORY, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

                Directory.Exists(NFHC_DIRECTORY);
                if (Directory.Exists(NFHC_DIRECTORY))
                {
                    foreach (string assetBundleName in AssetDatabase.GetAllAssetBundleNames())
                    {
                        File.Copy(Path.Combine(UNITY_DIRECTORY, assetBundleName), Path.Combine(NFHC_DIRECTORY, assetBundleName), true);
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException(NFHC_DIRECTORY + " wasn't found");
                }
                Debug.Log("Building NFHC AssetBundles successfully finished");
            }
            catch (Exception ex)
            {
                Debug.LogError("Building NFHC AssetBundles successfully finished");
                Debug.LogException(ex);
            }

        }
    }
}
