using System;
using System.Numerics;
using Engine.Collision.BEPUPhysics.Implementation;
using Engine.MathLib;
using Engine.Rendering.Abstract;
using Engine.Utilities.Concurrency;

namespace Engine.Objects
{
    /// <summary>
    /// Has a position, Location and Rotation, is used as the base for all entities in engine
    /// </summary>
    public class EngineObject
    {
        
        public bool PhysicsTick = false;
        public bool Ticks = false;

        public PhysicsBody Body { get; set; }
        

        protected EngineObject(Transform transform, EngineObject parent = null) 
        {
            _parent = parent;
            lock (_locker)
            {
                LocalTransform = transform;
            }
        }

        protected EngineObject(EngineObject parent = null) : this(new Transform(), parent)
        {
        }

        public bool Freed;
        internal void Free()
        {
            Freed = true;
            OnFree();
        }


        ~EngineObject()
        {
            if (Freed == false)
            {
                OnFree();
            }
        }

        readonly object _locker = new object();
        public Transform LocalTransform;
        
        /// <summary>
        /// The position, location rotation of the object in World space.
        /// </summary>
        protected Transform WorldTransform
        {
            get
            {
                if (Parent == null) return LocalTransform;
                return Parent.WorldTransform + LocalTransform;
            }
        }
        
        public Vector3 Scale 
        {
            get => LocalTransform.Scale;
            set
            {
                lock (_locker)
                {
                    LocalTransform.Scale = value;
                    OnTransformUpdated();
                }
            }
        }
        
        public Vector3 Position {
            get => LocalTransform.Position;
            set
            {
                lock (_locker)
                {
                    LocalTransform.Position = value;
                    OnTransformUpdated();
                }
            }
        }
        
        public Quaternion Rotation
        {
            get => LocalTransform.Rotation;
            set
            {
                lock (_locker)
                {
                    LocalTransform.Rotation = value;
                    OnTransformUpdated();
                }
            }
        }
        
        /// <summary>
        ///  More efficient way of transforming the instance if you are changing most or every parameter, as
        ///  updates the Transform only once for the three variables.
        /// </summary>
        public void SetTransform(Transform transform)
        {
            lock (_locker)
            {
                LocalTransform = transform;
                OnTransformUpdated();
            }
        }
        
        // Sets the transform of this object to be equal to that of another object
        public void SetTransform(EngineObject engineObject)
        {
            lock (_locker)
            {
                LocalTransform = engineObject.LocalTransform;
                OnTransformUpdated();
            }
        }

        internal Transform GetCameraSpaceTransform(ref CameraInfo camera)
        {
            Transform cameraSpaceMatrix;
            lock (_locker)
            {
                cameraSpaceMatrix = WorldTransform;
            }
            cameraSpaceMatrix.Position = Position - camera.CameraTransform.Position;
            return cameraSpaceMatrix;
        }
        

        

        public ILevelContract BaseLevel;

        public void AddChild(EngineObject engineObject)
        {
            BaseLevel?.AddEntityToLevel(engineObject);
            OnChildAdded(engineObject);
            Children.Add(engineObject);   
        }


        protected void RemoveChild(EngineObject engineObject)
        {
            BaseLevel?.RemoveEntityFromLevel(engineObject);
            Children.Remove(engineObject);
            OnChildRemoved(engineObject);
        }
        
        EngineObject _parent;
        
        public EngineObject Parent
        {
            get => _parent;
            set
            {
                OnChangedParent(value);
                _parent.RemoveChild(this);
                _parent = value;
 
            }
        }
        
        protected internal readonly ThreadSafeList<EngineObject> Children = new ThreadSafeList<EngineObject>();



        internal virtual void OnLevelTick(double deltaT)
        {
            if (Body?.IsValidBody == true)
            {
                SetTransform(Body._transform);
                Console.WriteLine(Body._transform.Position);
            }
            else if (Body != null)
            {
                Console.WriteLine("Body is not registered, but also not null!");
            }
            _Process(deltaT);
        }


        
        /// <summary>
        /// Is called when the transform is updated
        /// </summary>
        protected virtual void OnTransformUpdated()
        {
        }
        
        /// <summary>
        /// Entity has been removed from the parent 
        /// </summary>
        /// <param name="removedChild"></param>
        protected virtual void OnChildRemoved(EngineObject removedChild) 
        {
            
        }
        
        protected virtual void OnChildAdded(EngineObject engineObject)
        {
            
        }
        
        /// <summary>
        /// Runs just before changing the previous parent.
        /// </summary>
        /// <param name="parent">The new entity to be parented to</param>
        protected virtual void OnChangedParent(EngineObject parent)
        {
        }
        
        
        /// <summary>
        /// To be run after you run the constructor, when you want the entity to be recreated.
        /// </summary>
        [Obsolete("(For now and for the forseeable future) This is no longer called automatically, it might find a use in networking, but for the moment, it is practically useless call manually when ready")]
        public virtual void _Ready()
        {
        }
        
        /// <summary>
        ///  Runs every frame without exception
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        protected virtual void _Process(double delta)
        {
        }
        
        /// <summary>
        /// Runs at the physics framerate
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        protected internal virtual void _PhysicsProcess(double delta)
        {
        }
        /// <summary>
        /// Called just before freeing an object
        /// </summary>
        protected virtual void OnFree()
        {
        }


    }
}