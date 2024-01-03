using Assets.Scripts.ScreenStates;
using Newtonsoft.Json.Linq;
using Substrate.Integration.Client;
using Substrate.NetApi.Model.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenSubState
{
    public class MainSelectOnChainModeSubState : ScreenBaseState
    {
        public MainScreenState PlayScreenState => ParentState as MainScreenState;

        private Button _btnPlay;
        private Button _btnInviteFriend;
        private Button _btnValidateInviteFriend;

        private VisualElement _velInviteFriend;
        private VisualElement _velPortraitFriend;
        private Label _lblPlayerName;
        private VisualElement _playerNameContainer;
        private TextField _playerNameInput;
        PlayableAccount _currentFriendselectedAccount;
        private Account _invitedFriendAccount;

        private string _subscriptionId;
        private Label _lblExtriniscUpdate;

        public MainSelectOnChainModeSubState(HexalemController flowController, ScreenBaseState parent)
            : base(flowController, parent) { }

        public async override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer scrollViewElement = ElementInstance("UI/Elements/ScrollViewElement");
            floatBody.Add(scrollViewElement);

            var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

            TemplateContainer elementInstance = ElementInstance("UI/Frames/GameModeFrame");

            _btnPlay = elementInstance.Q<Button>("BtnPlay");
            _btnPlay.SetEnabled(false);
            _btnPlay.RegisterCallback<ClickEvent>(OnBtnPlayClicked);

            _btnInviteFriend = elementInstance.Q<Button>("BtnInviteFriend");
            _btnInviteFriend.RegisterCallback<ClickEvent>(OnBtnInviteFriendClicked);

            var btnCancelInviteFriend = elementInstance.Q<Button>("BtnCancelInviteFriend");
            btnCancelInviteFriend.RegisterCallback<ClickEvent>(OnBtnCancelInviteFriendClicked);

            _btnValidateInviteFriend = elementInstance.Q<Button>("BtnValidateInviteFriend");
            _btnValidateInviteFriend.RegisterCallback<ClickEvent>(OnBtnValidateInviteFriendClicked);

            _velInviteFriend = elementInstance.Q<VisualElement>("VelInviteFriend");

            _velPortraitFriend = elementInstance.Q<VisualElement>("VelPortrait");

            _lblPlayerName = elementInstance.Q<Label>("LblPlayerName");
            _playerNameContainer = elementInstance.Q<VisualElement>("PlayerNameContainer");
            _playerNameInput = elementInstance.Q<TextField>("FieldPlayerCustomName");
            _playerNameInput.RegisterValueChangedCallback(OnCustomNameChanged);

            _lblExtriniscUpdate = elementInstance.Q<Label>("LblExtriniscUpdate");

            var btnGoBackMenu = elementInstance.Q<Button>("BtnMenu");
            btnGoBackMenu.RegisterCallback<ClickEvent>(OnBtnMenuClicked);

            Grid.OnSwipeEvent += OnSwipeEvent;

            scrollView.Add(elementInstance);

            Storage.OnStorageUpdated += OnStorageUpdated;
            Network.Client.ExtrinsicManager.ExtrinsicUpdated += OnExtrinsicUpdated;

            _currentFriendselectedAccount = AccountManager.GetInstance().PlayableAccounts.Last();
            DisplayAccount(_currentFriendselectedAccount);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

            Storage.OnStorageUpdated -= OnStorageUpdated;

            AccountManager.GetInstance().ClearAvailableAccounts();
        }

        private void OnBtnMenuClicked(ClickEvent _)
        {
            FlowController.ChangeScreenSubState(HexalemScreen.MainScreen, HexalemSubScreen.MainChoose);
        }

        private async Task CreateNewOnChainGameAsync(List<Account> players)
        {
            _btnInviteFriend.SetEnabled(false);
            _btnPlay.SetEnabled(false);
            _btnPlay.text = "WAIT";

            Debug.Log($"[{nameof(MainSelectOnChainModeSubState)} | {nameof(CreateNewOnChainGameAsync)}] with players {string.Join(", ", players.Select(x => Substrate.NetApi.Utils.GetAddressFrom(x.Bytes)))}");

            var subscriptionId = await Network.Client.CreateGameAsync(Network.Client.Account, players, 25, 1, CancellationToken.None);
            if (subscriptionId == null)
            {
                _btnInviteFriend.SetEnabled(true);
                _btnPlay.SetEnabled(true);
                return;
            }

            Debug.Log($"Extrinsic[{nameof(CreateNewOnChainGameAsync)}] submited: {subscriptionId}");

            _subscriptionId = subscriptionId;
        }

        private async void OnBtnPlayClicked(ClickEvent evt)
        {
            Storage.UpdateHexalem = true;

            if (Storage.HexaGame != null)
            {
                FlowController.ChangeScreenState(HexalemScreen.PlayScreen);
            }
            else if (!Network.Client.ExtrinsicManager.Running.Any())
            {
                await CreateNewOnChainGameAsync(new List<Account>() { Network.Client.Account });
            }
        }

        private async void OnBtnValidateInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.None;
            _btnInviteFriend.style.display = DisplayStyle.Flex;
            _btnPlay.style.display = DisplayStyle.Flex;

            if (!Network.Client.ExtrinsicManager.Running.Any())
            {
                await CreateNewOnChainGameAsync(new List<Account>() { Network.Client.Account, _invitedFriendAccount });
            }
        }

        private void OnBtnInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.Flex;

            _btnPlay.style.display = DisplayStyle.None;
            _btnInviteFriend.style.display = DisplayStyle.None;
        }

        private void OnBtnCancelInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.None;
            _btnInviteFriend.style.display = DisplayStyle.Flex;
            _btnPlay.style.display = DisplayStyle.Flex;
        }

        private async void OnSwipeEvent(Vector3 direction)
        {
            if (direction == Vector3.left)
            {
                _currentFriendselectedAccount = await AccountManager.GetInstance().findNextAvailableAccountAsync(_currentFriendselectedAccount.accountType, CancellationToken.None);
            }
            else if (direction == Vector3.right)
            {
                _currentFriendselectedAccount = await AccountManager.GetInstance().findPreviousAvailableAccountAsync(_currentFriendselectedAccount.accountType, CancellationToken.None);
            }

            DisplayAccount(_currentFriendselectedAccount);
        }

        private void DisplayAccount(PlayableAccount? selectedAccount)
        {
            if (selectedAccount.Value.isCustom)
            {
                _playerNameContainer.style.display = DisplayStyle.Flex;
                _lblPlayerName.style.display = DisplayStyle.None;

                _btnValidateInviteFriend.SetEnabled(AccountManager.GetInstance().IsAccountNameValid(_playerNameInput.text));

                _invitedFriendAccount = Network.GetAccount(selectedAccount.Value.accountType, _playerNameInput.text);
            }
            else
            {
                _playerNameContainer.style.display = DisplayStyle.None;
                _lblPlayerName.style.display = DisplayStyle.Flex;

                _lblPlayerName.text = selectedAccount.Value.name;
                _btnValidateInviteFriend.SetEnabled(true);

                _invitedFriendAccount = Network.GetAccount(selectedAccount.Value.accountType);
            }

            _velPortraitFriend.style.backgroundImage = selectedAccount.Value.portrait;
        }

        private async void OnStorageUpdated(uint blocknumber)
        {
            if (Network.Client.ExtrinsicManager.Running.Any())
            {
                _btnPlay.SetEnabled(false);
                _btnInviteFriend.SetEnabled(false);
                return;
            }

            Debug.Log($"[{nameof(MainSelectOnChainModeSubState)} | {nameof(OnStorageUpdated)}] HexaGame is {(Storage.HexaGame == null ? "null" : "not null")}");

            if (Storage.HexaGame == null)
            {
                _btnPlay.text = "CREATE";
                _lblExtriniscUpdate.text = "\"No pvp game to join, bro!\"";
                _btnInviteFriend.SetEnabled(true);
            }
            else
            {
                // You can now only join game where you are in players list
                var currentPlayerAddress = Substrate.NetApi.Utils.GetAddressFrom(Network.Client.Account.Bytes);
                var boardInstance = await NetworkManager.GetInstance().Client.GetBoardAsync(currentPlayerAddress, CancellationToken.None);
                var gameInstance = await NetworkManager.GetInstance().Client.GetGameAsync(boardInstance.GameId, CancellationToken.None);

                var playerIdx = gameInstance.Players.ToList().IndexOf(currentPlayerAddress);
                if (playerIdx >= 0)
                {
                    // If you are the game creator => JOIN
                    _btnPlay.text = playerIdx == 0 ? "JOIN" : "ACCEPT";
                    
                    _lblExtriniscUpdate.text = $"\"Hey bro, {Storage.HexaGame.PlayersCount} buddies, waiting!\"";
                }
                
            }

            _btnPlay.SetEnabled(true);
        }

        private void OnExtrinsicUpdated(string subscriptionId, ExtrinsicInfo extrinsicInfo)
        {
            if (_subscriptionId == null || _subscriptionId != subscriptionId)
            {
                return;
            }
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                switch (extrinsicInfo.TransactionEvent)
                {
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Validated:
                        _lblExtriniscUpdate.text = $"\"Oh bro, need to check what you sent me.\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Broadcasted:
                        _lblExtriniscUpdate.text = $"\"Pump the jam, let's shuffle the dices, gang.\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.BestChainBlockIncluded:
                        _lblExtriniscUpdate.text = $"\"Besti, bro!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Finalized:
                        _lblExtriniscUpdate.text = $"\"We got a stamp!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Error:
                        _lblExtriniscUpdate.text = $"\"That doesn't work, bro!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Invalid:
                        _lblExtriniscUpdate.text = $"\"Invalid, bro, your invalid!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Dropped:
                        _lblExtriniscUpdate.text = $"\"Gonna, drop this, bro.\"";
                        break;

                    default:
                        _lblExtriniscUpdate.text = $"\"No blue, funk soul bro!\"";
                        break;
                }
            });
        }

        private void OnCustomNameChanged(ChangeEvent<string> evt)
        {
            bool isNameValid = AccountManager.GetInstance().IsAccountNameValid(_playerNameInput.text);
            _btnValidateInviteFriend.SetEnabled(isNameValid);
        }
    }
}
