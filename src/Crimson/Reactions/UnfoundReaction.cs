using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Crimson;

public class UnfoundReaction : PolibReactionBase
{
    protected UnfoundAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            UnfoundAction polibActionBase = value.TryCast<UnfoundAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public UnfoundReaction(IntPtr ptr) : base(ptr) {}
    public UnfoundReaction(UnfoundAction action)
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
        TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
        Tile instance = tile.GetInstance();

        if (instance != null && !instance.IsHidden)
        {
            instance.Sway();
            instance.SpawnDarkPuff();

            foreach (TileData areatile in GameManager.GameState.Map.GetArea(action.Coordinates, 3, true))
            {
                if (areatile != null && !areatile.GetInstance().IsHidden)
                {
                    areatile.GetInstance().Render();
                }
            }

            onComplete.Invoke();
        }
        else
        {
            onComplete.Invoke();
        }
    }
}