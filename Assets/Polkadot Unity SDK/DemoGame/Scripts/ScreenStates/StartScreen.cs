using Substrate.NET.Wallet;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class StartScreen : GameBaseState
    {
        private Label _lblNodeType;

        private Button _btnEnter;

        public StartScreen(DemoGameController _flowController)
            : base(_flowController)
        {
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/StartScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            var velAccountBox = instance.Q<VisualElement>("VelAccountBox");
            var velAccountSelector = instance.Q<VisualElement>("VelAccountSelector");

            velAccountBox.RegisterCallback<ClickEvent>(OnAccountClicked);
            velAccountSelector.RegisterCallback<ClickEvent>(OnAccountSelectorClicked);

            var lblAccountName = instance.Q<Label>("LblAccountName");
            var lblAccountAddress = instance.Q<Label>("LblAccountAddress");

            var txfPasswordInput = instance.Q<HexalemTextField>("TxfPasswordInput");

            if (Network.Wallet != null && Network.Wallet.IsStored)
            {
                lblAccountName.text = FlowController.Network.Wallet.FileName;
                lblAccountAddress.text = FlowController.Network.Wallet.Account.Value;
            }
            else
            {
                txfPasswordInput.SetEnabled(false);
            }
            txfPasswordInput.TextField.RegisterValueChangedCallback(OnChangeEventPasswordInput);

            _btnEnter = instance.Q<Button>("BtnUnlockWallet");
            _btnEnter.SetEnabled(false);
            _btnEnter.RegisterCallback<ClickEvent>(OnEnterClicked);

            _lblNodeType = instance.Q<Label>("LblNodeType");
            _lblNodeType.RegisterCallback<ClickEvent>(OnNodeTypeClicked);

            //Grid.OnSwipeEvent += OnSwipeEvent;

            // add container
            FlowController.VelContainer.Add(instance);

            // If no wallet stored, we only display a create wallet button
            if (!Network.StoredWallets().Any())
            {
                FlowController.ChangeScreenState(DemoGameScreen.FirstTimeScreen);
            }
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            if (FlowController.CurrentState != DemoGameScreen.FirstTimeScreen)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void OnEnterClicked(ClickEvent evt)
        {
            Debug.Log("Clicked enter button!");
            if (!Network.Wallet.Unlock(FlowController.TempAccountPassword))
            {
                Debug.Log("Couldn't unlock wallet!");
                return;
            }

            FlowController.ChangeScreenState(DemoGameScreen.MainScreen);
        }

        private void OnNodeTypeClicked(ClickEvent evt)
        {
            Network.ToggleNodeType();
            _lblNodeType.text = Network.CurrentNodeType.ToString();
        }

        private void OnAccountSelectorClicked(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.AccountSelection);
        }

        private void OnAccountClicked(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.OnBoarding);
        }

        private void OnChangeEventPasswordInput(ChangeEvent<string> evt)
        {
            var accountPassword = evt.newValue;

            if (!Wallet.IsValidPassword(accountPassword) || Network.Wallet == null || !Network.Wallet.IsStored)
            {
                FlowController.TempAccountPassword = null;
                _btnEnter.SetEnabled(false);
                return;
            }

            FlowController.TempAccountPassword = accountPassword;
            _btnEnter.SetEnabled(true);

            //_velLogo.style.rotate = new StyleRotate(new Rotate(180)); // Angle.Turns(0.5f)
        }
    }
}