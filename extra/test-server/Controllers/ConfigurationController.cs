// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Azure.AI.Chat.SampleService;

[ApiController]
[Route("config")]
public class ConfigController : ControllerBase
{
    [HttpPost]
    public IActionResult Config()
    {
        BackendChatService previousValue = GlobalSettings.backendChatService;
        const int maxIntValue = (int)BackendChatService.EndValue - 1;

        if (HttpContext.Request.Query.TryGetValue("backend", out StringValues stringValue))
        {
            if (int.TryParse(stringValue.ToString(), out int intValue))
            {
                if (intValue >= 0 && intValue <= maxIntValue)
                {
                    GlobalSettings.backendChatService = (BackendChatService)intValue;

                    if (previousValue == GlobalSettings.backendChatService)
                    {
                        return Content($"Backend service is already set to {GlobalSettings.backendChatService}. No updates.\n");
                    }
                    else
                    {
                        return Content($"Backend service updated from {previousValue} to {GlobalSettings.backendChatService}\n");
                    }
                }
            }

            return BadRequest($"Invalid query param value `backend={stringValue.ToString()}`. Only integer values in the range [0, {maxIntValue}] are supported.\n");
        }

        return BadRequest($"Missing query parameter `backend=...` with an integer value in the range [0, {maxIntValue}].\n");
    }
}
