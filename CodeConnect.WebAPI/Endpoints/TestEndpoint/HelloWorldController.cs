using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.TestEndpoint;
[Route("api/[controller]")]
[ApiController]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string GetHello()
    {
        return "Hello Endpoint";
    }
}