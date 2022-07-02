using System.Collections.Generic;

namespace Engine.AssetLoading.AssimpIntegration;
/// <summary>
/// Generic Class for ASSIMP Animation data, I could have multiple structs for each, but this is more elegant and maintainable.
/// </summary>
/// <typeparam name="T">The type of data this struct is supposed to store in it's Keys</typeparam>
public struct AssimpGenericAnimData<T>
{
    public string Name;
    public KeyValuePair<double, T>[] Keys;
}