using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Engine.Collision.Simple;
using Engine.Rendering.Abstract;

namespace Engine.Objects;

/// <summary>
/// I tried to avoid this, but OOP has forced my hand.
/// </summary>
public interface ILevelContract
{
    
    protected internal void EntityTransformUpdated(EngineObject engineObject);
    protected internal void OnTick(double delta);

    public void AddEntityToLevel(EngineObject entity);

    public void RemoveEntityFromLevel(EngineObject entity);

    protected internal ImmutableArray<EngineObject> GetPotentiallyVisibleInCamera(ref CameraInfo info);

    protected internal void OnLevelLoad();

    protected internal void OnLevelUnload();
    
    // Investigate the use of IReadOnlyList, and how much it could bring down performance
    public IReadOnlyList<EngineObject> GetEntitiesInBounds(AABB bounds);

    // Investigate the use of IReadOnlyList, and how much it could bring down performance
    public IReadOnlyList<EngineObject> GetOnCriteria(Predicate<EngineObject> criteria);

}