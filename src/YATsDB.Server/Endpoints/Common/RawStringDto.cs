using System.Reflection;

namespace YATsDB.Server.Endpoints.Common;

public class RawStringDto
{
    public string Value
    {
        get;
    }

    public RawStringDto(string value)
    {
        Value = value;
    }

    public static async ValueTask<RawStringDto?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        //TODO: check content type a content encoding http headers
        using var streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);
        var content = await streamReader.ReadToEndAsync(context.RequestAborted);

        return new RawStringDto(content);
    }
}
