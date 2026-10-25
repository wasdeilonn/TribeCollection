using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using UnityEngine.Scripting;

namespace TribeCollection.Sylth;

public class ChangeVisibilityAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public bool MakeInvisible = true;
    public ChangeVisibilityAction(IntPtr ptr) : base(ptr) {}
    public ChangeVisibilityAction() {}
    public override bool IsValid(GameState state)
    {
        TileData tile = state.Map.GetTile(Coordinates);

        return (tile.unit != null) && ((MakeInvisible && !tile.unit.HasEffect(UnitEffect.Invisible)) || (!MakeInvisible && tile.unit.HasEffect(UnitEffect.Invisible)));
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("changevisibilityaction");
    }
    
    public override void Execute(GameState state)
    {
        M.modLogger.LogInfo("executed");
        TileData tile = state.Map.GetTile(Coordinates);

        if (tile.unit == null) return;

        if (MakeInvisible)
        tile.unit.AddEffect(UnitEffect.Invisible);
        else
        tile.unit.RemoveEffect(UnitEffect.Invisible);
    }

    public override void Serialize(Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        writer.Write(PlayerId); //this line is important btw
        Coordinates.Serialize(writer, version);
        writer.Write(MakeInvisible);
    }

    public override void Deserialize(Il2CppSystem.IO.BinaryReader reader, int version)
    {
        PlayerId = reader.ReadByte(); //leave this line in
        Coordinates.Deserialize(reader, version);
        MakeInvisible = reader.ReadBoolean();
    }

    public override string ToString()
    {
        return string.Format("{0} (PlayerId: {1}, Coordinates: {2}, MakeInvisible: {3})", new object[]
        {
            base.GetType(),
            base.PlayerId,
            Coordinates,
            MakeInvisible
        });
    }
}