using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImagePool", menuName = "Scriptable Objects/ImagePool")]
public class ImagePool : ScriptableObject
{
    public List<Texture2D> Images = new List<Texture2D>();
}
