using GameOfLIfe_StrDem.Models;
using GameOfLIfe_StrDem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Controllers
{
    public class PlaygroundController : Controller
    {
        private readonly PlaygroundService _playgroundService;
        public PlaygroundController(PlaygroundService playgroundService)
        {
            _playgroundService = playgroundService;
        }


        [HttpGet]
        public IActionResult Playground()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Enter()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Enter(string playerName)
        {
            if (string.IsNullOrEmpty(playerName) ||
                string.IsNullOrWhiteSpace(playerName)) return Json("WrongName");
            playerName = playerName.Trim();

            var exist = _playgroundService.Players.FirstOrDefault(x => x.Name == playerName);
            if (exist != null) playerName = MakeUniqueName(playerName);

            HttpContext.Response.Cookies.Append("playerName", playerName);
            return Json("OK");
        }


        public string MakeUniqueName(string name)
        {
            int id = 0;
            do id++;
            while (_playgroundService
            .Players
            .FirstOrDefault(x => x.Name == name + (id > 0 ? " (" + id.ToString() + ")" : "")) != null);

            return name + (id > 0 ? " (" + id.ToString() + ")" : "");
        }

    }
}
