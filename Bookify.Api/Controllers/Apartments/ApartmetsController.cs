using Bookify.Application.Apartments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments;

[ApiController]
[Route("api/v{version:apiVersion}/apartments")]
//[Authorize]
public class ApartmetsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchApartments(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new SearchApartmentsQuery(startDate, endDate), cancellationToken));
}
