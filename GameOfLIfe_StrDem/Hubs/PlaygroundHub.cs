using AutoMapper;
using GameOfLIfe_StrDem.Models;
using GameOfLIfe_StrDem.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Hubs
{

    public class PlaygroundHub : Hub
    {
        private readonly PlaygroundService _playgroundService;
        private readonly IMapper _mapper;

        public PlaygroundHub(PlaygroundService playgroundService, IMapper mapper)
        {
            _playgroundService = playgroundService;
            _mapper = mapper;
        }
        private async Task UpdatePlayersOnAllClients()
        {
            await Clients.AllExcept(_playgroundService.Players
                .Where(x => x.Filtering)
                .Select(x => x.Id))
                .SendAsync("UpdatePlayerList", _mapper.Map<List<PlayerDto>>(_playgroundService.Players/*.OrderByDescending(x => x.Points)*/));
        }

        private async Task UpdatePlayersOnCaller()
        {
            await Clients.Caller.SendAsync("UpdatePlayerList", _mapper.Map<List<PlayerDto>>(_playgroundService.Players/*.OrderByDescending(x => x.Points)*/));
        }

        public async Task FilterPlayers(string predicate)
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null)
            {
                me.Filtering = true;
                var filterResult = _playgroundService.Players
                    .AsQueryable()
                    .Where(predicate)
                    .ToList();

                await Clients.Caller
                    .SendAsync("UpdatePlayerList", _mapper.Map<List<PlayerDto>>(filterResult/*.OrderByDescending(x => x.Points)*/));
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
                await Clients.Client(targetId).SendAsync("Invite", _mapper.Map<PlayerDto>(sender));
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
                    game.State = GameState.Starting;

                    var clients = Clients.Clients(game.P1.Id, game.P2.Id);
                    await clients.SendAsync("CountDown3");
                }
            }
        }


        public async Task NextGameState()
        {
            Player me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null && me.GameId != null && _playgroundService.Games.ContainsKey(me.GameId))
            {
                me.ReadyToNextState = true;
                Game game = _playgroundService.Games[me.GameId];
                Player opponent = game.GetOpponent(me);

                if (me.ReadyToNextState && opponent.ReadyToNextState)
                {
                    var clients = Clients.Clients(game.P1.Id, game.P2.Id);
                    me.ReadyToNextState = opponent.ReadyToNextState = false;
                    switch (game.State)
                    {
                        case GameState.Starting:
                            {
                                game.State = GameState.Drawing;
                                await clients.SendAsync("CountDownDraw", GameSettings.DrawTime);
                                return;
                            }
                        case GameState.Drawing:
                            {
                                game.State = GameState.Simulation;
                                await clients.SendAsync("CountDownSim", GameSettings.SimTime);

                                var meClient = Clients.Client(me.Id);
                                var opClient = Clients.Client(opponent.Id);

                                for (int i = 0; i < GameSettings.SimTime * GameSettings.SimStepsPerSecond; i++)
                                {
                                    me.Field.SimulationStep();
                                    opponent.Field.SimulationStep();
                                    await meClient.SendAsync("UpdateFields", me.Field, opponent.Field);
                                    await opClient.SendAsync("UpdateFields", opponent.Field, me.Field);
                                    Thread.Sleep(1000 / GameSettings.SimStepsPerSecond);
                                }

                                return;
                            }
                        case GameState.Simulation:
                            {
                                game.State = GameState.End;
                                Player winner = game.GetWinner();
                                winner?.AddPoints();
                                await clients.SendAsync("WinnerIs", winner);
                                return;
                            }
                        case GameState.End:
                            {
                                await StopGame(me.GameId);
                                return;
                            }
                    }
                }
            }
        }

        //public void SetField(bool[,] field)
        //{
        //    Player me = _playgroundService.GetPlayer(Context.ConnectionId);
        //    if (me != null) me.Field.Set(field);
        //}

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

                await Clients.Client(initiator.Id).SendAsync("GameCreated", _mapper.Map<PlayerDto>(inviteSender));
                await Clients.Client(inviteSenderId).SendAsync("GameCreated", _mapper.Map<PlayerDto>(initiator));
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

                _playgroundService.Games.Remove(gameId);

                await Clients.Clients(game.P1.Id, game.P2.Id).SendAsync("GameStoped");
                await UpdatePlayersOnAllClients();
            }

        }

        public async Task EditField(int x, int y, bool alive)
        {
            var me = _playgroundService.GetPlayer(Context.ConnectionId);
            if (me != null)
            {
                me.Field.SetCell(x, y, alive);

                if (_playgroundService.Games.ContainsKey(me.GameId))
                {
                    Game game = _playgroundService.Games[me.GameId];
                    var opponent = game.GetOpponent(me);

                    if (opponent != null)
                    {
                        await Clients.Client(opponent.Id).SendAsync("OpponentDraw", x, y, alive);
                    }
                }
            }
        }


    }
}
