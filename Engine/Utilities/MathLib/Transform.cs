using System.Numerics;

namespace Engine.MathLib;

public struct Transform
{
   public  Vector3 Position;
   public Quaternion Rotation;
   public Vector3 Scale;

   public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
   {
      Position = position;
      Rotation = rotation;
      Scale = scale;
   }
   
   public Transform()
   {
      Position = Vector3.Zero;
      Rotation = Quaternion.Identity;
      Scale = Vector3.One;
   }

   public static void Compose(in Transform inTransform, out Matrix4x4 outMatrix)
   {
      outMatrix = Matrix4x4.CreateFromQuaternion(inTransform.Rotation) * Matrix4x4.CreateScale(inTransform.Scale) * Matrix4x4.CreateTranslation(inTransform.Position);
   }

   public static void Decompose(in Matrix4x4 inMatrix, out Transform transform)
   {
      transform = new Transform();
      Matrix4x4.Decompose(inMatrix, out transform.Scale, out transform.Rotation, out transform.Position);
   }

   public Matrix4x4 AsMatrix4X4()
   {
      Compose(in this, out Matrix4x4 mat4);
      return mat4;
   }

   public static Transform operator +(Transform left, Transform right)
   {
      Transform transform = left;
      transform.Position += right.Position;
      transform.Rotation += right.Rotation;
      transform.Scale += right.Scale;
      return transform;

   }
   
   public static Transform operator -(Transform left, Transform right)
   {
      Transform transform = left;
      transform.Position -= right.Position;
      transform.Rotation -= right.Rotation;
      transform.Scale -= right.Scale;
      return transform;

   }
}