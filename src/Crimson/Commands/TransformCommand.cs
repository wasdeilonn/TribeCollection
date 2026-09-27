using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace TribeCollection.Crimson;
public class TransformCommand : PolibCommandBase
{
    public WorldCoordinates Coordinates; 
    public TransformCommand(IntPtr ptr) : base(ptr) {}
    public TransformCommand() {}
    public override void ExecuteNew(GameState state)
    {
        TransformAction action = PolibActionManager.MakeIl2CppAction<TransformAction>();
        action.PlayerId = PlayerId;
        action.Coordinates = Coordinates;
        state.ActionStack.Add(action);
    }
    public override bool IsValid(GameState state, out string validationError)
    {
        return PassesBasicValidation(state, out validationError);
    }
    public override void SerializeNew(Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        Coordinates.Serialize(writer, version);
    }

    public override void DeserializeNew(Il2CppSystem.IO.BinaryReader reader, int version)
    {
        Coordinates.Deserialize(reader, version);
    }
    
    public override CommandType GetCommandType()
    {
        CommandType type = EnumCache<CommandType>.GetType("transformcommand");
        return type;
    }

    public override string ToString()
    {
        return string.Format("{0} (PlayerId: {1}, Coordinates: {2})", new object[]
        {
            base.GetType(),
            base.PlayerId,
            this.Coordinates
        });
    }
}