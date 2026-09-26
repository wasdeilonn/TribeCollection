using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Crimson;

public class ImpactAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public ImpactAction(IntPtr ptr) : base(ptr) {}
    public ImpactAction() {}
    public override bool IsValid(GameState state)
    {
        return true;
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("impactaction");
    }
    
    public override void Execute(GameState state)
    {
        state.TryGetPlayer(PlayerId, out var playerState);

        TileData origin = state.Map.GetTile(Coordinates);

        foreach (TileData tile in state.Map.GetTileNeighbors(Coordinates))
        {
            if (tile.unit != null && tile.unit.owner != PlayerId && !tile.unit.HasActivePeaceTreaty(state, playerState))
            {
                BattleResults battleResults = BattleHelpers.GetBattleResults(state, origin.unit, tile.unit);
                state.ActionStack.Add(new AttackAction(PlayerId, Coordinates, tile.coordinates, battleResults.attackDamage / 2, shouldMoveToTarget: false, AttackAction.AnimationType.Splash, 20));
            }
        }
    }

    public override void Serialize(Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        writer.Write(PlayerId); //this line is important btw
        Coordinates.Serialize(writer, version);
    }

    public override void Deserialize(Il2CppSystem.IO.BinaryReader reader, int version)
    {
        PlayerId = reader.ReadByte(); //leave this line in
        Coordinates.Deserialize(reader, version);
    }

    public override string ToString()
    {
        return string.Format("{0} (PlayerId: {1}, Coordinates: {2})", new object[]
        {
            base.GetType(),
            base.PlayerId,
            Coordinates
        });
    }
}