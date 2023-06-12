using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pg2Project
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public const int Size = (3 + 3 + 2) * 4; // 3 pozice, 3 normály, 2 texturovací (každá po 4 bajetch) souřadnice

        public void SetPosition(Vector3 position)
        {
            Position += position;
        }
    }
}
