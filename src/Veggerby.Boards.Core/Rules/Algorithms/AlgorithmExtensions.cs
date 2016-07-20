using System;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public static class AlgorithmExtensions
    {
        public static void Print(this int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                System.Console.Write($"\t{i}");
            }
            
            System.Console.WriteLine();

            for (int i = 0; i < array.Length; i++)
            {
                System.Console.Write($"\t{array[i]}");
            }

            System.Console.WriteLine();
        }

        public static void Print(this int[,] array)
        {
            System.Console.WriteLine();
            
            for (int i = 0; i < array.GetLength(0); i++)
            {
                System.Console.Write($"\t{i}");
            }
            
            System.Console.WriteLine();

            for (int x = 0; x < array.GetLength(0); x++)
            {
                System.Console.Write($"{x}\t");
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    System.Console.Write($"{array[x, y]}\t");
                }

                System.Console.WriteLine();
            }
        }
    }
}