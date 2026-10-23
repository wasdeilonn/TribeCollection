using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Sylth;

public class UnveilTileReaction : PolibReactionBase
{
    protected UnveilTileAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            UnveilTileAction polibActionBase = value.TryCast<UnveilTileAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public UnveilTileReaction(IntPtr ptr) : base(ptr) {}
    public UnveilTileReaction(UnveilTileAction action)
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
            instance.Render();

            onComplete.Invoke();
        }
        else
        {
            onComplete.Invoke();
        }
    }
}