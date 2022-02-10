using System;
using System.Collections.Generic;

namespace Engine.Objects
{
    /// <summary>
    /// Base class for anything that interacts with the world, has a Free() method like godot, but internally implements IDisposable to allow for both using statements and and to keep an accurate tally from what remains, unlike godot, this can be Garbage collected safely, so entities with no references will eventually be removed
    /// </summary>
    public class GameObject : MinimalObject, IDisposable
    {
        public static List<WeakReference<GameObject>> Objects = new List<WeakReference<GameObject>>();
        internal bool Started = false;
        public bool PhysicsTick = false;
        public bool Ticks = false;
        internal bool cleanup;
        bool Freed = false;
        
        WeakReference<GameObject> ownWeakRef;

        public GameObject()
        {
            ownWeakRef = new WeakReference<GameObject>((this));
            Objects.Add(ownWeakRef); 
        }
        
        
        /// <summary>
        /// Runs on initialization
        /// </summary>
        public virtual void _Ready()
        {
        }

        /// <summary>
        ///  Runs every frame without exception
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        public virtual void _Process(double delta)
        {
            //Console.WriteLine("Running");
        }

        /// <summary>
        /// Runs at the physics framerate
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        public virtual void _PhysicsProcess(double delta)
        {
                    
        }

        /// <summary>
        /// Called just before freeing an object
        /// </summary>
        protected virtual void OnFree()
        {
            
        }

        /// <summary>
        ///  Removes the object from the world
        /// </summary>
        public void Free()
        {
            cleanup = true;
        }

        public void Dispose()
        {
            Freed = true;
            OnFree();
            Objects.Remove(ownWeakRef);
        }

        ~GameObject()
        {
            if (Freed == false)
            {
                Console.WriteLine("Object Leak!");
                OnFree();
                Objects.Remove(ownWeakRef);   
            }
        }
    }
}