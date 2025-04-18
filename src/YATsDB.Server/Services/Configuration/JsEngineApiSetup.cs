﻿using System.ComponentModel.DataAnnotations;

namespace YATsDB.Server.Services.Configuration;

public class JsEngineApiSetup
{
    [Required] public bool EnableHttpApi { get; init; }

    [Required] public bool EnableProcesspApi { get; init; }

    public JsEngineApiSetup()
    {
    }
}