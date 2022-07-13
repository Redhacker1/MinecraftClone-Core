

using System.Numerics;
using Engine.Rendering.Abstract;

namespace Engine.Objects
{   /// <summary>
    /// Has a position, Location and Rotation, is used as the base for all entities in engine, and is cheap to create
    /// Not to mention, is decoupled from the engine, so almost no bookkeeping is necessary. The downside however is it
    /// it has no tick or update methods, which in some circumstances the trade off may be worth it.
    /// </summary>
    public class MinimalObject
    {

        public MinimalObject()
        {
            ModelMatrix = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position);
        }

        public MinimalObject(Vector3 position = default, Quaternion rotation = default, Vector3 scale = default)
        {
            lock (_locker)
            {
                ModelMatrix.Translation = position;
                _rotation = rotation;
                _scale = scale;
                ModelMatrix = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position);
            }
        }
        
        
        
        //TODO: use this transform as the base for these properties and edit them directly
        internal Matrix4x4 ModelMatrix;

        public Matrix4x4 GetTransform()
        {
            return ModelMatrix;
        }

        readonly object _locker = new object();
        Vector3 _scale = Vector3.One;
        public Vector3 Scale 
        {
            get => _scale;
            set
            {
                lock (_locker)
                {
                    _scale = value;
                    RegenMatrix4X4();   
                }
            }
        }
        
        Vector3 _pos = Vector3.Zero;
        public Vector3 Position {
            get => _pos;
            set
            {
                lock (_locker)
                {
                    _pos = value;
                    RegenMatrix4X4();   
                }
            }
        }

        Quaternion _rotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                lock (_locker)
                {
                    _rotation = value;
                    RegenMatrix4X4();   
                }
            }
        }
        
        void RegenMatrix4X4()
        {
            ModelMatrix = Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateTranslation(_pos);
            OnTransformUpdated();
        }

        protected virtual void OnTransformUpdated()
        {
        }
        
        /// <summary>
        ///  More efficient way of transforming the instance if you are changing most or every parameter, as
        ///  updates the Transform only once for the three variables.
        /// </summary>
        /// <param name="position">Position to set the transform to</param>
        /// <param name="rotation">Rotation to set the transform to</param>
        /// <param name="scale">The scale to set the transform to</param>
        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            lock (_locker)
            {
                _pos = position;
                _rotation = rotation;
                _scale = scale;
                RegenMatrix4X4();
            }
        }
        // Sets the transform of this object to be equal to that of another object
        public void SetTransform(MinimalObject minimalObject)
        {
            lock (_locker)
            {
                _pos = minimalObject._pos;
                _rotation = minimalObject._rotation;
                _scale = minimalObject._scale;
                RegenMatrix4X4();
            }
        }

        internal Matrix4x4 GetCameraSpaceTransform(ref CameraInfo camera)
        {
            Matrix4x4 cameraSpaceMatrix;
            lock (_locker)
            {
                cameraSpaceMatrix = ModelMatrix;
            }
            cameraSpaceMatrix.Translation = Position - camera.CameraPos;
            return cameraSpaceMatrix;
        }
        
    }
}