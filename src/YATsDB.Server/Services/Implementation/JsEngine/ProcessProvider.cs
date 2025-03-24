using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace YATsDB.Server.Services.Implementation.JsEngine;

internal class ProcessProvider
{
    private readonly ILogger<ProcessProvider> logger;
    private readonly bool isEnabled;

    public ProcessProvider(ILogger<ProcessProvider> logger, bool isEnabled)
    {
        this.logger = logger;
        this.isEnabled = isEnabled;
    }

    public int StartProcess(IDictionary<string, object?> spParams)
    {
        logger.LogTrace("Entering to StartProcess.");

        CheckEnabled();

        var processStartInfo = new ProcessStartInfo();
        processStartInfo.UseShellExecute = false;
        processStartInfo.FileName = GetRequiredString(spParams, "path");
        processStartInfo.Arguments = GetOptionalString(spParams, "arguments", string.Empty);

        var workingDirectory = GetOptionalString(spParams, "workingDirectory", string.Empty);
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            processStartInfo.WorkingDirectory = workingDirectory;
        }

        var userName = GetOptionalString(spParams, "userName", string.Empty);
        if (!string.IsNullOrEmpty(userName))
        {
            processStartInfo.UserName = userName;
        }

        var timeout = TimeSpan.FromMinutes(1.0);

        var timeoutStr = GetOptionalString(spParams, "timeout", string.Empty);
        if (!string.IsNullOrEmpty(timeoutStr))
        {
            timeout = TimeSpan.Parse(timeoutStr);
        }

        var stdInStr = GetOptionalString(spParams, "stdin", string.Empty);
        if (!string.IsNullOrEmpty(stdInStr))
        {
            processStartInfo.RedirectStandardInput = true;
        }

        logger.LogInformation(
            "Start process: FileName={FileName} Arguments={Arguments} WorkingDirectory={WorkingDirectory} UserName={UserName} RedirectStandardInput={RedirectStandardInput}",
            processStartInfo.FileName,
            processStartInfo.Arguments,
            processStartInfo.WorkingDirectory,
            processStartInfo.UserName,
            processStartInfo.RedirectStandardInput);

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new Exception();
        }

        if (!string.IsNullOrEmpty(stdInStr))
        {
            process.StandardInput.Write(stdInStr);
            process.StandardInput.Flush();
            process.StandardInput.Close();
        }

        if (!process.WaitForExit(timeout))
        {
            throw new JsApiException("Program timeout.");
        }

        logger.LogDebug("Process exited: {exitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private string GetRequiredString(IDictionary<string, object?> spParams, string name,
        [CallerMemberName] string methodName = "")
    {
        if (spParams.TryGetValue(name, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }

        throw new JsApiException($"Method {methodName} require parameter object with key {name}.");
    }

    private string GetOptionalString(IDictionary<string, object?> spParams, string name, string defaultValue,
        [CallerMemberName] string methodName = "")
    {
        if (spParams.TryGetValue(name, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }

        return defaultValue;
    }

    private void CheckEnabled()
    {
        if (!isEnabled)
        {
            throw new JsApiException("Process API is not enabled.");
        }
    }
}