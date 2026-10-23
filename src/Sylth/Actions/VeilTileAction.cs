using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Sylth;

public class VeilTileAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public VeilTileAction(IntPtr ptr) : base(ptr) {}
    public VeilTileAction() {}
    public override bool IsValid(GameState state)
    {
        TileData tile = state.Map.GetTile(Coordinates);
        return !tile.HasEffect(Main.Veiled);
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("veiltileaction");
    }
    
    public override void Execute(GameState state)
    {
        TileData tile = state.Map.GetTile(Coordinates);

        tile.AddEffect(Main.Veiled);
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