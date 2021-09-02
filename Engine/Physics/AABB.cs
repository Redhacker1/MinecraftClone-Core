

using Engine.MathLib.DoublePrecision_Numerics;

namespace Engine.Physics
{
  public class Aabb
  {
    private const double epsilon = 0.01;
    public Vector3 MinLoc;
    public Vector3 MaxLoc;

    public Aabb(Vector3 minLoc, Vector3 maxLoc)
    {
      this.MinLoc = minLoc;
      this.MaxLoc = maxLoc;
    }

    public Aabb Expand(Vector3 size)
    {
      Vector3 minLoc = this.MinLoc;
      Vector3 maxLoc = this.MaxLoc;
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

    public Aabb Grow(Vector3 size) => new Aabb(this.MinLoc - size, this.MaxLoc + size);

    public double ClipXCollide(Aabb c, double xa)
    {
      if (c.MaxLoc.Y < this.MinLoc.Y || c.MinLoc.Y >= this.MaxLoc.Y || c.MaxLoc.Z < this.MinLoc.Z ||
          c.MinLoc.Z >= this.MaxLoc.Z || c.MaxLoc.Y - this.MinLoc.Y <= 1.40129846432482E-45 ||
          c.MinLoc.Y - this.MaxLoc.Y >= 1.40129846432482E-45 || c.MaxLoc.Z - this.MinLoc.Z <= 1.40129846432482E-45 ||
          c.MinLoc.Z - this.MaxLoc.Z >= 1.40129846432482E-45)
        return xa;
      double num;
      if (xa > 0.01 && c.MaxLoc.X < this.MinLoc.X && (num = this.MinLoc.X - c.MaxLoc.X - 0.01) < xa)
        xa = num;
      if (xa < 0.01 && c.MinLoc.X > this.MaxLoc.X && (num = this.MaxLoc.X - c.MinLoc.X + 0.01) > xa)
        xa = num;
      return xa;
    }

    public double ClipYCollide(Aabb c, double ya)
    {
      if (c.MaxLoc.X - this.MinLoc.X <= 1.40129846432482E-45 || c.MinLoc.X - this.MaxLoc.X >= 1.40129846432482E-45 ||
          c.MaxLoc.Z - this.MinLoc.Z <= 1.40129846432482E-45 || c.MinLoc.Z - this.MaxLoc.Z >= 1.40129846432482E-45)
        return ya;
      double num;
      if (ya > 0.01 && c.MaxLoc.Y < this.MinLoc.Y && (num = this.MinLoc.Y - c.MaxLoc.Y - 0.01) < ya)
        ya = num;
      if (ya < 0.01 && c.MinLoc.Y > this.MaxLoc.Y && (num = this.MaxLoc.Y - c.MinLoc.Y + 0.01) > ya)
        ya = num;
      return ya;
    }

    public double ClipZCollide(Aabb c, double za)
    {
      if (c.MaxLoc.X - this.MinLoc.X <= 1.40129846432482E-45 || c.MinLoc.X - this.MaxLoc.X >= 1.40129846432482E-45 ||
          c.MaxLoc.Y - this.MinLoc.Y <= 1.40129846432482E-45 || c.MinLoc.Y - this.MaxLoc.Y >= 1.40129846432482E-45)
        return za;
      double num;
      if (za > 0.01 && c.MaxLoc.Z <= this.MinLoc.Z && (num = this.MinLoc.Z - c.MaxLoc.Z - 0.01) < za)
        za = num;
      if (za < 0.01 && c.MinLoc.Z >= this.MaxLoc.Z && (num = this.MaxLoc.Z - c.MinLoc.Z + 0.01) > za)
        za = num;
      return za;
    }

    public bool Intersects(Aabb c) => this.MaxLoc.X > c.MinLoc.X && this.MaxLoc.Y > c.MinLoc.Y &&
                                      this.MaxLoc.Z > c.MinLoc.Z && this.MinLoc.X < c.MaxLoc.X &&
                                      this.MinLoc.Y < c.MaxLoc.Y && this.MinLoc.Z < c.MaxLoc.Z;

    public void Move(Vector3 a)
    {
      this.MinLoc += a;
      this.MaxLoc += a;
    }
  }
}