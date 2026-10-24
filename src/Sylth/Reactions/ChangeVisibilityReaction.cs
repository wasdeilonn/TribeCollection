using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Sylth;

public class ChangeVisibilityReaction : PolibReactionBase
{
    protected ChangeVisibilityAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            ChangeVisibilityAction polibActionBase = value.TryCast<ChangeVisibilityAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public ChangeVisibilityReaction(IntPtr ptr) : base(ptr) {}
    public ChangeVisibilityReaction(ChangeVisibilityAction action)
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
            instance.SpawnPuff();
            AudioManager.PlaySFXAtTile(SFXTypes.MoveSneak, action.Coordinates);
            instance.Render();
            onComplete.Invoke();
        }
        else
        {
            onComplete.Invoke();
        }
    }
}