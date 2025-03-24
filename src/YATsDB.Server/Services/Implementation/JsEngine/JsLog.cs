using Jint.Native;

namespace YATsDB.Server.Services.Implementation.JsEngine;

internal class JsLog
{
    private readonly ILogger logger;

    public JsLog(ILogger logger)
    {
        this.logger = logger;
    }

    public JsValue Trace(JsValue args)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Debug(JsValue args)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Info(JsValue args)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Warning(JsValue args)
    {
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Error(JsValue args)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }
}
