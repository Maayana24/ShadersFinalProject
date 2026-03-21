using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImagePool", menuName = "Scriptable Objects/ImagePool")]
public class ImagePool : ScriptableObject
{
    public List<ImageReference> Images = new List<ImageReference>();
}

[Serializable]
public struct ImageReference
{
    public Texture2D Right;
    public Texture2D Left;
}
