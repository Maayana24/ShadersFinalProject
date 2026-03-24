using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum Particles { HairCutting, Sparkles, Spray}
public class ParticleController : MonoBehaviour
{
    [SerializeField] VisualEffect hairCutEffect;
    [SerializeField] VisualEffect sparkleEffect;
    [SerializeField] VisualEffect sprayEffect;

    Dictionary<Particles, VisualEffect> effects;

    Particles currentParticleSystem;

    private void Awake()
    {
        effects = new Dictionary<Particles, VisualEffect>();
        effects.Add(Particles.HairCutting, hairCutEffect);
        effects.Add(Particles.Sparkles, sparkleEffect);
        effects.Add(Particles.Spray, sprayEffect);
        
        foreach(VisualEffect effect in effects.Values)
        {
            effect.Stop();
        }
    }


    //Call when the player changes their tool
    public void SetTool(Particles particleType)
    {
        currentParticleSystem = particleType;
    }

    //Call when the player starts using their tool
    public void Play()
    {
        effects[currentParticleSystem].Play();
    }

    //Call when the player stops using their tool
    public void Stop()
    {
        effects[currentParticleSystem].Stop();
    }

    //Call when the player hovers a new color on the sheep's hair
    public void SetHairColor(Color color)
    {
        effects[Particles.HairCutting].SetVector4("Color", color);
    }

    //Call when the player picks a different color of the spray tool
    public void SetCanColor(Color color)
    {
        effects[Particles.Spray].SetVector4("Spray Color", color);
    }
}
