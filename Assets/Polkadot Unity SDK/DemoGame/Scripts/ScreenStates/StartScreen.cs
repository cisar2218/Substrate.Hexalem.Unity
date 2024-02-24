using Substrate.Integration.Client;
using Substrate.NET.Wallet;
using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Substrate.NetApi.Mnemonic;

namespace Assets.Scripts.ScreenStates
{
    public enum AccountState
    {
        None = 0,
        CreateAccount,
        CreatePassword,
        VerifyPassword,
    }

    public enum OptionState
    {
        SelectedWallet = 0,
        CreateAccount = 1,
        ImportAccount = 2,
    }

    public class StartScreen : GameBaseState
    {
        private readonly Texture2D _portraitAlice;
        private readonly Texture2D _portraitBob;
        private readonly Texture2D _portraitCharlie;
        private readonly Texture2D _portraitDave;
        private readonly Texture2D _portraitCustom;

        private VisualElement _velPortrait;
        private VisualElement _velChooseNetwork;

        private Label _lblPlayerName;
        private Label _lblNodeType;

        private Label _lblActionDescribtion;

        private TextField _txfCustomName;

        private Button _btnEnter;

        private OptionState _optionState;

        private AccountState _currentAccountState;

        private string _tempAccountName;
        private string _tempPassword;

        private int _optionIndex;

        public StartScreen(DemoGameController _flowController)
            : base(_flowController)
        {
            _portraitAlice = Resources.Load<Texture2D>($"DemoGame/Images/alice_portrait");
            _portraitBob = Resources.Load<Texture2D>($"DemoGame/Images/bob_portrait");
            _portraitCharlie = Resources.Load<Texture2D>($"DemoGame/Images/charlie_portrait");
            _portraitDave = Resources.Load<Texture2D>($"DemoGame/Images/dave_portrait");
            _portraitCustom = Resources.Load<Texture2D>($"DemoGame/Images/custom_portrait");
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            _optionIndex = 0;

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/StartScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _velPortrait = instance.Q<VisualElement>("VelPortrait");
            _lblPlayerName = instance.Q<Label>("LblPlayerName");
            _lblPlayerName.style.display = DisplayStyle.Flex;

            _lblActionDescribtion = instance.Q<Label>("LblActionDescribtion");
            SetOptionState(_optionIndex);

            _txfCustomName = instance.Q<TextField>("TxfCustomName");
            _txfCustomName.style.display = DisplayStyle.None;
            _txfCustomName.RegisterValueChangedCallback(OnCustomNameChanged);

            _btnEnter = instance.Q<Button>("BtnEnter");
            _btnEnter.RegisterCallback<ClickEvent>(OnEnterClicked);

            _velChooseNetwork = instance.Q<VisualElement>("VelChooseNetwork");

            _lblNodeType = instance.Q<Label>("LblNodeType");
            _lblNodeType.RegisterCallback<ClickEvent>(OnNodeTypeClicked);

            // initially select alice
            Debug.Log($"############## {Network.CurrentAccountName}");
            _velPortrait.style.backgroundImage = GetPortraitByName(Network.CurrentAccountName);
            _lblPlayerName.text = Network.CurrentAccountName;

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
            if (direction == Vector3.right)
            {
                if (!Network.NextWallet())
                {
                    SetOptionState(++_optionIndex);
                }
                
                _velPortrait.style.backgroundImage = GetPortraitByName(Network.CurrentAccountName);
                _lblPlayerName.text = Network.CurrentAccountName;

                switch (_optionState)
                {
                    case OptionState.SelectedWallet:
                        _currentAccountState = AccountState.None;
                        _lblPlayerName.style.display = DisplayStyle.Flex;
                        _txfCustomName.style.display = DisplayStyle.None;
                        break;

                    case OptionState.CreateAccount:
                        _currentAccountState = AccountState.CreateAccount;
                        _lblPlayerName.style.display = DisplayStyle.None;
                        _txfCustomName.style.display = DisplayStyle.Flex;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (direction == Vector3.left)
            {
                if (_optionIndex > 0)
                {
                    SetOptionState(--_optionIndex);
                }
                else
                {
                    Network.PrevWallet();
                }

                _velPortrait.style.backgroundImage = GetPortraitByName(Network.CurrentAccountName);
                _lblPlayerName.text = Network.CurrentAccountName;

                switch (_optionState)
                {
                    case OptionState.SelectedWallet:
                        _currentAccountState = AccountState.None;
                        _lblPlayerName.style.display = DisplayStyle.Flex;
                        _txfCustomName.style.display = DisplayStyle.None;
                        break;

                    case OptionState.CreateAccount:
                        _currentAccountState = AccountState.CreateAccount;
                        _lblPlayerName.style.display = DisplayStyle.None;
                        _txfCustomName.style.display = DisplayStyle.Flex;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private StyleBackground GetPortraitByName(string currentAccountName)
        {
            switch (currentAccountName)
            {
                case "Alice":
                    return new StyleBackground(_portraitAlice);

                case "Bob":
                    return new StyleBackground(_portraitBob);

                case "Charlie":
                    return new StyleBackground(_portraitCharlie);

                case "Dave":
                    return new StyleBackground(_portraitDave);

                default:
                    return new StyleBackground(_portraitCustom);
            }
        }

        private void SetOptionState(int optionIndex)
        {
            _optionState = (OptionState)optionIndex;

            switch (_optionState)
            {
                case OptionState.SelectedWallet:
                    _lblActionDescribtion.text = "Selected Account";
                    break;

                case OptionState.CreateAccount:
                    _lblActionDescribtion.text = "Enter new Account name";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnCustomNameChanged(ChangeEvent<string> evt)
        {
            if (!WordManager.StandardAccountName.IsValid(evt.newValue))
            {
                _btnEnter.SetEnabled(false);
                return;
            }

            switch (_currentAccountState)
            {
                case AccountState.CreateAccount:

                    if (!WordManager.StandardAccountName.IsValid(evt.newValue))
                    {
                        _tempAccountName = "";
                        _btnEnter.SetEnabled(false);
                        return;
                    }

                    _tempAccountName = evt.newValue;
                    break;

                case AccountState.CreatePassword:

                    if (!WordManager.StandardPassword.IsValid(evt.newValue))
                    {
                        _tempPassword = "";
                        _btnEnter.SetEnabled(false);
                        return;
                    }

                    _tempPassword = evt.newValue;
                    break;

                case AccountState.VerifyPassword:

                    if (string.IsNullOrEmpty(_tempPassword) || _tempPassword != evt.newValue)
                    {
                        _btnEnter.SetEnabled(false);
                        return;
                    }

                    break;
            }

            //Network.SetAccount(AccountType.Custom, evt.newValue);

            _btnEnter.SetEnabled(true);
        }

        private void OnEnterClicked(ClickEvent evt)
        {
            Debug.Log("Clicked enter button!");

            if (_optionIndex == 0)
            {
                FlowController.ChangeScreenState(DemoGameScreen.MainScreen);
                return;
            }

            if (_optionIndex == 1)
            {
                switch (_currentAccountState)
                {
                    case AccountState.CreateAccount:

                        // verify account name
                        _currentAccountState = AccountState.CreatePassword;
                        break;

                    case AccountState.CreatePassword:

                        // verify password name
                        _currentAccountState = AccountState.VerifyPassword;
                        break;

                    case AccountState.VerifyPassword:

                        var newMnemonic = Mnemonic.GenerateMnemonic(MnemonicSize.Words12);
                        Wallet wallet = Network.Keyring.AddFromMnemonic(newMnemonic, new Meta() { Name = _tempAccountName }, KeyType.Sr25519);

                        if (wallet.IsStored)
                        {
                            // TODO: show error message
                            Debug.Log($"Wallet already exists");
                            return;
                        }
                        wallet.Save(_tempAccountName, _tempPassword);
                        Debug.Log($"Save wallet {_tempAccountName} with password");

                        Network.ChangeWallet(wallet);

                        FlowController.ChangeScreenState(DemoGameScreen.MainScreen);
                        break;

                    default:
                        break;
                }
            }
        }

        private void OnNodeTypeClicked(ClickEvent evt)
        {
            Network.ToggleNodeType();
            _lblNodeType.text = Network.CurrentNodeType.ToString();
        }
    }
}