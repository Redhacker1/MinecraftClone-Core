using System.Collections.Generic;
using System.Numerics;
using Engine.MathLib;
using Engine.Rendering.Abstract;

namespace Engine.Objects
{   /// <summary>
    /// Has a position, Location and Rotation, is used as the base for all entities in engine, and is cheap to create
    /// Not to mention, is decoupled from the engine, so almost no bookkeeping is necessary. The downside however is it
    /// it has no tick or update methods, which in some circumstances the trade off may be worth it.
    /// </summary>
    public class EngineObject
    {
        readonly object _locker = new object();
        
    #region Game loop properties

            internal bool Started = false;
            public bool PhysicsTick = false;
            public bool Ticks = false;
            
    #endregion


    #region Creation and Cleanup

    
        public EngineObject(EngineObject parent = null, Transform transform = new Transform())
        {
            _parent = parent;
            lock (_locker)
            {
                LocalTransform = transform;
            }
        }
    
        public bool Freed;
        internal void Free()
        {
            Freed = true;
            OnFree();
        }
        
        /// <summary>
        ///  Removes the object from the world on the next available update on the main thread.
        /// </summary>
        public void FreeSynchronous()
        {
            _parent?.RemoveChild(this);
            EngineFree = true;
        }

        public bool EngineFree { get; set; }

        ~EngineObject()
        {
            if (Freed == false)
            {
                OnFree();
            }
        }

    #endregion
        

        
    #region Engine Transform Properties and Methods

    public Transform LocalTransform;
        public Transform WorldTransform
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
        
#endregion


    #region Parent Child Hierarchy

    public void AddChild(EngineObject engineObject)
        {
            Children.Add(engineObject);
        }

    public void RemoveChild(EngineObject engineObject)
        {
            Children.Remove(engineObject);
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
        
        protected readonly List<EngineObject> Children = new List<EngineObject>();

#endregion





    #region User Overridable Functions
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
        
        /// <summary>
        /// Runs just before changing the previous parent.
        /// </summary>
        /// <param name="parent">The new entity to be parented to</param>
        protected virtual void OnChangedParent(EngineObject parent)
        {
        }
        
        
        /// <summary>
        /// Runs on initialization
        /// </summary>
        protected internal virtual void _Ready()
        {
        }
        
        /// <summary>
        ///  Runs every frame without exception
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        protected internal virtual void _Process(double delta)
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
        protected internal virtual void OnFree()
        {
        }
        #endregion


    }
}