using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using TribeCollection.Crimson.Visual;
using Polibrary;

namespace TribeCollection.Crimson;

public class InfectReaction : PolibReactionBase
{
    protected InfectAction action;
    public override ActionBase actionProperty 
    { 
        get => this.action; 
        set
        {
            InfectAction polibActionBase = value.TryCast<InfectAction>();
            if (polibActionBase != null)
            this.action = polibActionBase;
        } 
    }
    public InfectReaction(IntPtr ptr) : base(ptr) {}
    public InfectReaction(InfectAction action)
    {
        this.action = action;
    }

    public override bool ShouldFocusCamera()
    {
        return IsRecapOrOpponentAction(action);
    }

    public override WorldCoordinates GetCameraFocusCoordinates()
    {
        return action.Target;
    }

    public override void Execute(Il2CppSystem.Action onComplete)
    {
        Tile origin = GameManager.GameState.Map.GetTile(action.Origin).GetInstance();
        Tile target = GameManager.GameState.Map.GetTile(action.Target).GetInstance();

        if (!origin.IsHidden && origin != null)
        {
            origin.Unit.AnimatePathMoveJump
            (
                new List<WorldCoordinates>()
                {
                    action.Target,
                    action.Origin
                }.ToIl2List(),
                Jump.DURATION,
                (Il2CppSystem.Action) delegate
                {
                    CompletePath(onComplete, origin, target);
                }
            );
            origin.Unit.swayTween = Jump.JumpSway(origin.Unit.spriteContainer);
        }
        else onComplete.Invoke();
    }

    public void CompletePath(Il2CppSystem.Action onComplete, Tile origin, Tile target)
    {
        origin.Render();
        if (!target.IsHidden && origin != null)
        {
            target.Render();
            target.SpawnExplosion();
            target.Sway();
        }
        onComplete.Invoke();
    }
}