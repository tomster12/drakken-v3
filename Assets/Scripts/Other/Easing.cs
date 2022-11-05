
using UnityEngine;


public static class Easing
{
    public static float easeOutExpo(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    } 
}
