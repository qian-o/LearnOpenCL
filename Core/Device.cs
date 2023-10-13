using Core.Helpers;
using Silk.NET.OpenCL;

namespace Core;

public unsafe class Device : IDisposable
{
    private readonly CL _cl;
    private readonly nint _id;
    private readonly string _version;
    private readonly string _name;
    private readonly DeviceType _type;
    private readonly nint _context;
    private readonly nint _commandQueue;
    private readonly List<nint> _buffers;

    public nint Id => _id;

    public Platform Platform { get; }

    public string Version => _version;

    public string Name => _name;

    public DeviceType Type => _type;

    public nint Context => _context;

    public nint CommandQueue => _commandQueue;

    internal Device(CL cl, nint id, Platform platform)
    {
        _cl = cl;
        _id = id;
        Platform = platform;

        byte* version = stackalloc byte[1024];
        byte* name = stackalloc byte[1024];
        byte* type = stackalloc byte[8];

        cl.GetDeviceInfo(id, DeviceInfo.Version, 1024, version, null);
        cl.GetDeviceInfo(id, DeviceInfo.Name, 1024, name, null);
        cl.GetDeviceInfo(id, DeviceInfo.Type, 8, type, null);

        _version = new string((sbyte*)version);
        _name = new string((sbyte*)name);
        _type = (DeviceType)(*(ulong*)type);
        _context = cl.CreateContext(null, 1, id, null, null, null);
        _commandQueue = cl.CreateCommandQueue(_context, id, CommandQueueProperties.None, null);
        _buffers = new List<nint>();
    }

    /// <summary>
    /// 创建程序
    /// </summary>
    /// <param name="source">运算代码</param>
    /// <param name="options">
    /// 构建配置选项，可选项如下（每个选项之间需要使用空格隔开）：
    /// 1. `-cl-fast-relaxed-math`：启用快速松散的浮点数运算，可能会牺牲一些精度。
    /// 2. `-cl-mad-enable`：启用浮点数乘加运算的优化。
    /// 3. `-cl-no-signed-zeros`：禁用浮点数的负零。
    /// 4. `-cl-unsafe-math-optimizations`：启用不安全的数学优化（例如将除法转换为乘法）。
    /// 5. `-cl-single-precision-constant`：将浮点数常量视为单精度浮点数。
    /// 6. `-cl-denorms-are-zero`：将浮点数的非规格化数视为零。
    /// 7. `-cl-opt-disable`：禁用所有优化。
    /// 8. `-cl-strict-aliasing`：启用严格的别名规则。
    /// 9. `-cl-finite-math-only`：只使用有限的数学运算。
    /// 10. `-w`：禁用所有警告。
    /// </param>
    /// <returns></returns>
    public Program CreateProgram(string source, string[]? options = null)
    {
        options ??= new string[] { "-cl-mad-enable" };

        nint program_id = _cl.CreateProgramWithSource(_context, 1, new[] { source }, null, null);

        _cl.BuildProgram(program_id, 0, null, string.Join(' ', options), null, null).StateCheck();

        return new Program(_cl, program_id, this);
    }

    /// <summary>
    /// 创建缓存
    /// </summary>
    /// <param name="size">缓存长度</param>
    /// <param name="flags">标记</param>
    /// <returns></returns>
    public nint CreateBuffer<T>(uint size, MemFlags flags) where T : unmanaged
    {
        nint buffer_id = _cl.CreateBuffer(_context, flags, (uint)(size * sizeof(T)), null, null);

        _buffers.Add(buffer_id);

        return buffer_id;
    }

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="buffer_id">缓存Id</param>
    public void DeleteBuffer(nint buffer_id)
    {
        _cl.ReleaseMemObject(buffer_id).StateCheck();
        _buffers.Remove(buffer_id);
    }

    /// <summary>
    /// 写入缓存
    /// </summary>
    /// <param name="buffer_id">缓存Id</param>
    /// <param name="size">数据长度</param>
    /// <param name="ptr">数据地址</param>
    public void WriteBuffer<T>(nint buffer_id, uint size, void* ptr) where T : unmanaged
    {
        _cl.EnqueueWriteBuffer(_commandQueue, buffer_id, true, 0, (uint)(size * sizeof(T)), ptr, 0, null, null).StateCheck();
    }

    /// <summary>
    /// 读取缓存
    /// </summary>
    /// <param name="buffer_id">缓存Id</param>
    /// <param name="size">数据长度</param>
    /// <param name="ptr">数据地址</param>
    public void ReadBuffer<T>(nint buffer_id, uint size, void* ptr) where T : unmanaged
    {
        _cl.EnqueueReadBuffer(_commandQueue, buffer_id, true, 0, (uint)(size * sizeof(T)), ptr, 0, null, null).StateCheck();
    }

    public void Dispose()
    {
        foreach (nint buffer_id in _buffers)
        {
            _cl.ReleaseMemObject(buffer_id);
        }
        _buffers.Clear();
        _cl.ReleaseCommandQueue(_commandQueue);
        _cl.ReleaseContext(_context);
        _cl.ReleaseDevice(_id);

        GC.SuppressFinalize(this);
    }
}