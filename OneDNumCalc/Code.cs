using Core;
using Core.Helpers;
using Silk.NET.OpenCL;
using System.Diagnostics;

namespace OneDNumCalc;

class Code
{
    static void Main(string[] args)
    {
        _ = args;

        CL cl = CL.GetApi();

        Dictionary<Platform, Device[]> platforms = cl.GetPlatformsAndDevices();
        
        string source = @"
            kernel void calculateGCD(global const int* a, global const int* b, global int* c)
            {
                int i = get_global_id(0);

                int number1 = a[i];
                int number2 = b[i];
    
                while (number2 != 0)
                {
                    int remainder = number1 % number2;
                    number1 = number2;
                    number2 = remainder;
                }
    
                c[i] = number1;
            }";

        Program program = platforms.First().Value.First(item => item.Type == DeviceType.Gpu).CreateProgram(source);

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 5; i++)
        {
            stopwatch.Restart();

            CalculateGCD(program, 100000);
            CalculateGCD(program, 100000);
            CalculateGCD(program, 100000);

            Console.WriteLine($"GPU Parallel execution time: {stopwatch.ElapsedMilliseconds} milliseconds");
        }

        Console.WriteLine();

        for (int i = 0; i < 5; i++)
        {
            stopwatch.Restart();

            CalculateGCD(100000);
            CalculateGCD(100000);
            CalculateGCD(100000);

            Console.WriteLine($"CPU Parallel execution time: {stopwatch.ElapsedMilliseconds} milliseconds");
        }

        Console.ReadKey();
    }

    /// <summary>
    /// OpenCL implementation of GCD calculation.
    /// </summary>
    /// <param name="program"></param>
    /// <param name="length"></param>
    private static unsafe void CalculateGCD(Program program, uint length)
    {
        Kernel kernel = program.GetKernel("calculateGCD");

        nint memA = program.Device.CreateBuffer<int>(length, MemFlags.ReadOnly);
        nint memB = program.Device.CreateBuffer<int>(length, MemFlags.ReadOnly);
        nint memC = program.Device.CreateBuffer<int>(length, MemFlags.WriteOnly);

        kernel.SetArgument(0, memA);
        kernel.SetArgument(1, memB);
        kernel.SetArgument(2, memC);

        int* a = stackalloc int[(int)length];
        int* b = stackalloc int[(int)length];
        int* c = stackalloc int[(int)length];
        for (int i = 0; i < length; i++)
        {
            a[i] = i;
            b[i] = i;
        }

        program.Device.WriteBuffer<int>(memA, length, a);
        program.Device.WriteBuffer<int>(memB, length, b);

        kernel.Run(1, length);

        program.Device.ReadBuffer<int>(memC, length, c);

        program.Device.DeleteBuffer(memA);
        program.Device.DeleteBuffer(memB);
        program.Device.DeleteBuffer(memC);
    }

    /// <summary>
    /// CPU implementation of GCD calculation.
    /// </summary>
    /// <param name="length"></param>
    private static unsafe void CalculateGCD(uint length)
    {
        int* a = stackalloc int[(int)length];
        int* b = stackalloc int[(int)length];
        int* c = stackalloc int[(int)length];
        for (int i = 0; i < length; i++)
        {
            a[i] = i;
            b[i] = i;
        }

        Parallel.For(0, length, (i) =>
        {
            int number1 = a[i];
            int number2 = b[i];

            while (number2 != 0)
            {
                int remainder = number1 % number2;
                number1 = number2;
                number2 = remainder;
            }

            c[i] = number1;
        });
    }
}
