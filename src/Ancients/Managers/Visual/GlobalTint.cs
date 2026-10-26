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
 
    // --- global settings ---
    public static bool Enabled = true;
    public static Color From = Color.white;
    public static Color To = new Color(0.3f, 0.6f, 1f, 1f);
    public static float Duration = 1.5f;   // seconds per direction
    static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    static readonly int OverlayColorId = Shader.PropertyToID("_OverlayColor");
    static readonly int OverlayStrengthId = Shader.PropertyToID("_OverlayStrength");
 
    // target -> original color (restored when toggled off / unregistered)
    static readonly Dictionary<PolytopiaSpriteRenderer, Color> polyTargets =
        new Dictionary<PolytopiaSpriteRenderer, Color>();
    static readonly Dictionary<SpriteRenderer, Color> unityTargets =
        new Dictionary<SpriteRenderer, Color>();
 
    static bool wasEnabled = true;
 
    // ---------- PolytopiaSpriteRenderer ----------
    public static void Register(PolytopiaSpriteRenderer r)
    {
        if (r != null && !polyTargets.ContainsKey(r))
            polyTargets[r] = r.Color;              // <- adjust to the real property
    }
 
    public static void Unregister(PolytopiaSpriteRenderer r)
    {
        if (r != null && polyTargets.TryGetValue(r, out var orig))
        {
            r.Color = orig;                        // <- adjust to the real property
            polyTargets.Remove(r);
        }
    }
 
    // ---------- normal SpriteRenderer ----------
    public static void Register(SpriteRenderer r)
    {
        if (r != null && !unityTargets.ContainsKey(r))
            unityTargets[r] = r.color;
    }

    static void ApplyOverlay(SpriteRenderer r, Color c, float strength)
    {
        r.GetPropertyBlock(block);          // keeps whatever the game already set
        block.SetColor(OverlayColorId, c);
        block.SetFloat(OverlayStrengthId, strength);
        r.SetPropertyBlock(block);
    }

    public static void Unregister(SpriteRenderer r)
    {
        if (r != null && unityTargets.Remove(r))
            ApplyOverlay(r, To, 0f);        // overlay off
    }
 
    public static void Toggle() => Enabled = !Enabled;
 
    void Update()
    {
        if (!Enabled)
        {
            // restore everything once, then idle
            if (wasEnabled)
            {
                foreach (var kv in polyTargets) if (kv.Key != null) kv.Key.Color = kv.Value; // <- adjust
                foreach (var kv in unityTargets) if (kv.Key != null) ApplyOverlay(kv.Key, To, 0f);
            }
            wasEnabled = false;
            return;
        }
        wasEnabled = true;
 
        // one shared value for everyone, so all sprites stay in phase
        float k = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / Duration, 1f));
        Color c = Color.Lerp(From, To, k);
 
        // apply + prune destroyed targets
        List<PolytopiaSpriteRenderer> deadPoly = null;
        foreach (var kv in polyTargets)
        {
            if (kv.Key == null) { (deadPoly ??= new List<PolytopiaSpriteRenderer>()).Add(kv.Key); continue; }
            kv.Key.Color = c;                      // <- adjust
        }
        if (deadPoly != null) foreach (var d in deadPoly) polyTargets.Remove(d);
 
        List<SpriteRenderer> deadUnity = null;
        foreach (var kv in unityTargets)
        {
            if (kv.Key == null) { (deadUnity ??= new List<SpriteRenderer>()).Add(kv.Key); continue; }
            ApplyOverlay(kv.Key, To, k * 0.6f);   // fades 0 -> 0.6 -> 0, using the shared k
        }
        if (deadUnity != null) foreach (var d in deadUnity) unityTargets.Remove(d);
    }
}
