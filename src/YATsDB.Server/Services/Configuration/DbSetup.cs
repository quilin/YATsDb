using System.ComponentModel.DataAnnotations;

namespace YATsDB.Server.Services.Configuration;

public class DbSetup
{
    [Required] public string DbPath { get; set; }

    [Required] public Tenray.ZoneTree.Options.WriteAheadLogMode WriteAheadLogMode { get; set; }

    public int? MaxMutableSegmentsCount { get; set; }

    public DbSetup()
    {
        DbPath = string.Empty;
    }
}