using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using I2.Loc;

namespace TribeCollection.Crimson;

public class InfectAction : PolibActionBase
{
    public WorldCoordinates Origin;
    public WorldCoordinates Target;
    public InfectAction(IntPtr ptr) : base(ptr) {}
    public InfectAction() {}
    public override bool IsValid(GameState state)
    {
        return true;
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("infectaction");
    }
    
    public override void Execute(GameState state)
    {
        state.TryGetPlayer(PlayerId, out var playerState);

        TileData origin = state.Map.GetTile(Origin);
        TileData targetTile = state.Map.GetTile(Target);

        UnitState attacker = origin.unit;
        UnitState target = targetTile.unit;

        origin.unit = null;

        if (target != null)
        {
            target.owner = PlayerId;
            target.home = WorldCoordinates.NULL_COORDINATES;
            target.AddEffect(Main.Infected);

            if (target.HasFollower() && state.TryGetUnit(target.follower, out var follower) && follower.owner != base.PlayerId)
            {
                follower.owner = PlayerId;
                follower.home = WorldCoordinates.NULL_COORDINATES;
            }
            if (target.HasLeader() && state.TryGetUnit(target.leader, out var leader) && leader.owner != base.PlayerId)
            {
                leader.owner = PlayerId;
                leader.home = WorldCoordinates.NULL_COORDINATES;
            }
            if (target.passengerUnit != null)
            {
                target.passengerUnit.owner = PlayerId;
                target.passengerUnit.home = WorldCoordinates.NULL_COORDINATES;
            }
            if (state.GameLogicData.TryGetData(target.type, out var data))
            {
                ActionUtils.ExploreFromTile(state, playerState, targetTile, data.GetSightRange(), shouldUseActions: true);
            }
        }
    }

    public override void Serialize(Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        writer.Write(PlayerId); //this line is important btw
        Origin.Serialize(writer, version);
        Target.Serialize(writer, version);
    }

    public override void Deserialize(Il2CppSystem.IO.BinaryReader reader, int version)
    {
        PlayerId = reader.ReadByte(); //leave this line in
        Origin.Deserialize(reader, version);
        Target.Deserialize(reader, version);
    }

    public override string ToString()
    {
        return string.Format("{0} (PlayerId: {1}, Origin: {2}, Target: {3})", new object[]
        {
            base.GetType(),
            base.PlayerId,
            Origin,
            Target
        });
    }
}