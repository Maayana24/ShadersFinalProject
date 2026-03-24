using UnityEngine;
using System.Collections.Generic;

public enum SprayColor
{
    Red, 
    Blue, 
    Green, 
    Yellow, 
    White, 
    Black 
}

public class ColorSpray : Item
{
    [SerializeField] private SprayColor sprayColor;

    private static readonly Dictionary<SprayColor, Color> ColorMap = new Dictionary<SprayColor, Color>
    {
         { SprayColor.Red,    Color.red    },
         { SprayColor.Blue,   Color.blue   },
         { SprayColor.Green,  Color.green  },
         { SprayColor.Yellow, Color.yellow },
         { SprayColor.White,  Color.white  },
         { SprayColor.Black,  Color.black  },
    };

    public Color Color => ColorMap[sprayColor];
    public override Particles Type { get; protected set; } = Particles.Spray;
    public override void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject)
    {
        Debug.Log($"Spray bottle: {sprayColor.ToString()}");
    }
}