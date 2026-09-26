using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Crimson;

public class ImpactReaction : PolibReactionBase
{
    protected ImpactAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            ImpactAction polibActionBase = value.TryCast<ImpactAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public ImpactReaction(IntPtr ptr) : base(ptr) {}
    public ImpactReaction(ImpactAction action)
    {
        this.action = action;
    }

    public override bool ShouldFocusCamera()
    {
        return IsRecapOrOpponentAction(action);
    }

    public override WorldCoordinates GetCameraFocusCoordinates()
    {
        GameManager.GameState.TryGetPlayer(action.PlayerId, out var player);
        return player.GetCurrentCapitalCoordinates(GameManager.GameState);
    }

    public override void Execute(Il2CppSystem.Action onComplete)
    {
        Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
        if (instance != null && !instance.IsHidden)
        {
            instance.Sway();
            instance.SpawnAreaDamage();
            AudioManager.PlaySFXAtTile(SFXTypes.FireImpact, action.Coordinates);

            foreach (TileData neighborData in GameManager.GameState.Map.GetTileNeighbors(action.Coordinates))
            {
                if (neighborData != null && !neighborData.GetInstance().IsHidden)
                {
                    Tile neighbor = neighborData.GetInstance();
                    neighbor.Sway(0.1f);
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