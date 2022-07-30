using System.Numerics;

namespace MCClone_Core.Physics
{
  public class Aabb
  {
    const float Epsilon = 0.01f;
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

    public float ClipXCollide(Aabb c, float xa)
    {
      if (c.MaxLoc.Y < MinLoc.Y || c.MinLoc.Y >= MaxLoc.Y || c.MaxLoc.Z < MinLoc.Z ||
          c.MinLoc.Z >= MaxLoc.Z || c.MaxLoc.Y - MinLoc.Y <= 1.40129846432482E-45 ||
          c.MinLoc.Y - MaxLoc.Y >= 1.40129846432482E-45 || c.MaxLoc.Z - MinLoc.Z <= 1.40129846432482E-45 ||
          c.MinLoc.Z - MaxLoc.Z >= 1.40129846432482E-45)
        return xa;
      float num;
      if (xa > 0.01 && c.MaxLoc.X < MinLoc.X && (num = MinLoc.X - c.MaxLoc.X - 0.01f) < xa)
        xa = num;
      if (xa < 0.01 && c.MinLoc.X > MaxLoc.X && (num = MaxLoc.X - c.MinLoc.X + 0.01f) > xa)
        xa = num;
      return xa;
    }

    public float ClipYCollide(Aabb c, float ya)
    {
      if (c.MaxLoc.X - MinLoc.X <= 1.40129846432482E-45 || c.MinLoc.X - MaxLoc.X >= 1.40129846432482E-45 ||
          c.MaxLoc.Z - MinLoc.Z <= 1.40129846432482E-45 || c.MinLoc.Z - MaxLoc.Z >= 1.40129846432482E-45)
        return ya;
      float num;
      if (ya > 0.01 && c.MaxLoc.Y < MinLoc.Y && (num = MinLoc.Y - c.MaxLoc.Y - 0.01f) < ya)
        ya = num;
      if (ya < 0.01 && c.MinLoc.Y > MaxLoc.Y && (num = MaxLoc.Y - c.MinLoc.Y + 0.01f) > ya)
        ya = num;
      return ya;
    }

    public float ClipZCollide(Aabb c, float za)
    {
      if (c.MaxLoc.X - MinLoc.X <=  0.01f || c.MinLoc.X - MaxLoc.X >=  0.01f ||
          c.MaxLoc.Y - MinLoc.Y <=  0.01f || c.MinLoc.Y - MaxLoc.Y >=  0.01f)
        return za;
      float num;
      if (za > 0.01 && c.MaxLoc.Z <= MinLoc.Z && (num = MinLoc.Z - c.MaxLoc.Z - 0.01f) < za)
        za = num;
      if (za < 0.01 && c.MinLoc.Z >= MaxLoc.Z && (num = MaxLoc.Z - c.MinLoc.Z + 0.01f) > za)
        za = num;
      return za;
    }

    public bool Intersects(Aabb c) => MaxLoc.X > c.MinLoc.X && MaxLoc.Y > c.MinLoc.Y &&
                                      MaxLoc.Z > c.MinLoc.Z && MinLoc.X < c.MaxLoc.X &&
                                      MinLoc.Y < c.MaxLoc.Y && MinLoc.Z < c.MaxLoc.Z;

    public void Move(Vector3 a)
    {
      MinLoc += a;
      MaxLoc += a;
    }
  }
}