using Polibrary.PolyScript;
using Il2Gen = Il2CppSystem.Collections.Generic;
using M = TribeCollection.Main;

namespace TribeCollection.Ancients;
public class LightningStrikeReaction : PolibReactionBase
{
    protected LightningStrikeAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            LightningStrikeAction LightningStrikeAction = value.TryCast<LightningStrikeAction>();
            if (LightningStrikeAction != null)
            this.action = LightningStrikeAction;
            else
            M.modLogger.LogInfo("shits fucked");
        } 
    }
    public LightningStrikeReaction(IntPtr ptr) : base(ptr) {}
    public LightningStrikeReaction(LightningStrikeAction action)
    {
        this.action = action;
    }

    public override bool ShouldFocusCamera()
    {
        return IsRecapOrOpponentAction(action);
    }

    public override WorldCoordinates GetCameraFocusCoordinates()
    {
        return action.Coordinates;
    }

    public override void Execute(Il2CppSystem.Action onComplete)
    {
        TileData originTileData = GameManager.GameState.Map.GetTile(action.Coordinates);
        Tile originTileInstance = originTileData.GetInstance();
        if (originTileInstance == null)
        {
            onComplete.Invoke();
            return;
        }

        if (originTileInstance != null && !originTileInstance.IsHidden)
        {
            originTileInstance.Render();
            originTileInstance.Sway();
            //lightning
            VFXManager.SizeMappings["lightning"] = 6f;
            VFXManager.FadeInOutAnimOverrideMappings["lightning"] = new UnityEngine.Vector2(0.1f, 1f);
            VFXManager.EnsureCustomPuffRegistered("Lightning", "Puff");
            originTileInstance.DoPuff("Lightning", originTileInstance.transform, originTileInstance.VisualCenterObject.localPosition);

            //audio
            AudioManager.PlaySFXAtTile(Main.LightningSFX, originTileData.coordinates);
            AudioManager.PlaySFXAtTile(SFXTypes.FireImpact, originTileData.coordinates);

            //explosion
            VFXManager.SizeMappings["dischargepuff"] = 2f;
            VFXManager.EnsureCustomPuffRegistered("DischargePuff", "Puff");
            originTileInstance.DoPuff("DischargePuff", originTileInstance.transform, originTileInstance.VisualCenterObject.localPosition);
        }

        foreach (WorldCoordinates coordinates in action.ToAnimate)
        {
            float delay = 0.1f;
            Tile toAnimate = MapRenderer.Current.GetTileInstance(coordinates);

            if (toAnimate != null && !toAnimate.IsHidden)
            {
                GameManager.DelayCall(delay, (Il2CppSystem.Action)delegate
                {
                    VFXManager.EnsureCustomPuffRegistered("ChargePuff", "Puff");
                    toAnimate.DoPuff("ChargePuff", toAnimate.transform, toAnimate.VisualCenterObject.localPosition);
                    toAnimate.SpawnShine();
                    AudioManager.PlaySFXAtTile(SFXTypes.Plop, coordinates);
                });

                delay += 0.1f;
            }
        }
        GameManager.DelayCall(200, onComplete);
    }
}