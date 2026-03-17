using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private ImagePool pool;


    [ContextMenu("ChangeImage")]
    public void ChangeImage()
    {
        image.texture = pool.Images[Random.Range(0, pool.Images.Count)];
    }
}
