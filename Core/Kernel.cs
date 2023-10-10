using Core.Helpers;
using Silk.NET.OpenCL;

namespace Core;

public unsafe class Kernel : IDisposable
{
    private readonly CL _cl;
    private readonly nint _id;

    public nint Id => _id;

    public Program Program { get; }

    internal Kernel(CL cl, nint id, Program program)
    {
        _cl = cl;
        _id = id;
        Program = program;
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    /// <param name="index">参数位置</param>
    /// <param name="size">参数长度</param>
    /// <param name="buffer_id">缓存Id</param>
    public void SetArgument(uint index, nint buffer_id)
    {
        _cl.SetKernelArg(_id, index, (uint)sizeof(nint), &buffer_id).StateCheck();
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="index">参数位置</param>
    /// <param name="value">值</param>
    public void SetArgument<T>(uint index, T value) where T : unmanaged
    {
        _cl.SetKernelArg(_id, index, (uint)sizeof(T), value).StateCheck();
    }

    /// <summary>
    /// 运行内核
    /// </summary>
    /// <param name="dim">计算维度</param>
    /// <param name="size">计算长度</param>
    public void Run(uint dim, uint size)
    {
        _cl.EnqueueNdrangeKernel(Program.Device.CommandQueue, _id, dim, null, size, null, 0, null, null).StateCheck();
    }

    public void Dispose()
    {
        _cl.ReleaseKernel(_id);

        GC.SuppressFinalize(this);
    }
}
