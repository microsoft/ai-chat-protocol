// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

using Backend.Interfaces;
using Backend.Model;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStateStore<string>>(new InMemoryStore<string>());
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

app.Run();
