using Core;
using Core.Helpers;
using Silk.NET.OpenCL;

namespace OneDNumCalc;

class Code
{
    static void Main(string[] args)
    {
        _ = args;

        CL cl = CL.GetApi();

        Dictionary<Platform, Device[]> platforms = new();
        foreach (Platform item in cl.GetPlatforms())
        {
            platforms.Add(item, item.GetDevices(DeviceType.All));
        }

        // Print Platforms;
        {
            Console.WriteLine("Supported Platforms:");
            foreach (Platform platform in platforms.Keys)
            {
                Console.WriteLine($"  {platform.Name} - {platform.Version}");

                Console.WriteLine("    Devices:");
                foreach (Device device in platforms[platform])
                {
                    Console.WriteLine($"        {device.Name} - {device.Version} - {device.Type}");
                }

                Console.WriteLine();
            }
        }

        string source = @"
            kernel void add(global const int* a, global const int* b, global int* c)
            {
                int i = get_global_id(0);
                c[i] = a[i] + b[i];
            }

            kernel void mul(global const int* a, global const int* b, global int* c)
            {
                int i = get_global_id(0);
                c[i] = a[i] * b[i];
            }";

        Program program = platforms.First().Value.First(item => item.Type == DeviceType.Gpu).CreateProgram(source);

        TestAdd(program);
        TestMul(program);
    }

    private static unsafe void TestAdd(Program program)
    {
        Kernel kernel = program.GetKernel("add");

        nint memA = program.Device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memB = program.Device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memC = program.Device.CreateBuffer<int>(1024, MemFlags.WriteOnly);

        kernel.SetArgument(0, memA);
        kernel.SetArgument(1, memB);
        kernel.SetArgument(2, memC);

        int* a = stackalloc int[1024];
        int* b = stackalloc int[1024];
        int* c = stackalloc int[1024];
        for (int i = 0; i < 1024; i++)
        {
            a[i] = i;
            b[i] = i;
        }

        program.Device.WriteBuffer<int>(memA, 1024, a);
        program.Device.WriteBuffer<int>(memB, 1024, b);

        kernel.Run(1, 1024);

        program.Device.ReadBuffer<int>(memC, 1024, c);

        program.Device.DeleteBuffer(memA);
        program.Device.DeleteBuffer(memB);
        program.Device.DeleteBuffer(memC);
    }

    private static unsafe void TestMul(Program program)
    {
        Kernel kernel = program.GetKernel("mul");

        nint memA = program.Device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memB = program.Device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memC = program.Device.CreateBuffer<int>(1024, MemFlags.WriteOnly);

        kernel.SetArgument(0, memA);
        kernel.SetArgument(1, memB);
        kernel.SetArgument(2, memC);

        int* a = stackalloc int[1024];
        int* b = stackalloc int[1024];
        int* c = stackalloc int[1024];
        for (int i = 0; i < 1024; i++)
        {
            a[i] = i;
            b[i] = i;
        }

        program.Device.WriteBuffer<int>(memA, 1024, a);
        program.Device.WriteBuffer<int>(memB, 1024, b);

        kernel.Run(1, 1024);

        program.Device.ReadBuffer<int>(memC, 1024, c);

        program.Device.DeleteBuffer(memA);
        program.Device.DeleteBuffer(memB);
        program.Device.DeleteBuffer(memC);
    }
}
