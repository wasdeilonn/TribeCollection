using Polytopia.Data;
using Polibrary.PolyScript;
using Il2Gen = Il2CppSystem.Collections.Generic;
using M = TribeCollection.Main;

namespace TribeCollection.Ancients;
public class LightningStrikeAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public List<WorldCoordinates> ToAnimate = new();
    public LightningStrikeAction(IntPtr ptr) : base(ptr) {}
    public LightningStrikeAction() {}

    public LightningStrikeAction(byte playerId, WorldCoordinates coordinates) 
    : base(playerId)
    {
        base.PlayerId = playerId;
        Coordinates = coordinates;
    }
    
    public override bool IsValid(GameState state)
    {
        return true;
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("lightningstrikeaction");
    }
    
    public override void Execute(GameState state)
    {
        Il2Gen.List<TileData> rodNeighbors = state.Map.GetArea(Coordinates, 1, true, false);

        foreach (TileData rodNeighbor in rodNeighbors)
        {
            bool saveForAnim = false;

            if (rodNeighbor.unit != null && rodNeighbor.unit.HasAbility(Main.Capacitor) && rodNeighbor.unit.GetChargeCount() < rodNeighbor.unit.GetMaxCharge())
            {
                CAR.New(out ChargeAction action);
                action.PlayerId = PlayerId;
                action.Coordinates = rodNeighbor.unit.coordinates;
                action.Positive = true;
                state.ActionStack.Add(action);

                saveForAnim = true;
            }

            if (rodNeighbor != null && rodNeighbor.improvement != null)
            {
                ImprovementData rodNeighborData = state.GameLogicData.GetImprovementData(rodNeighbor.improvement.type);
                if (rodNeighborData.HasAbility(Main.PowerStorage))
                {
                    if (rodNeighbor.improvement.level < rodNeighborData.maxLevel)
                    {
                        if(rodNeighbor.effects == null)
                            rodNeighbor.effects = new();
                        rodNeighbor.effects.Add(Main.PowerStored);
                        ActionUtils.UpdateImprovementLevel(state, PlayerId, rodNeighbor); // cant do surrounding check cuz i dont have adjacency improvement
                        
                        saveForAnim = true;
                    }
                }
            }

            if (saveForAnim) ToAnimate.Add(rodNeighbor.coordinates);
        }
    }

    public override void Serialize(Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        writer.Write(PlayerId); //this line is important btw
        Coordinates.Serialize(writer, version);
        ToAnimate.Serialize(writer, version);
    }

    public override void Deserialize(Il2CppSystem.IO.BinaryReader reader, int version)
    {
        PlayerId = reader.ReadByte(); //leave this line in
        Coordinates.Deserialize(reader, version);
        ToAnimate.Deserialize(reader, version);
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