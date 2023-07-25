﻿using Microsoft.AspNetCore.Http;


public static class HttpContextExt
{
    private static IHttpContextAccessor _accessor;

    public static HttpContext Current => _accessor.HttpContext;

    internal static void Configure(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
}
