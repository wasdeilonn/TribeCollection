using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Sylth;

public class VeilTileReaction : PolibReactionBase
{
    protected VeilTileAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            VeilTileAction polibActionBase = value.TryCast<VeilTileAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public VeilTileReaction(IntPtr ptr) : base(ptr) {}
    public VeilTileReaction(VeilTileAction action)
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
        Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
        if (instance != null && !instance.IsHidden)
        {
            instance.Sway();
            instance.Render();

            GameManager.DelayCall(0.02f, onComplete);
        }
        else
        {
            onComplete.Invoke();
        }
    }
}