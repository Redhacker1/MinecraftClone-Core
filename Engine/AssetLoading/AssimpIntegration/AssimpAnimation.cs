using System.Numerics;

namespace Engine.AssetLoading.AssimpIntegration;

public struct AssimpAnimation
{
    public double Duration;
    public string Name;
    public double TicksPerSecond;

    public AssimpGenericAnimData<Vector3>[] Channel;
    public AssimpGenericAnimData<uint>[] MeshChannels;
    public AssimpGenericAnimData<MeshMorphAnimKey>[] MeshMorphChannels;
}