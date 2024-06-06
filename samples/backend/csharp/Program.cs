// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Backend.Interfaces;
using Backend.Model;
using Backend.Services;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis";
    options.InstanceName = "SemanticKernel";
});

builder.Services.AddSingleton<ISecretStore>(new EnvVarSecretStore());
builder.Services.AddSingleton<ISemanticKernelApp, SemanticKernelApp>();

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<AIChatRole>(JsonNamingPolicy.CamelCase)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var currentTimeUTC = DateTimeOffset.UtcNow.ToString();
    var encodedTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
    var options = new DistributedCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
    var distributedCache = app.Services.GetService<IDistributedCache>();
    if (distributedCache != null)
    {
        distributedCache.Set("CachedTimeUTC", encodedTimeUTC, options);
    }
});

app.Run();
