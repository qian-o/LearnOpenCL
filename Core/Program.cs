using Silk.NET.OpenCL;

namespace Core;

public unsafe class Program : IDisposable
{
    private readonly CL _cl;
    private readonly nint _id;
    private readonly Dictionary<string, Kernel> _kernels;

    public nint Id => _id;

    public Device Device { get; }

    internal Program(CL cl, nint id, Device device)
    {
        _cl = cl;
        _id = id;
        _kernels = new Dictionary<string, Kernel>();
        Device = device;
    }

    /// <summary>
    /// 获取指定名称的计算内核
    /// </summary>
    /// <param name="method">方法名</param>
    /// <returns></returns>
    public Kernel GetKernel(string method)
    {
        if (!_kernels.TryGetValue(method, out Kernel? kernel))
        {
            nint kernel_id = _cl.CreateKernel(_id, method, null);

            kernel = new Kernel(_cl, kernel_id, this);

            _kernels.Add(method, kernel);
        }

        return kernel;
    }

    public void Dispose()
    {
        _cl.ReleaseProgram(_id);

        GC.SuppressFinalize(this);
    }
}
