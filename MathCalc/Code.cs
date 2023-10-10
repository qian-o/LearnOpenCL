using Core;
using Core.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenCL;
using System.Diagnostics;

namespace MathCalc;

internal class Code
{
    static void Main(string[] args)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        _ = args;

        CL cl = CL.GetApi();

        Dictionary<Platform, Device[]> platforms = cl.GetPlatformsAndDevices();

        Program program = platforms.First().Value.First(item => item.Type == DeviceType.Gpu).CreateProgram(File.ReadAllText("matrix_calculate.cl"));

        stopwatch.Restart();

        Multiply(100000);

        stopwatch.Stop();

        Console.WriteLine($"CPU Parallel execution time: {stopwatch.ElapsedMilliseconds} milliseconds");

        stopwatch.Restart();

        Multiply(program, 100000);

        stopwatch.Stop();

        Console.WriteLine($"GPU Parallel execution time: {stopwatch.ElapsedMilliseconds} milliseconds");
    }

    private static unsafe Vector4D<float>[] Multiply(Program program, uint length)
    {
        Kernel kernel = program.GetKernel("multiply");

        nint memA = program.Device.CreateBuffer<Vector4D<float>>(length, MemFlags.ReadOnly);
        nint memB = program.Device.CreateBuffer<Matrix4X4<float>>(length, MemFlags.ReadOnly);
        nint memC = program.Device.CreateBuffer<Vector4D<float>>(length, MemFlags.WriteOnly);

        kernel.SetArgument(0, memA);
        kernel.SetArgument(1, memB);
        kernel.SetArgument(2, memC);

        Vector4D<float>[] a = new Vector4D<float>[length];
        Matrix4X4<float>[] b = new Matrix4X4<float>[length];
        Vector4D<float>[] c = new Vector4D<float>[length];

        for (int i = 0; i < length; i++)
        {
            a[i] = new Vector4D<float>(i, i, i, i);
            b[i] = Matrix4X4.CreateRotationX<float>(i);
        }

        fixed (void* aPtr = a)
        {
            program.Device.WriteBuffer<Vector4D<float>>(memA, length, aPtr);
        }

        fixed (void* bPtr = b)
        {
            program.Device.WriteBuffer<Matrix4X4<float>>(memB, length, bPtr);
        }

        kernel.Run(1, length);

        fixed (void* cPtr = c)
        {
            program.Device.ReadBuffer<Vector4D<float>>(memC, length, cPtr);
        }

        program.Device.DeleteBuffer(memA);
        program.Device.DeleteBuffer(memB);
        program.Device.DeleteBuffer(memC);

        return c;
    }

    private static Vector4D<float>[] Multiply(uint length)
    {
        Vector4D<float>[] a = new Vector4D<float>[length];
        Matrix4X4<float>[] b = new Matrix4X4<float>[length];
        Vector4D<float>[] c = new Vector4D<float>[length];

        Parallel.For(0, length, (i) =>
        {
            a[i] = new Vector4D<float>(i, i, i, i);
            b[i] = Matrix4X4.CreateRotationX<float>(i);

            c[i] = Vector4D.Transform(a[i], b[i]);
        });

        return c;
    }
}
