using Assets.Scripts.ScreenStates;
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
        private Account _invitedFriendAccount;

        private string _subscriptionId;
        private Label _lblExtriniscUpdate;

        public MainSelectOnChainModeSubState(HexalemController flowController, ScreenBaseState parent)
            : base(flowController, parent) { }

        public override void EnterState()
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
            _velPortraitFriend.style.backgroundImage = AccountManager.GetInstance().GetPortrait(AccountManager.DefaultAccountType);

            _lblPlayerName = elementInstance.Q<Label>("LblPlayerName");
            _playerNameContainer = elementInstance.Q<VisualElement>("PlayerNameContainer");
            _playerNameInput = elementInstance.Q<TextField>("FieldPlayerCustomName");

            _lblExtriniscUpdate = elementInstance.Q<Label>("LblExtriniscUpdate");

            var btnGoBackMenu = elementInstance.Q<Button>("BtnMenu");
            btnGoBackMenu.RegisterCallback<ClickEvent>(OnBtnMenuClicked);

            Grid.OnSwipeEvent += OnSwipeEvent;

            scrollView.Add(elementInstance);

            Storage.OnStorageUpdated += OnStorageUpdated;
            Network.Client.ExtrinsicManager.ExtrinsicUpdated += OnExtrinsicUpdated;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

            Storage.OnStorageUpdated -= OnStorageUpdated;
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
            var subscriptionId = await Network.Client.CreateGameAsync(Network.Client.Account, players, 25, 1, CancellationToken.None);
            if (subscriptionId == null)
            {
                _btnInviteFriend.SetEnabled(true);
                _btnPlay.SetEnabled(true);
                return;
            }

            Debug.Log($"Extrinsic[CreateGameAsync] submited: {subscriptionId}");

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
                await CreateNewOnChainGameAsync(new List<Account>() { Network.Client.Account, _invitedFriendAccount });
            }
        }

        private async void OnBtnValidateInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.None;

            if (!Network.Client.ExtrinsicManager.Running.Any())
            {
                await CreateNewOnChainGameAsync(new List<Account>() { Network.Client.Account });
            }
        }

        private void OnBtnInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.Flex;
            _btnInviteFriend.style.display = DisplayStyle.None;
        }

        private void OnBtnCancelInviteFriendClicked(ClickEvent evt)
        {
            _velInviteFriend.style.display = DisplayStyle.None;
            _btnInviteFriend.style.display = DisplayStyle.Flex;
        }

        private void OnSwipeEvent(Vector3 direction)
        {
            PlayableAccount? selectedAccount = null;

            if (direction == Vector3.left)
            {
                selectedAccount = AccountManager.GetInstance().findNext(Network.CurrentAccountType);
            }
            else if (direction == Vector3.right)
            {
                selectedAccount = AccountManager.GetInstance().findPrevious(Network.CurrentAccountType);
            }

            if (selectedAccount != null)
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

                    Network.SetAccount(selectedAccount.Value.accountType);
                }

                _velPortraitFriend.style.backgroundImage = selectedAccount.Value.portrait;
            }
        }

        private void OnStorageUpdated(uint blocknumber)
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
                _btnPlay.text = "JOIN";
                _lblExtriniscUpdate.text = $"\"Hey bro, {Storage.HexaGame.PlayersCount} buddies, waiting!\"";
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
    }
}
