using Assets.Scripts;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;
using System.Threading;
using Substrate.Integration.Client;

namespace Assets.Polkadot_Unity_SDK.DemoGame.Scripts.ScreenSubState
{
    internal class PlayMatchmakingSubState : GameBaseState
    {
        private VisualElement _playersAccepted;
        private VisualElement _playersInQueueSameBracket;

        private Button _btnAcceptMatch;
        private string _acceptGameSubscriptionId;
        private string _forceMatchSubscriptionId = null;
        private Button _btnForceAcceptMatch;
        private Label _lblExtrinsicUpdate;

        private VisualElement _velWaitingQueue;
        private VisualElement _velMatchFound;

        public PlayMatchmakingSubState(DemoGameController flowController, GameBaseState parent)
            : base(flowController, parent)
        {
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer scrollViewElement = ElementInstance("DemoGame/UI/Elements/ScrollViewElement");
            floatBody.Add(scrollViewElement);

            var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

            TemplateContainer elementInstance = ElementInstance("DemoGame/UI/Frames/MatchmakingFrame");
            initPlayerInformation(elementInstance);

            _velWaitingQueue = elementInstance.Q<VisualElement>("VelWaiting");
            _velMatchFound = elementInstance.Q<VisualElement>("VelMatchFound");
            _playersInQueueSameBracket = elementInstance.Q<VisualElement>("VelPlayerQueueBox");

            _btnAcceptMatch = elementInstance.Q<Button>("BtnAcceptMatch");
            _btnAcceptMatch.SetEnabled(false);
            _btnAcceptMatch.RegisterCallback<ClickEvent>(OnGameAccepted);

            _btnForceAcceptMatch = elementInstance.Q<Button>("BtnForceAcceptMatch");
            _btnForceAcceptMatch.SetEnabled(false);
            _btnForceAcceptMatch.RegisterCallback<ClickEvent>(OnForceMatch);

            _lblExtrinsicUpdate = elementInstance.Q<Label>("LblExtrinsicUpdate");

            _playersAccepted = elementInstance.Q<VisualElement>("VelPlayersAccept");
            _playersAccepted.Clear();

            for (int i = 0; i < 1; i++)
            {
                TemplateContainer waitingSlotInstance = ElementInstance("DemoGame/UI/Elements/SlotElement");
                var waitingSlotContent = waitingSlotInstance.Q<VisualElement>("VelSlotContent");
                waitingSlotContent.Add(GetSlotTextContent(". . ."));

                _playersAccepted.Add(waitingSlotInstance);
            }

            TemplateContainer filledSlotInstance = ElementInstance("DemoGame/UI/Elements/SlotElement");
            var filledSlotContent = filledSlotInstance.Q<VisualElement>("VelSlotContent");
            TemplateContainer eloSlotInstance = ElementInstance("DemoGame/UI/Elements/PlayerEloElement");
            eloSlotInstance.style.marginTop = 20;
            filledSlotContent.Add(eloSlotInstance);

            _playersAccepted.Insert(0, filledSlotInstance);

            scrollView.Add(elementInstance);

            Network.Client.ExtrinsicManager.ExtrinsicUpdated += OnExtrinsicUpdated;
            Storage.OnGameFound += OnGameFound;
            Storage.OnForceStartMatch += OnForceMatchEnable;
            Storage.OnGameStarted += OnGameStarted;
            Storage.OnStorageUpdated += OnPlayerQueueSameBracketChange;
        }

        private void OnPlayerQueueSameBracketChange(uint blocknumber)
        {
            _playersInQueueSameBracket.Clear();
            Debug.Log($"[{nameof(PlayMatchmakingSubState)}] Player in same bracket = {Storage.PlayersInSameBracket.Count}");

            foreach (var playerSameBracket in Storage.PlayersInSameBracket)
            {
                TemplateContainer playerEloInstance = ElementInstance("DemoGame/UI/Elements/PlayerEloElement");
                var opponentEloRating = playerEloInstance.Q<Label>("LblEloRating");
                opponentEloRating.text = playerSameBracket.ToString();
                _playersInQueueSameBracket.Add(playerEloInstance);
            }
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
            Network.Client.ExtrinsicManager.ExtrinsicUpdated -= OnExtrinsicUpdated;
            Storage.OnGameFound -= OnGameFound;
            Storage.OnForceStartMatch -= OnForceMatchEnable;
            Storage.OnGameStarted -= OnGameStarted;
            Storage.OnStorageUpdated -= OnPlayerQueueSameBracketChange;
        }

        private void initPlayerInformation(TemplateContainer elementInstance)
        {
            
            var advertisingOnJoinGame = elementInstance.Q<Label>("LblAdvertising");
            var currentPlayerRating = elementInstance.Q<Label>("RatingValue");

            currentPlayerRating.text = Storage.AccountEloRating.ToString();

            advertisingOnJoinGame.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Matchmaking done, a game has been found
        /// </summary>
        /// <param name="gameId"></param>
        private void OnGameFound(byte[] gameId)
        {
            _velWaitingQueue.style.display = DisplayStyle.None;
            _velMatchFound.style.display = DisplayStyle.Flex;
            _btnAcceptMatch.SetEnabled(true);
        }

        /// <summary>
        /// Player click on accept match
        /// </summary>
        /// <param name="evt"></param>
        private async void OnGameAccepted(ClickEvent evt)
        {
            _acceptGameSubscriptionId = await Network.Client.AcceptAsync(Network.Client.Account, 1, CancellationToken.None);

            _btnAcceptMatch.style.display = DisplayStyle.None;
            _btnForceAcceptMatch.style.display = DisplayStyle.Flex;

            _btnForceAcceptMatch.text = "... Please wait ...";
        }

        public void OnGameStarted(byte[] gameId)
        {
            Debug.Log($"[{this.GetType().Name}][SUB] Game started");
            FlowController.ChangeScreenState(DemoGameScreen.PlayScreen);
        }

        private async void OnForceMatch(ClickEvent evt)
        {
            _forceMatchSubscriptionId = await Network.Client.ForceAcceptMatch(Network.Client.Account, 1, CancellationToken.None);

            _btnForceAcceptMatch.text = "... Game is starting...";
            _btnForceAcceptMatch.SetEnabled(false);
        }

        private void OnForceMatchEnable()
        {
            // If we already clicked on the button, we do nothing
            if (!string.IsNullOrEmpty(_forceMatchSubscriptionId)) return;

            _btnForceAcceptMatch.text = "Force start match !";
            _btnForceAcceptMatch.SetEnabled(true);
        }

        private void OnExtrinsicUpdated(string subscriptionId, ExtrinsicInfo extrinsicInfo)
        {
            if (_acceptGameSubscriptionId == null || _acceptGameSubscriptionId != subscriptionId)
            {
                return;
            }
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                switch (extrinsicInfo.TransactionEvent)
                {
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Validated:
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Finalized:
                        _lblExtrinsicUpdate.text = $"\"Awesome ! Get ready !\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Broadcasted:
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.BestChainBlockIncluded:
                        _lblExtrinsicUpdate.text = $"\"Hold on ! Hold oon ! Hold oooon !\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Error:
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Invalid:
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Dropped:
                        _lblExtrinsicUpdate.text = $"\"That doesn't work, bro!\"";
                        break;

                    default:
                        _lblExtrinsicUpdate.text = $"\"No blue, funk soul bro!\"";
                        break;
                }
            });
        }

        public Label GetSlotTextContent(string content)
        {
            var lbl = new Label();
            lbl.text = content;
            lbl.AddToClassList("slot-text-content");

            return lbl;
        }
    }
}
