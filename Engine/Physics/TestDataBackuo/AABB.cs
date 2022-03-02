

using System.Numerics;

namespace Engine.Physics.TestDataBackuo
{

  /// <summary>
  /// Struct used internally, is lower precision but will in most instances due to implementation of the methods below handle stuff further out easier.
  /// </summary>
  internal struct internalAABB
  {
    public Vector3 MinLoc;
    public Vector3 MaxLoc;
    internal internalAABB(Vector3 Minloc, Vector3 Maxloc)
    {
      MinLoc = Minloc;
      MaxLoc = Maxloc;
    }
  }
  public class Aabb
  {
    const double epsilon = 0.01;
    public Vector3 MinLoc;
    public Vector3 MaxLoc;

    public Aabb(Vector3 minLoc, Vector3 maxLoc)
    {
      MinLoc = minLoc;
      MaxLoc = maxLoc;
    }

    public Aabb Expand(Vector3 size)
    {
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
      return new Aabb(minLoc, maxLoc);
    }

    public Aabb Grow(Vector3 size) => new Aabb(MinLoc - size, MaxLoc + size);

    public double ClipXCollide(Aabb c, double xa)
    {
      Vector3 SubtractAmount;
      if (Vector3.Distance(Vector3.Zero, c.MinLoc) > Vector3.Distance(Vector3.Zero, MinLoc))
      {
        SubtractAmount = MinLoc;
      }
      else
      {
        SubtractAmount = c.MinLoc;
      }
      
      internalAABB cInternal = new internalAABB(c.MinLoc - SubtractAmount, c.MaxLoc - SubtractAmount);
      internalAABB Internal = new internalAABB(MinLoc - SubtractAmount, MaxLoc - SubtractAmount);
      
      if (cInternal.MaxLoc.Y < Internal.MinLoc.Y || cInternal.MinLoc.Y >= Internal.MaxLoc.Y || cInternal.MaxLoc.Z < Internal.MinLoc.Z ||
          cInternal.MinLoc.Z >= Internal.MaxLoc.Z || cInternal.MaxLoc.Y - Internal.MinLoc.Y <= 0.01f ||
          cInternal.MinLoc.Y - Internal.MaxLoc.Y >= 0.01f || cInternal.MaxLoc.Z - Internal.MinLoc.Z <= 0.01f ||
          cInternal.MinLoc.Z - Internal.MaxLoc.Z >= 0.01f)
        return xa;
      float num;
      if (xa > 0.01f && cInternal.MaxLoc.X < Internal.MinLoc.X && (num = Internal.MinLoc.X - cInternal.MaxLoc.X - 0.01f) < xa)
        xa = num;
      if (xa < 0.01f && cInternal.MinLoc.X > Internal.MaxLoc.X && (num = Internal.MaxLoc.X - cInternal.MinLoc.X + 0.01f) > xa)
        xa = num;
      return xa;
    }

    public double ClipYCollide(Aabb c, double ya)
    {
      Vector3 SubtractAmount;
      if (Vector3.Distance(Vector3.Zero, c.MinLoc) > Vector3.Distance(Vector3.Zero, MinLoc))
      {
        SubtractAmount = MinLoc;
      }
      else
      {
        SubtractAmount = c.MinLoc;
      }
      
      internalAABB cInternal = new internalAABB(c.MinLoc - SubtractAmount, c.MaxLoc - SubtractAmount);
      internalAABB Internal = new internalAABB(MinLoc - SubtractAmount, MaxLoc - SubtractAmount);
      
      if (c.MaxLoc.X - MinLoc.X <= 0.01f || c.MinLoc.X - MaxLoc.X >= 0.01f ||
          c.MaxLoc.Z - MinLoc.Z <= 0.01f || c.MinLoc.Z - MaxLoc.Z >= 0.01f)
        return ya;
      float num;
      if (ya > 0.01f && cInternal.MaxLoc.Y < Internal.MinLoc.Y && (num = Internal.MinLoc.Y - cInternal.MaxLoc.Y - 0.01f) < ya)
        ya = num;
      if (ya < 0.01f && cInternal.MinLoc.Y > Internal.MaxLoc.Y && (num = Internal.MaxLoc.Y - cInternal.MinLoc.Y + 0.01f) > ya)
        ya = num;
      return ya;
    }

    public double ClipZCollide(Aabb c, double za)
    {
      Vector3 SubtractAmount;
      if (Vector3.Distance(Vector3.Zero, c.MinLoc) > Vector3.Distance(Vector3.Zero, MinLoc))
      {
        SubtractAmount = MinLoc;
      }
      else
      {
        SubtractAmount = c.MinLoc;
      }
      
      internalAABB cInternal = new internalAABB(c.MinLoc - SubtractAmount, c.MaxLoc - SubtractAmount);
      internalAABB Internal = new internalAABB(MinLoc - SubtractAmount, MaxLoc - SubtractAmount);
      
      if (cInternal.MaxLoc.X - Internal.MinLoc.X <= 0.01f || cInternal.MinLoc.X - Internal.MaxLoc.X >= 0.01f ||
          cInternal.MaxLoc.Y - Internal.MinLoc.Y <= 0.01f || cInternal.MinLoc.Y - Internal.MaxLoc.Y >= 0.01f)
        return za;
      float num;
      if (za > 0.01 && cInternal.MaxLoc.Z <= Internal.MinLoc.Z && (num = Internal.MinLoc.Z - cInternal.MaxLoc.Z - 0.01f) < za)
        za = num;
      if (za < 0.01 && cInternal.MinLoc.Z >= Internal.MaxLoc.Z && (num = Internal.MaxLoc.Z - cInternal.MinLoc.Z + 0.01f) > za)
        za = num;
      return za;
    }

    
    public bool Intersects(Aabb c)
    {
      Vector3 SubtractAmount;
      if (Vector3.Distance(Vector3.Zero, c.MinLoc) > Vector3.Distance(Vector3.Zero, MinLoc))
      {
        SubtractAmount = MinLoc;
      }
      else
      {
        SubtractAmount = c.MinLoc;
      }
      internalAABB cInternal = new internalAABB(c.MinLoc - SubtractAmount, c.MaxLoc - SubtractAmount);
      internalAABB Internal = new internalAABB(MinLoc - SubtractAmount, MaxLoc - SubtractAmount);
      
      return Internal.MaxLoc.X > cInternal.MinLoc.X && Internal.MaxLoc.Y > cInternal.MinLoc.Y &&
             Internal.MaxLoc.Z > cInternal.MinLoc.Z && Internal.MinLoc.X < cInternal.MaxLoc.X &&
             Internal.MinLoc.Y < cInternal.MaxLoc.Y && Internal.MinLoc.Z < cInternal.MaxLoc.Z;
    }

    public void Move(Vector3 a)
    {
      MinLoc += a;
      MaxLoc += a;
    }
  }
}