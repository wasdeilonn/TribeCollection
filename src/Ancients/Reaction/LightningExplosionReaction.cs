using Polibrary.PolyScript;
using TribeCollection.Ancients.Visual;
using Il2Gen = Il2CppSystem.Collections.Generic;
using M = TribeCollection.Main;

namespace TribeCollection.Ancients;
public class LightningExplosionReaction : PolibReactionBase
{
    protected LightningExplosionAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            LightningExplosionAction LightningExplosionAction = value.TryCast<LightningExplosionAction>();
            if (LightningExplosionAction != null)
            this.action = LightningExplosionAction;
        } 
    }
    public LightningExplosionReaction(IntPtr ptr) : base(ptr) {}
    public LightningExplosionReaction(LightningExplosionAction action)
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

            originTileInstance.SpawnLightning();

            AudioManager.PlaySFXAtTile(Main.LightningSFX, originTileData.coordinates);
            AudioManager.PlaySFXAtTile(SFXTypes.FireImpact, originTileData.coordinates);

            originTileInstance.SpawnDischargePuff();
            originTileInstance.SpawnAreaDamage();
        }

        Il2Gen.List<TileData> neighbors = GameManager.GameState.Map.GetArea(action.Coordinates, 1, true, false);

        foreach (TileData neighborData in neighbors)
        {
            if (neighborData == null) continue;

            Tile neighborInstance = neighborData.GetInstance();

            if (neighborInstance == null || neighborInstance.IsHidden) continue;

            neighborInstance.Render();

            GameManager.DelayCall(100, (Il2CppSystem.Action)(() =>
            {
                neighborInstance.Sway();
            }));
        }

        GameManager.DelayCall(0.2f, onComplete);
    }
}