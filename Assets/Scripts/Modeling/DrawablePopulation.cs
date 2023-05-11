using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Modeling
{
    public class DrawablePopulation<T> : Population<T>, IDrawable where T : DrawableAgent
    {
        public Mesh Mesh;
        public Material Material;

        public virtual void Draw()
        {
            //{
            //    /*var instData = Agents.Select(p => p.Matrix).ToList();
            //    for (int i =0; i < instData.Count / 1023; i++)
            //    {
            //        var data = instData.Skip(i * 1023).Take(1023);
            //        Graphics.DrawMeshInstanced(Mesh, 0, Material, data.ToArray());
            //    }*/

            foreach (var agent in Agents)
            {
                Graphics.DrawMesh(Mesh, agent.Matrix, Material, 0);
            }
        }
    }
}
