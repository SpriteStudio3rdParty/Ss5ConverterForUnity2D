using UnityEngine;
using System.Collections;
using System.IO;

public class Bezier2Poly
{
    // Bézier 曲線の各成分（3次 Bernstein 多項式）の値を求めます
    public static Vector2 Bezier3(float t, Vector2 p1, Vector2 v1, Vector2 p2, Vector2 v2)
    {
        float s = 1 - t;
        Vector2 result = Vector2.zero;
        result.x = p1.x * s * s * s + 3 * (p1.x + v1.x) * s * s * t + 3 * (p2.x + v2.x) * s * t * t + p2.x * t * t * t;
        result.y = p1.y * s * s * s + 3 * (p1.y + v1.y) * s * s * t + 3 * (p2.y + v2.y) * s * t * t + p2.y * t * t * t;
        return result;
    }

    // 線形スケール
    static float LinearScale(float x, float x1, float x2, float y1, float y2)
    {
        return y1 + (y2 - y1) * (x - x1) / (x2 - x1);
    }


    // poly3() の形式の曲線のうち，data を最もよく近似するものを最小二乗法を利用して求めます
    static Vector2 Approximate(Vector2 p1, Vector2 p2, Vector2[] data, int num_data)
    {
        float A42 = 0, A33 = 0, A24 = 0, B21 = 0, B12 = 0;
        
        for (int i = 0; i < num_data; ++i)
        {
            Vector2 datum = data[i];
            float t = LinearScale(datum.x, p1.x, p2.x, 0f, 1f), s = 1 - t;
            float s2 = s * s, st = s * t, t2 = t * t;
            
            A42 += s2 * st * st; // = s^4 * t^2
            A33 += st * st * st; // = s^3 * t^3
            A24 += st * st * t2; // = s^2 * t^4
            
            float tmp = datum.y - p1.y * s * s2 - p2.y * t * t2;
            B21 += s * st * tmp; // = s^2 * t
            B12 += st * t * tmp; // = s * t^2
        }
        
        float det = A42 * A24 - A33 * A33;
        Vector2 alpha = Vector2.zero;
        alpha.x = (A24 * B21 - A33 * B12) / det;
        alpha.y = (A42 * B12 - A33 * B21) / det;

        return alpha;
    }

    // 近似曲線に用いる3次多項式
    static float Poly3(float x, Vector2 p1, Vector2 p2, float alpha1, float alpha2)
    {
        float t = LinearScale(x, p1.x, p2.x, 0, 1), s = 1 - t;
        return p1.y * s * s * s + alpha1 * s * s * t + alpha2 * s * t * t + p2.y * t * t * t;
    }

    public static Vector2 Bezier(Vector2 p1, Vector2 v1, Vector2 p2, Vector2 v2)
    {
        int N = 100; // Bézier 曲線の分割数（標本点の決定に用います）
        
        // Bézier 曲線 p(t) = (x(t), y(t)) (t ∈ [0, 1]) から，
        // 近似に使用する標本点 p(i / N) (i = 1, 2, ..., N - 1) を求める
        Vector2[] data = new Vector2[N - 1];
        for (int i = 0; i < N - 1; ++i)
        {
            data[i] = Bezier3((i + 1) / (float) N, p1, v1, p2, v2);
        }
        
        // 近似曲線を求める
        Vector2 alpha = Approximate(p1, p2, data, N - 1);
        
        // 両端での微分係数
        Vector2 tangent = Vector2.zero;
        tangent.x = (-3 * p1.y + alpha.x) / (p2.x - p1.x); // 左端
        tangent.y = (3 * p2.y - alpha.y) / (p2.x - p1.x); // 右端

        return tangent;
    }
}

