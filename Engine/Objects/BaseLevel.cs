﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Engine.Collision.BEPUPhysics;
using Engine.Collision.BEPUPhysics.Implementation;
using Engine.Collision.Simple;
using Engine.Rendering.Abstract;
using Engine.Utilities.Concurrency;

namespace Engine.Objects;

/// <summary>
/// Terribly unoptimized basic level class
/// </summary>
public class BaseLevel : EngineObject,  ILevelContract
{
    public PhysicsWorld PhysicsWorld;
    
    public const double FixedStepTime = 0.01666666;

    readonly ThreadSafeList<EngineObject> _levelObjects = new ThreadSafeList<EngineObject>();
    void ILevelContract.EntityTransformUpdated(EngineObject engineObject)
    {
        
    }


    internal override void OnLevelTick(double deltaT)
    {
        if (BaseLevel != null)
        {
            base.OnLevelTick(deltaT);
            ((ILevelContract) this).OnTick(deltaT);   
        }
    }

    double physicsDelta;
    void ILevelContract.OnTick(double deltaT)
    {

        physicsDelta += deltaT;
        bool physicsProcess = physicsDelta >= FixedStepTime;

        ImmutableArray<EngineObject> objectSnapshot = _levelObjects.ToImmutableArray();

        FrameUpdate(deltaT, objectSnapshot);
        if (physicsProcess)
        {
            PhysicsWorld.Simulate((float)physicsDelta);
            FixedUpdate(FixedStepTime, objectSnapshot);
            physicsDelta -= FixedStepTime;
            //Clean up empty objects
            Task.Run(Clean);
        }
    }

    protected override void OnChildRemoved(EngineObject removedChild)
    {
        RemoveEntityFromLevel(removedChild);
    }

    protected override void OnChildAdded(EngineObject engineObject)
    {
        AddEntityToLevel(engineObject);
    }

    internal void Clean()
    {
        for (int index = 0; index < _levelObjects.Count;)
        {
            if (_levelObjects[index] == null || _levelObjects[index].Freed)
            {
                _levelObjects[index] = _levelObjects[^1];
                _levelObjects.RemoveAt(_levelObjects.Count - 1);
            }
            else
            {
                index++;
            }
        }
    }

    protected void FrameUpdate(double delta, ImmutableArray<EngineObject> objectSnapshot)
    {
        OnLevelTick(delta);   
        foreach (EngineObject engineObject in objectSnapshot)
        {
            if (engineObject?.Ticks == true)
            {
                engineObject.OnLevelTick(delta);   
            }
        }
    }

    protected void FixedUpdate(double delta, ImmutableArray<EngineObject> objectSnapshot)
    {

        _PhysicsProcess(delta);
        foreach (EngineObject engineObject in objectSnapshot)
        {
            if (engineObject?.PhysicsTick == true)
            {
                engineObject._PhysicsProcess(delta);   
            }
        }
    }

    public void AddEntityToLevel(EngineObject entity)
    {
        if (entity != null && _levelObjects.Contains(entity) == false)
        {
            _levelObjects.Add(entity);
            entity.BaseLevel = this;

            foreach (EngineObject child in entity.Children)
            {
                AddEntityToLevel(child);
                child.BaseLevel = this;
            }
        }
    }

    public void RemoveEntityFromLevel(EngineObject entity)
    {
        if (entity == null) return;
        
        _levelObjects.Remove(entity);
        entity.BaseLevel = null;
        for (int index = 0; index < entity.Children.Count ; index++)
        {
            RemoveEntityFromLevel(Children[index]);
            Children[index].BaseLevel = null;
        }
        
        // Occasionally do some housekeeping
            if (_levelObjects.Capacity > _levelObjects.Count * 1.5)
        {
            _levelObjects.TrimExcess();
        }
    }

    ImmutableArray<EngineObject> ILevelContract.GetPotentiallyVisibleInCamera(ref CameraInfo info)
    {
        return _levelObjects.ToImmutableArray();
    }

    void ILevelContract.OnLevelLoad()
    {
        PhysicsWorld = PhysicsWorld.Create(new DefaultNarrowPhase(), new DefaultPoseIntegrator());
        _Ready();
    }

    void ILevelContract.OnLevelUnload()
    {
        OnFree();
    }

    public IReadOnlyList<EngineObject> GetEntitiesInBounds(AABB bounds)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<EngineObject> GetOnCriteria(Predicate<EngineObject> criteria)
    {
        throw new NotImplementedException();
    }
}