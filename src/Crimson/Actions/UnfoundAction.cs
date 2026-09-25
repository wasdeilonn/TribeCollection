using Polibrary.PolyScript;
using M = TribeCollection.Main;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection.Crimson;

public class UnfoundAction : PolibActionBase
{
    public WorldCoordinates Coordinates;
    public UnfoundAction(IntPtr ptr) : base(ptr) {}
    public UnfoundAction() {}
    public override bool IsValid(GameState state)
    {
        return true;
    }

    public override ActionType GetActionType()
    {
        return EnumCache<ActionType>.GetType("unfoundaction");
    }
    
    public override void Execute(GameState state)
    {
        TileData tile = state.Map.GetTile(Coordinates);
        byte previousOwnerId = tile.owner;
        if (previousOwnerId == 0) return;

        state.TryGetPlayer(previousOwnerId, out var playerState);

        if (playerState != null)
        {
            playerState.cities--;
        }

        for (int i = 0; i < state.Map.Tiles.Length; i++)
        {
            TileData tileData = state.Map.Tiles[i];
            if (tileData.unit != null && tileData.unit.home == Coordinates)
            {
                tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
            }
        }

        Il2Gen.List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
        List<ActionBase> scoreActions = new List<ActionBase>();

        for (int j = 0; j < cityAreaSorted.Count; j++)
        {
            TileData territoryTile = cityAreaSorted[j];

            if (territoryTile.owner == previousOwnerId)
            {
                int scoreValue = ScoreSheet.tileValue;
                if (territoryTile.improvement != null && territoryTile.coordinates != tile.coordinates)
                {
                    scoreValue += state.CalculateImprovementScore(territoryTile);
                }

                scoreActions.Add(new DecreaseScoreAction(previousOwnerId, scoreValue));

                territoryTile.owner = 0;
                territoryTile.rulingCityCoordinates = WorldCoordinates.NULL_COORDINATES;
            }
        }

        int cityCenterScore = state.CalculateImprovementScore(tile);
        ActionUtils.AddScore(state, playerState, -cityCenterScore);
        foreach (ActionBase ab in scoreActions)
        {
            state.ActionStack.Add(ab);
        }

        Polibrary.PolibUtils.RemoveEmbassies(state, tile);

        tile.owner = 0;
        
        tile.improvement.level = 1;
        
        if (playerState != null && !playerState.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
        {
            state.ActionStack.Add(new WipePlayerAction(PlayerId, previousOwnerId));
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
            this.Coordinates
        });
    }
}