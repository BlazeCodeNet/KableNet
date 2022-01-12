using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KableNet.Math
{
    public class Vec3f
    {
        public Vec3f( float x, float y, float z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x, y, z;

        public static Vec3f Zero { get { return new Vec3f( 0, 0, 0 ); } }

        public override string ToString( )
        {
            return $"({x},{y},{z})";
        }
    }
}
