using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class StartScreen : ScreenBaseState
    {
        private Button _btnEnter;

        private VisualElement _velPortrait;
        private VisualElement _playerNameContainer;
        private TextField _playerNameInput;

        private Label _lblPlayerName;
        private Label _lblNodeType;

        public StartScreen(HexalemController _flowController)
            : base(_flowController) 
        {

        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/Screens/StartScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _velPortrait = instance.Q<VisualElement>("VelPortrait");
           _lblPlayerName = instance.Q<Label>("LblPlayerName");

            _btnEnter = instance.Q<Button>("BtnEnter");
            _btnEnter.RegisterCallback<ClickEvent>(OnEnterClicked);

            _lblNodeType = instance.Q<Label>("LblNodeType");
            _lblNodeType.RegisterCallback<ClickEvent>(OnNodeTypeClicked);

            _playerNameContainer = instance.Q<VisualElement>("PlayerNameContainer");
            _playerNameInput = instance.Q<TextField>("FieldPlayerCustomName");
            _playerNameInput.RegisterValueChangedCallback(OnCustomNameChanged);

            // initially select alice
            Network.SetAccount(AccountManager.DefaultAccountType);
            _velPortrait.style.backgroundImage = AccountManager.GetInstance().GetPortrait(AccountManager.DefaultAccountType);

            Grid.OnSwipeEvent += OnSwipeEvent;

            // add container
            FlowController.VelContainer.Add(instance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            Grid.OnSwipeEvent -= OnSwipeEvent;

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnSwipeEvent(Vector3 direction)
        {
            PlayableAccount? selectedAccount = null;

            if (direction == Vector3.left)
            {
                selectedAccount = AccountManager.GetInstance().findNextPlayableAccount(Network.CurrentAccountType);
            }
            else if (direction == Vector3.right)
            {
                selectedAccount = AccountManager.GetInstance().findPreviousPlayableAccount(Network.CurrentAccountType);
            }

            if(selectedAccount != null)
            {
                if (selectedAccount.Value.isCustom)
                {
                    _playerNameContainer.style.display = DisplayStyle.Flex;
                    _lblPlayerName.style.display = DisplayStyle.None;

                    Network.SetAccount(AccountType.Custom, _playerNameInput.text);
                    _btnEnter.SetEnabled(AccountManager.GetInstance().IsAccountNameValid(_playerNameInput.text));
                }
                else
                {
                    _playerNameContainer.style.display = DisplayStyle.None;
                    _lblPlayerName.style.display = DisplayStyle.Flex;

                    _lblPlayerName.text = selectedAccount.Value.name;
                    _btnEnter.SetEnabled(true);

                    Network.SetAccount(selectedAccount.Value.accountType);
                }

                _velPortrait.style.backgroundImage = selectedAccount.Value.portrait;
            }
        }

        

        private void OnEnterClicked(ClickEvent evt)
        {
            Debug.Log("Clicked enter button!"); 

            FlowController.ChangeScreenState(HexalemScreen.MainScreen);
        }

        private void OnNodeTypeClicked(ClickEvent evt)
        {
            Network.ToggleNodeType();
            _lblNodeType.text = Network.CurrentNodeType.ToString();
        }

        private void OnCustomNameChanged(ChangeEvent<string> evt)
        {
            Debug.Log($"New custom player account name = {evt.newValue}");

            if (Network.CurrentAccountType != AccountType.Custom) return;

            bool isNameValid = AccountManager.GetInstance().IsAccountNameValid(_playerNameInput.text);
            _btnEnter.SetEnabled(isNameValid);
            
            if(isNameValid)
            {
                Network.SetAccount(Network.CurrentAccountType, _playerNameInput.text);
            }
        }
    }
}