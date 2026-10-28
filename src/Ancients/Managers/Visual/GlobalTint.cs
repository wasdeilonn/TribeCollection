using HarmonyLib;
using Polytopia.Data;
using Il2Gen = Il2CppSystem.Collections.Generic;
using Polibrary.PolyScript;
using M = TribeCollection.Main;
using UnityEngine;


namespace TribeCollection.Ancients.Visual;

public class GlobalTint : MonoBehaviour
{
    public GlobalTint(IntPtr ptr) : base(ptr) { }
    public static bool Enabled = true;
    public static Color From = Color.white;
    public static Color To = new Color(0.3f, 0.6f, 1f, 1f);
    public static float Duration = 1.5f;
    static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    static readonly int OverlayColorId = Shader.PropertyToID("_OverlayColor");
    static readonly int OverlayStrengthId = Shader.PropertyToID("_OverlayStrength");
    static readonly Dictionary<PolytopiaSpriteRenderer, Color> polyTargets = new Dictionary<PolytopiaSpriteRenderer, Color>();
    static readonly Dictionary<SpriteRenderer, Color> unityTargets = new Dictionary<SpriteRenderer, Color>();
    static bool wasEnabled = true;
    public static void Register(PolytopiaSpriteRenderer renderer)
    {
        if (renderer != null && !polyTargets.ContainsKey(renderer))
        {
            polyTargets[renderer] = renderer.Color;
        }
    }
    public static void Unregister(PolytopiaSpriteRenderer renderer)
    {
        if (renderer != null && polyTargets.TryGetValue(renderer, out var orig))
        {
            renderer.Color = orig;
            polyTargets.Remove(renderer);
        }
    }
    public static void Register(SpriteRenderer renderer)
    {
        if (renderer != null && !unityTargets.ContainsKey(renderer))
        {
            unityTargets[renderer] = renderer.color;
        }
    }
    static void ApplyOverlay(SpriteRenderer renderer, Color color, float strength)
    {
        renderer.GetPropertyBlock(block);
        block.SetColor(OverlayColorId, color);
        block.SetFloat(OverlayStrengthId, strength);
        renderer.SetPropertyBlock(block);
    }
    public static void Unregister(SpriteRenderer renderer)
    {
        if (renderer != null && unityTargets.Remove(renderer))
        {
            ApplyOverlay(renderer, To, 0f);
        }
    }
    public static void Toggle() => Enabled = !Enabled;
    void Update()
    {
        if (!Enabled)
        {
            if (wasEnabled)
            {
                foreach (var kv in polyTargets) if (kv.Key != null) kv.Key.Color = kv.Value; // <- adjust
                foreach (var kv in unityTargets) if (kv.Key != null) ApplyOverlay(kv.Key, To, 0f);
            }
            wasEnabled = false;
            return;
        }
        wasEnabled = true;

        float overlayStrenght = 0.6f;
        
        float num = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / Duration, 1f));
        Color color = Color.Lerp(From, To, num);
 
        
        List<PolytopiaSpriteRenderer> deadPoly = null;
        foreach (var kvp in polyTargets)
        {
            PolytopiaSpriteRenderer renderer = kvp.Key;
            if (renderer == null) 
            { 
                if (deadPoly == null)
                {
                    deadPoly = new List<PolytopiaSpriteRenderer>();
                }
                deadPoly.Add(renderer); 
                continue; 
            }
            renderer.Color = color;
        }
        if (deadPoly != null)
        {
            foreach (var d in deadPoly) 
            {
                polyTargets.Remove(d);
            }
        }
 
        List<SpriteRenderer> deadUnity = null;
        foreach (var kvp in unityTargets)
        {
            SpriteRenderer renderer = kvp.Key;
            if (renderer == null) 
            { 
                if (deadUnity == null)
                {
                    deadUnity = new();
                }
                deadUnity.Add(kvp.Key);
                continue; 
            }
            ApplyOverlay(kvp.Key, To, num * overlayStrenght);
        }
        if (deadUnity != null)
        {
            foreach (var d in deadUnity)
            {
                unityTargets.Remove(d);
            }   
        }
    }
}