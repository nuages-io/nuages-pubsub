using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IPubSubService _pubSubService;

    public AuthController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
   
}