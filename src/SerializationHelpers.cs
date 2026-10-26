using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Rendering.Universal;
using Il2Gen = Il2CppSystem.Collections.Generic;

namespace TribeCollection;

public static class SerializationHelpers
{
    public static void Serialize(this List<WorldCoordinates> list, Il2CppSystem.IO.BinaryWriter writer, int version)
    {
        writer.Write(list.Count);

        foreach (WorldCoordinates item in list)
        {
            item.Serialize(writer, version);
        }
    }

    public static void Deserialize(this List<WorldCoordinates> list, Il2CppSystem.IO.BinaryReader reader, int version)
    {
        int count = reader.ReadInt32();

        list = new List<WorldCoordinates>(count);

        foreach (WorldCoordinates item in list)
        {
            item.Deserialize(reader, version);
        }
    }
}
