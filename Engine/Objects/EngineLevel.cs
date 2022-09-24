using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Engine.Collision.BEPUPhysics;
using Engine.Collision.Simple;

namespace Engine.Objects;

public class EngineLevel : EngineObject
{
    PhysicsWorld World;
    internal bool LevelDestroy;

    public virtual float FixedStepTIme { get; protected set; } = 0.0166666f;

    /// <summary>
    /// Level Initialization code goes here!
    /// </summary>
    protected internal virtual void OnLevelLoaded()
    {
        
    }
    
    /// <summary>
    /// Level Destruction code goes here!
    /// </summary>
    public void OnLevelDestroyed()
    {
        
    }


    double physicsDelta;

    protected internal override void _Process(double delta)
    {
        if(LevelDestroy) return;
        
        physicsDelta += delta;
        bool physicsProcess = physicsDelta >= FixedStepTIme;

        ImmutableList<EngineObject> objectSnapshot = Children.ToImmutableList();

        FrameUpdate(delta, objectSnapshot);
        if (physicsProcess)
        {
            FixedUpdate(physicsDelta, objectSnapshot);
            physicsDelta = 0;
        }
        Cleanup(Children);
        
    }


    protected void Cleanup(List<EngineObject> engineObjects)
    {
        for (int i = 0; i < engineObjects.Count; ++i)
        {
            EngineObject engineObject = engineObjects[i];
            if (engineObject.EngineFree)
            {
                engineObject.OnFree();
                engineObjects[i] = engineObjects[^1];
                engineObjects.RemoveAt(engineObjects.Count - 1);

            }
        }

        if ((float)engineObjects.Capacity / engineObjects.Count > .6)
        {
            engineObjects.TrimExcess();
        }
    }
    

    protected virtual void FrameUpdate(double delta, ImmutableList<EngineObject> entities)
    {
        foreach (EngineObject entity in entities)
        {
            bool success = entity != null;

            if (success)
            {
                if (entity.Started != true)
                {
                    entity._Ready();
                    entity.Started = true;
                }
                if (entity.Ticks)
                {
                    entity._Process(delta);
                }
            }
        }
    }
    
    protected virtual void FixedUpdate(double delta, ImmutableList<EngineObject> entities)
    {
        foreach (EngineObject entity in entities)
        {
            bool success = entity != null;
            if (success)
            {
                if (entity.PhysicsTick)
                {
                    // TODO: Implement physics sub-stepping when on low frame rates
                    entity.OnPhysicsTick((float)physicsDelta);
                }
            }
        }
    }

    public virtual List<AABB> GetAabbs(int collisionlayer, Entity entity)
    {
        throw new System.NotImplementedException();
    }
}