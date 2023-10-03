using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { Args = args });

builder.WebHost.UseKestrelCore();
builder.Services.AddRoutingCore();
builder.Services.AddLogging(opt => opt.AddSimpleConsole());

builder.Services.Configure<RouteHandlerOptions>(opt => opt.ThrowOnBadRequest = true);
builder.Services.AddOutputCache(opt =>
{
    opt.SizeLimit = 64 * 1024 * 1024;
    opt.MaximumBodySize = 4 * 1024 * 1024;
    // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-8.0#default-output-caching-policy
    // something wrong with default policy (?)
    opt.AddBasePolicy(policyBuilder => policyBuilder
      .Expire(TimeSpan.FromSeconds(10))
      .Cache()
    );
});

var app = builder.Build();

app.UseRouting();
app.UseOutputCache();

app.MapGet("/", ([FromQuery(Name = "id")] Id id) => { return Task.FromResult(Results.Ok($"id: {id}")); });

// UseEndpoints in RunAsync
await app.RunAsync();

record struct Id(long Value) : ISpanParsable<Id>
{

    public static Id Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new(long.Parse(s, provider));

    public static Id Parse(string s, IFormatProvider? provider) => new(long.Parse(s, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Id result)
    {
        result = new Id(default);
        if (long.TryParse(s, provider, out long val))
        {
            result = new(val);
            return true;
        }
        return false;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Id result)
    {
        result = new Id(default);
        if (long.TryParse(s, provider, out long val))
        {
            result = new(val);
            return true;
        }
        return false;
    }
}