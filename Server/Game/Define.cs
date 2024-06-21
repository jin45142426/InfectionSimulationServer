using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

    public static Vector3 Up { get { return new Vector3(0, 1, 0); } }
    public static Vector3 Forward { get { return new Vector3(0, 0, 1); } }
    public static Vector3 Right { get { return new Vector3(1, 0, 0); } }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static bool operator ==(Vector3 a, Vector3 b)
    {
        if (a.x == b.x && a.y == b.y && a.z == b.z)
            return true;
        else
            return false;
    }

    public static bool operator !=(Vector3 a, Vector3 b)
    {
        if (a.x == b.x && a.y == b.y && a.z == b.z)
            return false;
        else
            return true;
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3(a.x * b, a.y * b, a.z * b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
        return new Vector3(a.x / b, a.y / b, a.z / b);
    }

    public static Vector3 operator -(Vector3 a)
    {
        return new Vector3(-a.x, -a.y, -a.z);
    }

    public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }

    public float sqrMagnitude { get { return (x * x + y * y + z * z); } }

    public Vector3 normalized
    {
        get
        {
            float length = magnitude;
            
            if (length == 0)
            {
                return new Vector3(0, 0, 0);
            }

            return new Vector3(x / length, y / length, z / length);
        }
    }

    public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
    {
        return new Vector3(start.x + (end.x - start.x) * t, start.y + (end.y - start.y) * t, start.z + (end.z - start.z) * t);
    }
}
