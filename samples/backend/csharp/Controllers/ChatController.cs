// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

using Backend.Interfaces;
using Backend.Model;

namespace Backend.Controllers;

[ApiController, Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ISemanticKernelApp _semanticKernelApp;

    public ChatController(ISemanticKernelApp semanticKernelApp)
    {
        _semanticKernelApp = semanticKernelApp;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> ProcessMessage(AIChatRequest request)
    {
        var session = request.SessionState switch
        {
            Guid sessionId => await _semanticKernelApp.GetSession(sessionId),
            _ => await _semanticKernelApp.CreateSession(Guid.NewGuid())
        };
        var response = await session.ProcessRequest(request);
        return Ok(response);
    }

    [HttpPost("stream")]
    [Consumes("application/json")]
    public async Task ProcessStreamingMessage(AIChatRequest request)
    {
        var session = request.SessionState switch
        {
            Guid sessionId => await _semanticKernelApp.GetSession(sessionId),
            _ => await _semanticKernelApp.CreateSession(Guid.NewGuid())
        };
        var response = Response;
        response.Headers.Append("Content-Type", "application/x-ndjson");
        await foreach (var delta in session.ProcessStreamingRequest(request))
        {
            await response.WriteAsync($"{JsonSerializer.Serialize(delta)}\r\n");
            await response.Body.FlushAsync();
        }
    }
}
