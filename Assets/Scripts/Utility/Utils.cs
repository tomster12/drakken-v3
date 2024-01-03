using UnityEngine;

public static class Utils
{
    public static float FunkyLerp(float x, float a, float b, float t, float v, bool set = false)
    {
        float nx = t * (b - a) + a;
        if (t == 0) nx = 0;
        if (set) return nx;
        return Mathf.Lerp(x, nx, Time.deltaTime * v);
    }
}
