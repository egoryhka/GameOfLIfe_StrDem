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
                if (player.InGame)
                {
                    if (_playgroundService.Games.ContainsKey(player.GameId))
                        await StopGame(player.GameId);
                }
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

        public async Task Invite(string targetId)
        {
            Player sender = _playgroundService.Players.FirstOrDefault(x => x.Id == Context.ConnectionId);
            if (sender != null) await Clients.Client(targetId).SendAsync("Invite", sender);
        }

        public async Task StartGame(string initiatorId, string inviteSenderId)
        {
            Player initiator = _playgroundService.Players.FirstOrDefault(x => x.Id == initiatorId);
            Player inviteSender = _playgroundService.Players.FirstOrDefault(x => x.Id == inviteSenderId);
            if (initiator == null || inviteSender == null) return;

            if (!_playgroundService.Games.ContainsKey(initiatorId))
            {
                Game game = new Game(initiator, inviteSender);
                _playgroundService.Games.Add(initiatorId, game);
            }
            initiator.InGame = inviteSender.InGame = true;
            initiator.GameId = inviteSender.GameId = initiatorId;
            await Clients.Client(initiatorId).SendAsync("GameStarted", inviteSender);
            await Clients.Client(inviteSenderId).SendAsync("GameStarted", initiator);
        }

        public async Task StopGame(string gameId)
        {
            if (_playgroundService.Games.ContainsKey(gameId))
            {
                Game game = _playgroundService.Games[gameId];

                game.P1.InGame = game.P2.InGame = false;
                game.P1.GameId = game.P2.GameId = null;

                if (_playgroundService.Players.FirstOrDefault(x => x.Id == game.P1.Id) != null)
                    await Clients.Client(game.P1.Id).SendAsync("GameStoped");

                if (_playgroundService.Players.FirstOrDefault(x => x.Id == game.P2.Id) != null)
                    await Clients.Client(game.P2.Id).SendAsync("GameStoped");

                _playgroundService.Games.Remove(gameId);

            }

        }

    }
}
