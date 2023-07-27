using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Web;
using Newtonsoft.Json;
using System.Net;
namespace WebApplication1.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ChannelModel : PageModel
    {
        private readonly ILogger<ChannelModel> _logger;
        public ChannelModel(ILogger<ChannelModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
        }
    }
}