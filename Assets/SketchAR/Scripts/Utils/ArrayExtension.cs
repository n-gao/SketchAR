using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ArrayExtension
{
    /// <summary>
    /// Fills an array at the given start with the given value.
    /// </summary>
    /// <typeparam name="T">Type of the array.</typeparam>
    /// <param name="self">Array</param>
    /// <param name="value">Value to override</param>
    /// <param name="start">Start index</param>
    public static void Fill<T>(this T[] self, T value, int start = 0)
    {
        for (int i = start; i < self.Length; i++)
        {
            self[i] = value;
        }
    }
}