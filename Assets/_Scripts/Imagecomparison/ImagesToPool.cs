using UnityEngine;
using System.Collections;

using UnityEditor;

public class ImagesToPool : MonoBehaviour
{
    [SerializeField] private ImagePool pool;
    [SerializeField] private CaptureImage capture;

    [SerializeField] private Camera[] cameras; //right then left

    [ContextMenu("Capture")]
    public void GetNewImageToPool()
    {
        StartCoroutine(AddToPool());
    }

    private IEnumerator AddToPool()
    {
        ImageReference reference = new ImageReference();
        Texture2D[] textures = new Texture2D[2];

        for(int i = 0; i < cameras.Length; i++)
        {
            yield return StartCoroutine(capture.Capture(cameras[i]));

            if (!System.IO.Directory.Exists("Assets/ReferencePhotos"))
                System.IO.Directory.CreateDirectory("Assets/ReferencePhotos");

            string[] sides = { "Right", "Left" };
            string path = $"Assets/ReferencePhotos/Reference_{pool.Images.Count}_{sides[i]}.png";
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
            textures[i] = savedTexture;
        }

        reference.Right = textures[0];


        reference.Left = textures[1];
        pool.Images.Add(reference);
        EditorUtility.SetDirty(pool);
        AssetDatabase.SaveAssets();
    }
}