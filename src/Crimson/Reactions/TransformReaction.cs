using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Crimson;

public class TransformReaction : PolibReactionBase
{
    protected TransformAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            TransformAction polibActionBase = value.TryCast<TransformAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public TransformReaction(IntPtr ptr) : base(ptr) {}
    public TransformReaction(TransformAction action)
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
        Tile tile = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
        if (tile != null && !tile.IsHidden)
        {
            ExplodeUnitReaction.AnimateGrow(tile.Unit, (Il2CppSystem.Action) DoTransform);
            GameManager.DelayCall(0.4f, onComplete);
        }
        else
        {
            onComplete.Invoke();
        }

        void DoTransform()
        {
            tile.Render();
            tile.SpawnExplosion();
            tile.SpawnEmbers(0.5f);
            AudioManager.PlaySFXAtTile(SFXTypes.Parasite, tile.Coordinates);
            tile.Sway();
        }
    }
}