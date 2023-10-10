using Silk.NET.OpenCL;

namespace Core.Helpers;

public unsafe static class OpenCLExtensions
{
    public static Platform[] GetPlatforms(this CL cl)
    {
        uint* num_platforms = stackalloc uint[1];
        cl.GetPlatformIDs(0, null, num_platforms);

        nint* platform_ids = stackalloc nint[(int)*num_platforms];
        cl.GetPlatformIDs(*num_platforms, platform_ids, null);

        Platform[] result = new Platform[*num_platforms];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Platform(cl, platform_ids[i]);
        }

        return result;
    }

    public static Dictionary<Platform, Device[]> GetPlatformsAndDevices(this CL cl, bool print = true)
    {
        Dictionary<Platform, Device[]> platforms = new();

        foreach (Platform item in cl.GetPlatforms())
        {
            platforms.Add(item, item.GetDevices(DeviceType.All));
        }

        if (print)
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

        return platforms;
    }

    public static void StateCheck(this int errorCode)
    {
        if ((ErrorCodes)errorCode != ErrorCodes.Success)
        {
            throw new Exception(errorCode.ToString());
        }
    }
}
