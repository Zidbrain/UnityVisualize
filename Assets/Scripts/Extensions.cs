using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public static class Extensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> keyValuePair, out T1 param1, out T2 param2)
        {
            param1 = keyValuePair.Key;
            param2 = keyValuePair.Value;
        }

        public static IEnumerable<T> Between<T>(this IEnumerable<T> values, Func<T, bool> startPredicateExclude, Func<T, bool> endPredicateInclude)
        {
            var startIncluding = false;
            foreach (var value in values)
            {
                if (startIncluding)
                {
                    yield return value;
                    if (endPredicateInclude(value)) break;
                }
                if (startPredicateExclude(value)) startIncluding = true;
            }
        }

        public static int CoerceAtMin(this int value, int min)
        {
            return Math.Max(value, min);
        }

        public static UnityEngine.Vector2 FlipY(this UnityEngine.Vector2 vector)
        {
            return new UnityEngine.Vector2(vector.x, -vector.y);
        }

        public static float MaxOfAxis(this UnityEngine.Vector2 vector)
        {
            return Math.Max(vector.x, vector.y);
        }
    }
}
