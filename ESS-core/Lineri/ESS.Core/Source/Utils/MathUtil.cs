using System;
using System.Runtime.CompilerServices;


namespace Lineri.ESS.Core.Utils
{

    internal static class MathUtil
    {
        private static Random _random = new Random();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            t = Clamp01(t);
            return a + (b - a) * t;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) <= 0.0001f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomRange(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandomRange(int min, int max)
        {
            return _random.Next(min, max);
        }
    }

}