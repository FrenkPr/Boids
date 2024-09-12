using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids
{
    static class RandomGenerator
    {
        private static Random r;

        static RandomGenerator()
        {
            r = new Random();
        }

        public static int GetRandomInt(int max)
        {
            return r.Next(max);
        }

        public static int GetRandomInt(int min, int max)
        {
            return r.Next(min, max);
        }

        public static float GetRandomFloat()
        {
            return (float)r.NextDouble();
        }
    }
}
