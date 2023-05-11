// Alan Zucconi
// www.alanzucconi.com
using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using Assets.Scripts;

public class Heatmap : MonoBehaviour
{
    public Vector4[] positions = new Vector4[0];
    public Vector4[] properties = new Vector4[0];

    public Material material;

    void Start()
    {
    }

    public void SetMagnitudes(int[] magnitues, Vector2 minPos, Vector2 maxPos) { 
        if (!enabled) return;

        if (positions.Length != 100)
        {
            positions = new Vector4[100];
            properties = new Vector4[100];
        }

        if (magnitues.Length != 100) throw new Exception();
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                var pos = new Vector2(x, y) * (maxPos - minPos) / 10 + minPos + (maxPos - minPos) / 20f;
                positions[y * 10 + x] = new Vector4(pos.x, 0.001f, pos.y, 0f);
                properties[y * 10 + x] = new Vector4((maxPos - minPos).MaxOfAxis() / 10f, magnitues[y * 10 + x] / 25f);
            }
        }
    }

    void Update()
    {
        if (positions.Length != 100) return;

        material.SetInt("_Points_Length", positions.Length);
        material.SetVectorArray("_Points", positions);
        material.SetVectorArray("_Properties", properties);
    }
}