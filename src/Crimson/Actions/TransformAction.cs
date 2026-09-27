using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using I2.Loc;
using Polytopia.Data;

namespace TribeCollection.Crimson;

public class TransformAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public TransformAction(IntPtr ptr) : base(ptr) {}
    public TransformAction() {}
    public override bool IsValid(GameState state)
    {
        return true;
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("transformaction");
    }
    
    public override void Execute(GameState state)
    {
        TileData tile = state.Map.GetTile(Coordinates);
        state.TryGetPlayer(PlayerId, out var playerState);
        state.GameLogicData.TryGetData(EnumCache<UnitData.Type>.GetType("chrymeavessel_unit"), out var data);

        tile.unit = ActionUtils.TrainUnit(state, playerState, tile, data);
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