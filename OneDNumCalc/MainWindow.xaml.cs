using Core;
using Core.Helpers;
using Silk.NET.OpenCL;
using System.Linq;
using System.Windows;

namespace OneDNumCalc;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CL _cl = CL.GetApi();
    private readonly Program _program;

    public MainWindow()
    {
        InitializeComponent();

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

        Platform platform = _cl.GetPlatforms().First();

        Device device = platform.GetDevices(DeviceType.Gpu).First();

        _program = device.CreateProgram(source);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        TestAdd(_program);
        TestMul(_program);
    }

    private unsafe void TestAdd(Program program)
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

    private unsafe void TestMul(Program program)
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
