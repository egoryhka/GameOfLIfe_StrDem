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
        private async Task UpdatePlayersOnAllClients()
        {
            await Clients.AllExcept(_playgroundService.Players
                .Where(x => x.Filtering)
                .Select(x => x.Id))
                .SendAsync("UpdatePlayerList", _playgroundService.Players);
        }

        private async Task UpdatePlayersOnCaller()
        {
            await Clients.Caller.SendAsync("UpdatePlayerList", _playgroundService.Players);
        }

        public async Task FilterPlayers(string predicate)
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null)
            {
                me.Filtering = true;
                await Clients.Caller
                    .SendAsync("UpdatePlayerList", _playgroundService.Players
                    .AsQueryable()
                    .Where(predicate));
            }
        }

        public async Task CancelFiltering()
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null)
            {
                me.Filtering = false;
                await UpdatePlayersOnCaller();
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.GetHttpContext().Request.Cookies["playerName"];
            _playgroundService.Players.Add(new Player(Context.ConnectionId, userName));
            await UpdatePlayersOnAllClients();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Player player = _playgroundService.GetPlayer(Context.ConnectionId);
            if (player != null)
            {
                if (player.GameId != null)
                {
                    if (_playgroundService.Games.ContainsKey(player.GameId))
                        await StopGame(player.GameId);
                }
                _playgroundService.Players.Remove(player);
            }
            await UpdatePlayersOnAllClients();
            await base.OnDisconnectedAsync(exception);
        }


        public async Task Invite(string targetId)
        {
            Player target = _playgroundService.GetPlayer(targetId);
            if (target == null || target.Inviting || target.GameId != null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("InviteFail");
                return;
            }

            Player sender = _playgroundService.GetPlayer(Context.ConnectionId);
            if (sender != null)
            {
                sender.Inviting = target.Inviting = true;
                await Clients.Client(targetId).SendAsync("Invite", sender);
            }

            await UpdatePlayersOnAllClients();
        }

        public async Task AcceptInvite(string inviteSenderId)
        {
            await CreateGame(inviteSenderId);
        }

        public async Task DeclineInvite(string inviteSenderId)
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId); // Отклонитель
            Player sender = _playgroundService.GetPlayer(inviteSenderId);
            if (me != null)
            {
                me.Inviting = false;
            }
            if (sender != null)
            {
                sender.Inviting = false;
                await Clients.Client(inviteSenderId).SendAsync("InviteRejected");
            }
            await UpdatePlayersOnAllClients();
        }

        public async Task Ready()
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null && me.GameId != null && _playgroundService.Games.ContainsKey(me.GameId))
            {
                Game game = _playgroundService.Games[me.GameId];
                Player opponent = game.GetOpponent(me);
                if (opponent != null)
                {
                    me.Ready = true;
                    await Clients.Caller.SendAsync("MeReady");
                    await Clients.Client(opponent.Id).SendAsync("OpponentReady");
                }

                if (game.P1.Ready && game.P2.Ready)
                {
                    await Clients.Clients(game.P1.Id, game.P2.Id).SendAsync("StartCountDown3");

                    // сделать планировщиком! 
                    //await Clients.Clients(game.P1.Id, game.P2.Id).SendAsync("StopCountDown3");

                }
            }
        }

        public async Task CreateGame(string inviteSenderId)
        {
            Player initiator = _playgroundService.GetPlayer(Context.ConnectionId); // <- Я (тот кто принял инвайт)
            Player inviteSender = _playgroundService.GetPlayer(inviteSenderId);
            if (initiator == null || inviteSender == null) return;

            if (_playgroundService.Games.ContainsKey(initiator.Id) == false)
            {
                Game game = new Game(initiator, inviteSender);
                _playgroundService.Games.Add(initiator.Id, game);
                initiator.GameId = inviteSender.GameId = initiator.Id;

                game.P1.Field.Clean();
                game.P2.Field.Clean();

                await Clients.Client(initiator.Id).SendAsync("GameCreated", inviteSender);
                await Clients.Client(inviteSenderId).SendAsync("GameCreated", initiator);
            }

            await UpdatePlayersOnAllClients();
        }

        public async Task StopGame(string gameId)
        {
            if (_playgroundService.Games.ContainsKey(gameId))
            {
                Game game = _playgroundService.Games[gameId];

                game.P1.Inviting = game.P2.Inviting = false;
                game.P1.Ready = game.P2.Ready = false;
                game.P1.GameId = game.P2.GameId = null;

                if (_playgroundService.Players.FirstOrDefault(x => x.Id == game.P1.Id) != null)
                    await Clients.Client(game.P1.Id).SendAsync("GameStoped");

                if (_playgroundService.Players.FirstOrDefault(x => x.Id == game.P2.Id) != null)
                    await Clients.Client(game.P2.Id).SendAsync("GameStoped");

                _playgroundService.Games.Remove(gameId);
                await UpdatePlayersOnAllClients();
            }

        }

    }
}
