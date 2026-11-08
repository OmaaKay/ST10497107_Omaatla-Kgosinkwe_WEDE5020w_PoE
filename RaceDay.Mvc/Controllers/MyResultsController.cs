using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Participant)]
public class MyResultsController : AppController
{
    public MyResultsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var result = await Api.GetAsync<List<MyResultDto>>("api/results/me");
        if (Guard(result) is { } failure) return failure;

        return View((result.Data ?? new()).OrderByDescending(r => r.EventDate).ToList());
    }
}
