using System;
using System.Collections.Generic;
using Engine.MathLib.DoublePrecision_Numerics;

namespace Engine.Objects
{
    /// <summary>
    /// Base class for anything that interacts with the world, has a Free() method like godot, but internally implements IDisposable to allow for both using statements and and to keep an accurate tally from what remains, unlike godot, this can be Garbage collected safely, so entities with no references will eventually be removed
    /// </summary>
    public class GameObject : MinimalObject, IDisposable
    {
        public static List<GameObject> Objects = new List<GameObject>();

        public GameObject()
        {
            Objects.Add(this); 
            _Ready();
        }
        
        
        /// <summary>
        /// Runs on initialization
        /// </summary>
        protected virtual void _Ready()
        {
        }
        
        /// <summary>
        ///  Runs every frame without exception
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        public virtual void _Process(float delta)
        {
            //Console.WriteLine("Running");
        }
        
        /// <summary>
        /// Runs at the physics framerate
        /// </summary>
        /// <param name="delta">Time elapsed since last call in milliseconds</param>
        public virtual void _PhysicsProcess(float delta)
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
            Dispose();
        }

        ~GameObject()
        {
            Free();
        }

        public void Dispose()
        {
            OnFree();
            Objects.Remove(this);
        }
    }
}