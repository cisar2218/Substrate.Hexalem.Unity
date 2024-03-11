using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;
using System.Xml.Linq;

namespace Assets.Polkadot_Unity_SDK.DemoGame.Scripts.ScreenSubState
{
    internal class PlayMatchmakingSubState : GameBaseState
    {
        private Label _advertisingOnJoinGame;
        private Label _currentPlayerRating;
        private VisualElement _matchAccepted;

        private VisualElement _playersAccepted;
        private VisualElement _playersInQueue;
        


        public PlayMatchmakingSubState(DemoGameController flowController, GameBaseState parent)
            : base(flowController, parent)
        {
            //_playerScoreElement = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Elements/PlayerScoreElement");
            //_waitingPlayersElements = new List<VisualElement>();
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

            _matchAccepted = elementInstance.Q<VisualElement>("VelJoinQueue");
            _matchAccepted.RegisterCallback<ClickEvent>(OnJoinQueueClicked);
            _advertisingOnJoinGame = elementInstance.Q<Label>("LblAdvertising");

            _currentPlayerRating = elementInstance.Q<Label>("RatingValue");
            _currentPlayerRating.text = "2000";

            _playersInQueue = elementInstance.Q<VisualElement>("VelPlayersQueue");
            _playersInQueue.Clear();

            for (int i = 0; i < 5; i++)
            {
                TemplateContainer playerEloInstance = ElementInstance("DemoGame/UI/Elements/PlayerEloElement");
                _playersInQueue.Add(playerEloInstance);
            }

            _playersAccepted = elementInstance.Q<VisualElement>("VelPlayersAccept");
            _playersAccepted.Clear();

            for (int i = 0; i < 3; i++)
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
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }

        public void OnJoinQueueClicked(ClickEvent evt)
        {
            // Call extrinsic and wait for confirmation
            OnCurrentPlayerJoinedQueue(); // Tmp
        }

        public void OnCurrentPlayerJoinedQueue()
        {
            TemplateContainer playerProfilElement = ElementInstance("DemoGame/UI/Elements/CurrentPlayerProfilElement");

            _matchAccepted.Clear();
            _matchAccepted.Add(playerProfilElement);

            _advertisingOnJoinGame.visible = false;
        }

        public void OnOtherPlayersJoinedQueue(int slotIndex)
        {

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
