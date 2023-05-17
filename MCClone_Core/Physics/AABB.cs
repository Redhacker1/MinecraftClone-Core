using System.Numerics;
using System.Runtime.CompilerServices;

namespace MCClone_Core.Physics
{
  public struct AABB
  {
      const float Epsilon = 0.01f;
      public Vector3 Origin;
      public Vector3 Extents;
  
      public AABB(Vector3 origin, Vector3 extents)
      {
  
          Origin = origin;
          Extents = extents;
      }
  
      public void SetExtents(Vector3 extents)
      {
          Extents = extents;
      }
      
      public AABB Expand(Vector3 size)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        
        Vector3 minLoc = MinLoc;
        Vector3 maxLoc = MaxLoc;
        if (size.X < 0.01)
          minLoc.X += size.X;
        if (size.X > 0.01)
          maxLoc.X += size.X;
        if (size.Y < 0.01)
          minLoc.Y += size.Y;
        if (size.Y > 0.01)
          maxLoc.Y += size.Y;
        if (size.Z < 0.01)
          minLoc.Z += size.Z;
        if (size.Z > 0.01)
          maxLoc.Z += size.Z;

        var aabb = new AABB();
        aabb.SetAABB(minLoc, maxLoc);
        return aabb;
      }
      
      
      public float ClipXCollide(AABB c, float xa)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        c.GetMinMax(out Vector3 cMinLoc, out Vector3 cMaxLoc);


        if (cMaxLoc.Y < MinLoc.Y || cMinLoc.Y >= MaxLoc.Y || cMaxLoc.Z < MinLoc.Z ||
            cMinLoc.Z >= MaxLoc.Z || cMaxLoc.Y - MinLoc.Y <= 1.40129846432482E-45 ||
            cMinLoc.Y - MaxLoc.Y >= 1.40129846432482E-45 || cMaxLoc.Z - MinLoc.Z <= 1.40129846432482E-45 ||
            cMinLoc.Z - MaxLoc.Z >= 1.40129846432482E-45)
          return xa;
        float num;
        if (xa > 0.01 && cMaxLoc.X < MinLoc.X && (num = MinLoc.X - cMaxLoc.X - 0.01f) < xa)
          xa = num;
        if (xa < 0.01 && cMinLoc.X > MaxLoc.X && (num = MaxLoc.X - cMinLoc.X + 0.01f) > xa)
          xa = num;
        return xa;
      }
      
      public float ClipYCollide(AABB c, float ya)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        c.GetMinMax(out Vector3 cMinLoc, out Vector3 cMaxLoc);
        
        if (cMaxLoc.X - MinLoc.X <= 1.40129846432482E-45 || cMinLoc.X - MaxLoc.X >= 1.40129846432482E-45 ||
            cMaxLoc.Z - MinLoc.Z <= 1.40129846432482E-45 || cMinLoc.Z - MaxLoc.Z >= 1.40129846432482E-45)
          return ya;
        float num;
        if (ya > 0.01 && cMaxLoc.Y < MinLoc.Y && (num = MinLoc.Y - cMaxLoc.Y - 0.01f) < ya)
          ya = num;
        if (ya < 0.01 && cMinLoc.Y > MaxLoc.Y && (num = MaxLoc.Y - cMinLoc.Y + 0.01f) > ya)
          ya = num;
        return ya;
      }
      
      public float ClipZCollide(AABB c, float za)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        c.GetMinMax(out Vector3 cMinLoc, out Vector3 cMaxLoc);
        
        if (cMaxLoc.X - MinLoc.X <=  0.01f || cMinLoc.X - MaxLoc.X >=  0.01f ||
            cMaxLoc.Y - MinLoc.Y <=  0.01f || cMinLoc.Y - MaxLoc.Y >=  0.01f)
          return za;
        float num;
        if (za > 0.01 && cMaxLoc.Z <= MinLoc.Z && (num = MinLoc.Z - cMaxLoc.Z - 0.01f) < za)
          za = num;
        if (za < 0.01 && cMinLoc.Z >= MaxLoc.Z && (num = MaxLoc.Z - cMinLoc.Z + 0.01f) > za)
          za = num;
        return za;
      }



      public AABB Grow(Vector3 size)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        
        var aabb = new AABB();
        aabb.SetAABB(MinLoc - size, MaxLoc + size);
        return aabb;
      } 
      
      public bool Intersects(AABB c)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        c.GetMinMax(out Vector3 cMinLoc, out Vector3 cMaxLoc);
        
        return MaxLoc.X > cMinLoc.X && MaxLoc.Y > cMinLoc.Y &&
               MaxLoc.Z > cMinLoc.Z && MinLoc.X < cMaxLoc.X &&
               MinLoc.Y < cMaxLoc.Y && MinLoc.Z < cMaxLoc.Z;
      }
      
      public void Move(Vector3 a)
      {
        GetMinMax(out Vector3 MinLoc, out Vector3 MaxLoc);
        MinLoc += a;
        MaxLoc += a;
        SetAABB(MinLoc, MaxLoc);
      }


      public void SetAABB(Vector3 min, Vector3 max)
      {
          Origin = (min + max) * .5f;
          Extents = max - Origin;
      }
  
      /// <summary>
      /// Returns min and max values for each entity
      /// </summary>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void GetMinMax( out Vector3 Min, out Vector3 Max)
      {
          Min = Origin - Extents;
          Max = Origin + Extents;
      }
  }
  
}

