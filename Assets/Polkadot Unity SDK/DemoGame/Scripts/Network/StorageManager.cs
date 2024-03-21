using Substrate.Hexalem.Engine;
using Substrate.Hexalem.Integration.Model;
using Substrate.Hexalem.NET.NetApiExt.Generated.Storage;
using Substrate.Integration.Helper;
using Substrate.Integration.Model;
using Substrate.NetApi.Model.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts
{
    public class StorageManager : Singleton<StorageManager>
    {
        public delegate void NextBlocknumberHandler(uint blocknumber);

        public event NextBlocknumberHandler OnNextBlocknumber;

        public delegate void StorageUpdatedHandler(uint blocknumber);

        public event StorageUpdatedHandler OnStorageUpdated;

        public delegate void HexaBoardChangedHandler(HexaBoard hexaBoard);

        public event HexaBoardChangedHandler OnChangedHexaBoard;

        public delegate void HexaPlayerChangedHandler(HexaPlayer hexaPlayer);

        public event HexaPlayerChangedHandler OnChangedHexaPlayer;

        public delegate void HexaSelectionChangedHandler(List<byte> hexaSelection);

        public event HexaSelectionChangedHandler OnChangedHexaSelection;

        public delegate void NextPlayerTurnHandler(byte playerTurn);

        public event NextPlayerTurnHandler OnNextPlayerTurn;

        public delegate void BoardStateChangedHandler(HexBoardState boardState);

        public event BoardStateChangedHandler OnBoardStateChanged;

        public delegate void MatchFoundHandler(byte[] gameId);
        public event MatchFoundHandler OnGameFound;

        public delegate void ForceAcceptMatchHandler();
        public event ForceAcceptMatchHandler OnForceAcceptMatch;

        public delegate void GameStartedhHandler(byte[] gameId);
        public event GameStartedhHandler OnGameStarted;

        public NetworkManager Network => NetworkManager.GetInstance();

        public uint BlockNumber { get; private set; }

        public AccountInfoSharp AccountInfo { get; private set; }
        public uint AccountEloRating { get; private set; }
        public bool IsAlreadyInQueue { get; private set; }
        public List<uint> PlayersInSameBracket {  get; private set; }
        public HexaGame HexaGame { get; private set; }

        public bool UpdateHexalem { get; internal set; }

        public uint MockBlockNumber { get; internal set; }

        public bool[] HasAccountBoard { get; internal set; }

        private bool _isPolling;
        private uint? blockNumberNewGameIsCreated;

        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            //Your code goes here

            UpdateHexalem = true;
            MockBlockNumber = 1;

            HasAccountBoard = new bool[Enum.GetValues(typeof(AccountType)).Length];

            _isPolling = false;
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            InvokeRepeating(nameof(UpdatedBaseData), 1.0f, 2.0f);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            /// Your code goes here
        }

        public bool CanPollStorage()
        {
            if (Network.Client == null)
            {
                Debug.LogError($"[StorageManager] Client is null");
                return false;
            }

            if (!Network.Client.IsConnected)
            {
                //Debug.Log($"[StorageManager] Client is not connected");
                return false;
            }

            return true;
        }

        private async void UpdatedBaseData()
        {
            if (_isPolling)
            {
                return;
            }

            // don't update hexalem on chain informations ...
            if (!UpdateHexalem)
            {
                MockBlockNumber++;
                OnStorageUpdated?.Invoke(MockBlockNumber);
                OnNextBlocknumber?.Invoke(MockBlockNumber);
                return;
            }

            if (!CanPollStorage())
            {
                return;
            }

            var blockNumber = await Network.Client.GetBlocknumberAsync(null, CancellationToken.None);

            if (blockNumber == null || BlockNumber >= blockNumber)
            {
                return;
            }

            BlockNumber = blockNumber.Value;
            OnNextBlocknumber?.Invoke(blockNumber.Value);

            Debug.Log($"[StorageManager] Block {BlockNumber}");

            if (Network.Client.Account == null)
            {
                Debug.Log($"[StorageManager] Client account not set");
                return;
            }

            // make sure to not poll storage twice
            _isPolling = true;

            var stopwatch = new System.Diagnostics.Stopwatch(); // Create a Stopwatch instance
            stopwatch.Start(); // Start timing

            AccountInfo = await Network.Client.GetAccountAsync(null, CancellationToken.None);

            AccountEloRating = await Network.Client.GetRatingStorageAsync(Network.Client.Account.ToString(), CancellationToken.None);
            IsAlreadyInQueue = await Network.Client.IsPlayerInMatchmakingAsync(Network.Client.Account, CancellationToken.None);

            // Just fake it until I get the real value
            PlayersInSameBracket = new List<uint>() { 1500, 1000, 1200, 1100 };

            // Get the game id
            // 1. If we are in matchmaking => matchmaking state
            // 2. if we are playing => board state
            GameSharp? playerGame = null;

            var matchmaking = await Network.Client.GetMatchmakingStateAsync(Network.Client.Account, null, CancellationToken.None);
            if(matchmaking.IsGameFound)
            {
                if (blockNumberNewGameIsCreated == null)
                {
                    Debug.Log($"Game is created at block {blockNumberNewGameIsCreated}");
                    blockNumberNewGameIsCreated = blockNumber.Value;
                }

                playerGame = await Network.Client.GetGameAsync(matchmaking.GameId, null, CancellationToken.None);

                var playerIndex = playerGame.Players.ToList().IndexOf(Network.Client.Account.ToString());
                if (!playerGame.PlayerAccepted[playerIndex]) {
                    Debug.Log($"New game found for player {Network.Client.Account}");
                    OnGameFound?.Invoke(matchmaking.GameId);
                }
            }

            //var playerBracket = await Network.Client.GetPlayerBracketAsync(Network.Client.Account, CancellationToken.None);
            //var nbPlayerSameBracket = (await Network.Client.GetBracketIndicesAsync((byte)playerBracket, CancellationToken.None);

            var myBoard = await Network.Client.GetBoardAsync(Network.Client.Account.Value, null, CancellationToken.None);
            playerGame = playerGame == null && myBoard != null ? 
                await Network.Client.GetGameAsync(myBoard.GameId, null, CancellationToken.None) : 
                null;
            if (myBoard == null || playerGame == null)
            {
                HexaGame = null;
            }
            else
            {
                switch(playerGame.State)
                {
                    case Substrate.Hexalem.NET.NetApiExt.Generated.Model.pallet_hexalem.types.game.GameState.Accepting:
                        if(blockNumberNewGameIsCreated + 10 > blockNumber)
                        {
                            OnForceAcceptMatch?.Invoke();
                        }
                        break;
                    case Substrate.Hexalem.NET.NetApiExt.Generated.Model.pallet_hexalem.types.game.GameState.Playing:
                        OnGameStarted.Invoke(playerGame.GameId);
                        var playerBoards = new List<BoardSharp>();
                        foreach (var player in playerGame.Players)
                        {
                            var playerBoard = await Network.Client.GetBoardAsync(player, null, CancellationToken.None);
                            if (playerBoard != null)
                            {
                                playerBoards.Add(playerBoard);
                            }
                        }

                        HexaGame oldGame = null;
                        if (HexaGame != null)
                        {
                            oldGame = (HexaGame)HexaGame.Clone();
                        }
                        HexaGame = HexalemWrapper.GetHexaGame(playerGame, playerBoards.ToArray());
                        // check for the event
                        HexaGameDiff(oldGame, HexaGame, PlayerIndex(Network.Client.Account).Value);
                        break;
                    case Substrate.Hexalem.NET.NetApiExt.Generated.Model.pallet_hexalem.types.game.GameState.Finished:
                        // Todo
                        break;
                }
            }

            OnStorageUpdated?.Invoke(blockNumber.Value);

            stopwatch.Stop(); // Stop timing
            Debug.Log($"Poll Storage: {stopwatch.ElapsedMilliseconds} ms");

            _isPolling = false;
        }

        public void HexaGameDiff(HexaGame oldGame, HexaGame newGame, int playerIndex)
        {
            if (newGame == null)
            {
                return;
            }

            var newPlayer = newGame.HexaTuples[playerIndex].player;
            if (oldGame == null || !oldGame.HexaTuples[playerIndex].player.Value.SequenceEqual(newPlayer.Value))
            {
                Debug.Log("[EVENT] OnChangedHexaPlayer");
                OnChangedHexaPlayer?.Invoke(newPlayer);
            }

            var newBoard = newGame.HexaTuples[playerIndex].board;
            if (oldGame == null || !oldGame.HexaTuples[playerIndex].board.IsSame(newBoard))
            {
                Debug.Log("[EVENT] OnChangedHexaBoard");
                OnChangedHexaBoard?.Invoke(newBoard);
            }

            if (oldGame == null || !oldGame.UnboundTileOffers.SequenceEqual(newGame.UnboundTileOffers))
            {
                Debug.Log("[EVENT] OnSelectionChanged");
                OnChangedHexaSelection?.Invoke(newGame.UnboundTileOffers);
            }

            if (oldGame == null || oldGame.PlayerTurn != newGame.PlayerTurn || oldGame.HexBoardRound != newGame.HexBoardRound)
            {
                Debug.Log("[EVENT] OnNextPlayerTurn");
                OnNextPlayerTurn?.Invoke(newGame.PlayerTurn);
            }

            if (oldGame == null || oldGame.HexBoardState != newGame.HexBoardState)
            {
                Debug.Log("[EVENT] OnBoardStateChanged");
                OnBoardStateChanged?.Invoke(newGame.HexBoardState);
            }
        }

        public void SetTrainGame(HexaGame newGame, int playerIndex)
        {
            HexaGame oldGame = null;
            if (HexaGame != null)
            {
                oldGame = (HexaGame)HexaGame.Clone();
            }
            // check for the event
            HexaGame = newGame;
            HexaGameDiff(oldGame, newGame, playerIndex);
        }

        public int? PlayerIndex(Account account) => HexaGame?.HexaTuples.FindIndex(p => p.player.Id.SequenceEqual(account.Bytes));

        public HexaPlayer Player(int playerIndex) => HexaGame?.CurrentPlayer;

        public HexaBoard Board(int playerIndex) => HexaGame?.CurrentPlayerBoard;
    }
}