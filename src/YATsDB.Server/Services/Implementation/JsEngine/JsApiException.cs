﻿namespace YATsDB.Server.Services.Implementation.JsEngine;

public class JsApiException : ApplicationException
{
    public JsApiException()
    {
    }

    public JsApiException(string? message)
        : base(message)
    {
    }

    public JsApiException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
