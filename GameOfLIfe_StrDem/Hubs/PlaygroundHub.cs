using GameOfLIfe_StrDem.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfLIfe_StrDem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;


namespace GameOfLIfe_StrDem.Hubs
{
    public class PlaygroundHub : Hub
    {
        private readonly PlaygroundService _playgroundService;

        public PlaygroundHub(PlaygroundService playgroundService)
        {
            _playgroundService = playgroundService;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.GetHttpContext().Request.Cookies["playerName"];
            _playgroundService.Players.Add(new Player(Context.ConnectionId, userName));
            await Clients.All.SendAsync("UpdatePlayerList", _playgroundService.Players);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Player player = _playgroundService.Players.FirstOrDefault(x => x.Id == Context.ConnectionId);
            if (player != null)
            {
                _playgroundService.Players.Remove(player);
            }
            await Clients.All.SendAsync("UpdatePlayerList", _playgroundService.Players);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task FilterPlayerNames(string nameFilter)
        {
            nameFilter = nameFilter.Trim().ToLower();
            await Clients.Caller.SendAsync("UpdatePlayerList",
                _playgroundService.Players.AsQueryable().Where("Name.ToLower().StartsWith(@0)", nameFilter));
        }



    }
}
