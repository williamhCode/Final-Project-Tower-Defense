using System;

namespace TowerDefense.Extensions
{
    public static class ExtensionMethods
    {
        public static (int X, int Y)? CoordinatesOf<T>(this T[][] matrix, T value)
        {
            for (int x = 0; x < matrix.Length; x++)
            {
                for (int y = 0; y < matrix[x].Length; y++)
                {
                    var currValue = matrix[x][y];
                    if (currValue != null && currValue.Equals(value))
                        return (x, y);
                }
            }

            return null;
        }
    }
}