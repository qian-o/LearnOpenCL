﻿using Silk.NET.OpenCL;

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
}
