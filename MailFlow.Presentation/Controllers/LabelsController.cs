using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DTOs;
using Shared.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MailFlow.API.Controllers;

[Route("api/labels")]
[ApiController]
public class LabelsController : ControllerBase
{

    private readonly IServiceManager _serviceManager;
    public LabelsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    /// <summary>
    /// Authorize the user to access Gmail API.
    /// </summary>
    /// <returns></returns>


    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize()
    {
        await _serviceManager.UserService.AuthorizeUser();
        return Ok();
    }


    /// <summary>
    /// Sync Gmail labels with the database.
    /// </summary>
    /// <returns>Standardized API response </returns>
    /// <response code="200">Returns standardized API response with list of labels</response>
    /// <response code="204">Returnded if the labels are up to date. Returns empty list</response>
    /// <response code="400">Returned if the request is invalid</response>
    /// <response code="401">Returned if the user is not authorized</response>
    /// <response code="404">Returned if the user is not found</response>
    [ProducesResponseType(200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [HttpGet("sync-labels")]
    public async Task<ActionResult<ApiResponse<GmailLabelListDTO>>> SyncLabels()
    {
        var response = await _serviceManager.GmailLabelService.DownloadAndSyncLabelsAsync(trackChanges: false, path: "labels");

        return response.StatusCode switch
        {
            200 => Ok(response),
            204 => NoContent(),
            401 => Unauthorized(response.Message),
            400 => BadRequest(response.Message),
            404 => NotFound(response.Message),
            _ => StatusCode(response.StatusCode, response.Message)
        };

    }
}