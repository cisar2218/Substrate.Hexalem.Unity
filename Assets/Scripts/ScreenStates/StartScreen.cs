using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class StartScreen : ScreenBaseState
    {
        private readonly Texture2D _portraitAlice;
        private readonly Texture2D _portraitBob;
        private readonly Texture2D _portraitCharlie;
        private readonly Texture2D _portraitDave;
        private readonly Texture2D _portraitCustom;

        private Button _btnEnter;

        private List<PlayableAccount> _playableAccounts =>
            new()
            {
                new(AccountType.Alice, _portraitAlice, false, "Alice"),
                new(AccountType.Bob, _portraitBob, false, "Bob"),
                new(AccountType.Charlie, _portraitCharlie, false, "Charlie"),
                new(AccountType.Dave, _portraitDave, false, "Dave"),
                new(AccountType.Custom, _portraitCustom, true, string.Empty),
            };

        private VisualElement _velPortrait;
        private VisualElement _playerNameContainer;
        private TextField _playerNameInput;


        private Label _lblPlayerName;
        private Label _lblNodeType;

        public StartScreen(HexalemController _flowController)
            : base(_flowController) 
        {
            _portraitAlice = Resources.Load<Texture2D>($"Images/alice_portrait");
            _portraitBob = Resources.Load<Texture2D>($"Images/bob_portrait");
            _portraitCharlie = Resources.Load<Texture2D>($"Images/charlie_portrait");
            _portraitDave = Resources.Load<Texture2D>($"Images/dave_portrait");
            _portraitCustom = Resources.Load<Texture2D>($"Images/custom_account_portrait");
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
            Network.SetAccount(AccountType.Alice);
            _velPortrait.style.backgroundImage = _portraitAlice;

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
                selectedAccount = findNext();
            }
            else if (direction == Vector3.right)
            {
                selectedAccount = findPrevious();
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

        private PlayableAccount findNext()
        {
            var idx = _playableAccounts.FindIndex(x => x.accountType == Network.CurrentAccountType);

            if (idx + 1 >= _playableAccounts.Count) return _playableAccounts[idx];
            return _playableAccounts[idx + 1];
        }

        private PlayableAccount findPrevious()
        {
            var idx = _playableAccounts.FindIndex(x => x.accountType == Network.CurrentAccountType);

            if (idx - 1 < 0) return _playableAccounts[idx];
            return _playableAccounts[idx - 1];
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

    internal struct PlayableAccount
    {
        public AccountType accountType;
        public string name;
        public Texture2D portrait;
        public bool isCustom;

        public PlayableAccount(AccountType accountType, Texture2D portrait, bool isCustom, string name)
        {
            this.accountType = accountType;
            this.portrait = portrait;
            this.isCustom = isCustom;
            this.name = name;
        }
    }
}