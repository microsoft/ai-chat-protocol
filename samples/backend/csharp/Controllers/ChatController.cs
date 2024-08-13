// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

using Backend.Interfaces;
using Backend.Model;
using System.Text.RegularExpressions;

namespace Backend.Controllers;

[ApiController, Route("api/[controller]")]
public partial class ChatController : ControllerBase
{
    private readonly ISemanticKernelApp _semanticKernelApp;

    public ChatController(ISemanticKernelApp semanticKernelApp)
    {
        _semanticKernelApp = semanticKernelApp;
    }

    [GeneratedRegex(@"messages\[(\d+)\]\.files\[(\d+)\]")]
    private static partial Regex MessageFilesRegex();

    private (int MessageIndex, int FileIndex, IFormFile File) GetPosition(IFormFile formFile)
    {
        var match = MessageFilesRegex().Match(formFile.Name);
        if (match.Success && int.TryParse(match.Groups[1].ValueSpan, out var messageIndex) && int.TryParse(match.Groups[2].ValueSpan, out var fileIndex))
        {
            return (messageIndex, fileIndex, formFile);
        }

        throw new ArgumentException("Malformed multipart request: Invalid file name.");
    }

    private async Task<AIChatRequest> RequestFromMultipart(IFormFileCollection formFiles)
    {
        using var jsonFileStream = formFiles
            .First(f => f.Name == "json")
            .OpenReadStream();
        if (jsonFileStream is null)
        {
            throw new Exception("Malformed multipart request: Missing json part.");
        }

        var request = await JsonSerializer.DeserializeAsync<AIChatRequest>(jsonFileStream) ??
            throw new Exception("Malformed multipart request: Invalid json part.");
        foreach (var (messageIndex, fileIndex, file) in formFiles.Where(f => f.Name != "json").Select(GetPosition).OrderBy(p => p.MessageIndex).ThenBy(p => p.FileIndex))
        {
            if (request.Messages.Count <= messageIndex)
            {
                throw new Exception("Malformed multipart request: Invalid message index.");
            }

            var message = request.Messages[messageIndex];
            message.Files ??= new List<AIChatFile>();
            if (message.Files.Count != fileIndex)
            {
                throw new Exception("Malformed multipart request: Invalid file index.");
            }

            using var fileStream = file.OpenReadStream();
            var fileData = await BinaryData.FromStreamAsync(fileStream);
            message.Files.Add(new AIChatFile
            {
                ContentType = file.ContentType,
                Data = fileData
            });

        }
        return request;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ProcessMessage(IFormFileCollection files)
    {
        try
        {
            var request = await RequestFromMultipart(files);
            var session = request.SessionState switch
            {
                Guid sessionId => await _semanticKernelApp.GetSession(sessionId),
                _ => await _semanticKernelApp.CreateSession(Guid.NewGuid())
            };

            return Ok(await session.ProcessRequest(request));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
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
        response.Headers.Append("Content-Type", "application/jsonl");
        await foreach (var delta in session.ProcessStreamingRequest(request))
        {
            await response.WriteAsync($"{JsonSerializer.Serialize(delta)}\r\n");
            await response.Body.FlushAsync();
        }
    }
}
