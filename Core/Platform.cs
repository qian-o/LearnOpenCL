using Silk.NET.OpenCL;

namespace Core;

public unsafe class Platform
{
    private readonly CL _cl;
    private readonly nint _id;
    private readonly string _version;
    private readonly string _name;

    public nint Id => _id;

    public string Version => _version;

    public string Name => _name;

    internal Platform(CL cl, nint id)
    {
        _cl = cl;
        _id = id;

        byte* version = stackalloc byte[1024];
        byte* name = stackalloc byte[1024];

        cl.GetPlatformInfo(id, PlatformInfo.Version, 1024, version, null);
        cl.GetPlatformInfo(id, PlatformInfo.Name, 1024, name, null);

        _version = new string((sbyte*)version);
        _name = new string((sbyte*)name);
    }

    /// <summary>
    /// 获取平台支持的设备
    /// </summary>
    /// <param name="deviceType">设备类型</param>
    /// <returns></returns>
    public Device[] GetDevices(DeviceType deviceType = DeviceType.Gpu)
    {
        uint* num_devices = stackalloc uint[1];
        _cl.GetDeviceIDs(_id, deviceType, 0, null, num_devices);

        Device[] devices = new Device[*num_devices];

        nint* device_ids = stackalloc nint[(int)*num_devices];
        _cl.GetDeviceIDs(_id, deviceType, *num_devices, device_ids, null);

        for (int i = 0; i < devices.Length; i++)
        {
            devices[i] = new Device(_cl, device_ids[i], this);
        }

        return devices;
    }
}
