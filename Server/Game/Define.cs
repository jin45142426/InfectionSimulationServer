using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Scenario
{
    public string Infection { get; set; }       //현재 시나리오의 감염병 종류

    #region 환자
    public string PatientName { get; set; }     //환자 이름
    public string PatientId { get; set; }       //환자 생년월일
    #endregion

    public int ScenarioCount { get; set; } = 0;     //시나리오 진행도
    public ScenarioInfo ScenarioInfo { get; set; } = new ScenarioInfo();
}

public class ScenarioInfo
{
    public string Situation { get; set; }   //현재 상황
    public string Actor { get; set; }       //행동을 취해야하는 사람
    public string Action { get; set; }      //취해야 할 행동
    public string Script { get; set; }      //NPC 대사
    public List<string> Keywords { get; set; }    //키워드
}

public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y) { this.x = x; this.y = y; }

    public static Vector2 Up { get { return new Vector2(0, 1); } }
    public static Vector2 Right { get { return new Vector2(1, 0); } }

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y )
            return true;
        else
            return false;
    }

    public static bool operator !=(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y)
            return false;
        else
            return true;
    }

    public static Vector2 operator *(Vector2 a, float b)
    {
        return new Vector2(a.x * b, a.y * b);
    }

    public static Vector2 operator /(Vector2 a, float b)
    {
        return new Vector2(a.x / b, a.y / b);
    }

    public static Vector2 operator -(Vector2 a)
    {
        return new Vector2(-a.x, -a.y);
    }

    public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }

    public float sqrMagnitude { get { return (x * x + y * y); } }

    public Vector2 normalized
    {
        get
        {
            float length = magnitude;

            if (length == 0)
            {
                return new Vector2(0, 0);
            }

            return new Vector2(x / length, y / length);
        }
    }

    public static Vector2 Lerp(Vector2 start, Vector2 end, float t)
    {
        return new Vector2(start.x + (end.x - start.x) * t, start.y + (end.y - start.y) * t);
    }
}

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
