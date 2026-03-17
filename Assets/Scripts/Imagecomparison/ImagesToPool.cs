using UnityEngine;
using System.Collections;

using UnityEditor;

public class ImagesToPool : MonoBehaviour
{
    [SerializeField] private ImagePool pool;
    [SerializeField] private CaptureImage capture;

    [ContextMenu("Capture")]
    public void GetNewImageToPool()
    {
        StartCoroutine(AddToPool());
    }

    private IEnumerator AddToPool()
    {
        yield return StartCoroutine(capture.Capture());

        if (!System.IO.Directory.Exists("Assets/ReferencePhotos"))
            System.IO.Directory.CreateDirectory("Assets/ReferencePhotos");

        string path = $"Assets/ReferencePhotos/Reference_{pool.Images.Count}.png";
        byte[] png = capture.LastCapture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        importer.textureType = TextureImporterType.Default;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        pool.Images.Add(savedTexture);
        EditorUtility.SetDirty(pool);
        AssetDatabase.SaveAssets();
    }
}