using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Security.Policy;
using System.Drawing;
using System.Threading;
using System.Data;
using Microsoft.EntityFrameworkCore;


namespace Pupa.Controllers
{

    [Route("beesuite/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        public UserController()
        {

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            return Ok("USD");
        }

    }
}
