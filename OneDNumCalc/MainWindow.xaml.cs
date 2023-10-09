using Core;
using Core.Helpers;
using Silk.NET.OpenCL;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace OneDNumCalc;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CL cl = CL.GetApi();

    public MainWindow()
    {
        InitializeComponent();
    }

    private unsafe void Window_Loaded(object sender, RoutedEventArgs e)
    {
        string source = @"
            kernel void add(global const int* a, global const int* b, global int* c, float amplify)
            {
                int i = get_global_id(0);
                c[i] = (a[i] + b[i]) * amplify;
            }";

        Platform platform = cl.GetPlatforms().First();

        using Device device = platform.GetDevices(DeviceType.Gpu).First();
        using Program program = device.CreateProgram(source);
        using Kernel kernel = program.GetKernel("add");

        nint memA = device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memB = device.CreateBuffer<int>(1024, MemFlags.ReadOnly);
        nint memC = device.CreateBuffer<int>(1024, MemFlags.WriteOnly);

        kernel.SetArgument(0, memA);
        kernel.SetArgument(1, memB);
        kernel.SetArgument(2, memC);
        kernel.SetArgument(3, 2.0f);

        int* a = stackalloc int[1024];
        int* b = stackalloc int[1024];
        int* c = stackalloc int[1024];
        for (int i = 0; i < 1024; i++)
        {
            a[i] = i;
            b[i] = i;
        }

        device.WriteBuffer<int>(memA, 1024, a);
        device.WriteBuffer<int>(memB, 1024, b);

        kernel.Run(1, 1024);

        device.ReadBuffer<int>(memC, 1024, c);

        for (int i = 0; i < 1024; i++)
        {
            Debug.WriteLine(c[i]);
        }
    }
}
