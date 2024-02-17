using Assets.Polkadot_Unity_SDK.DemoGame.Scripts;
using Substrate.NET.Wallet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class UnlockWalletState: GameBaseState
    {
        private Button _btnUnlockWallet;
        private VisualElement _velLogo;

        public UnlockWalletState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/LoginScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            var velReturnBox = instance.Q<VisualElement>("VelReturnBox");
            velReturnBox.style.visibility = Visibility.Hidden;
            var lblSubTitle = instance.Q<Label>("LblSubTitle");
            lblSubTitle.text = "Demo Wallet"; // could write 'Login' here
            _velLogo = instance.Q<VisualElement>("VelLogo");

            var velAccountBox = instance.Q<VisualElement>("VelAccountBox");
            var velAccountSelector = instance.Q<VisualElement>("VelAccountSelector");

            var lblAccountName = instance.Q<Label>("LblAccountName");
            var lblAccountAddress = instance.Q<Label>("LblAccountAddress");
            _btnUnlockWallet = instance.Q<Button>("BtnUnlockWallet");
            _btnUnlockWallet.SetEnabled(false);

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

            // add manipulators
            velAccountBox.RegisterCallback<ClickEvent>(OnAccountClicked);
            velAccountSelector.RegisterCallback<ClickEvent>(OnAccountSelectorClicked);
            _btnUnlockWallet.RegisterCallback<ClickEvent>(OnClickBtnUnlockWallet);
            txfPasswordInput.TextField.RegisterValueChangedCallback(OnChangeEventPasswordInput);

            // add container
            FlowController.VelContainer.Add(instance);

            _velLogo.style.transitionProperty = new List<StylePropertyName>() { "rotate" };
            _velLogo.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Linear };
            _velLogo.style.transitionDuration = new List<TimeValue> { new(500, TimeUnit.Millisecond) };
            _velLogo.style.transitionDelay = new List<TimeValue> { 0f };
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnChangeEventPasswordInput(ChangeEvent<string> evt)
        {
            var accountPassword = evt.newValue;

            if (!Wallet.IsValidPassword(accountPassword) || !Network.Wallet.IsStored)
            {
                FlowController.TempAccountPassword = null;
                _btnUnlockWallet.SetEnabled(false);
                return;
            }

            FlowController.TempAccountPassword = accountPassword;
            _btnUnlockWallet.SetEnabled(true);

            _velLogo.style.rotate = new StyleRotate(new Rotate(180)); // Angle.Turns(0.5f)
        }

        private void OnClickBtnUnlockWallet(ClickEvent evt)
        {
            if (!Network.Wallet.Unlock(FlowController.TempAccountPassword)) // need to be removed
            {
                Debug.Log("Couldn't unlock wallet!");
                return;
            }

            // make sure that the client account is also unlocked, not needed anymore we use only the wallet account
            //Network.Client.Account = Network.Wallet.Account;

            FlowController.ChangeScreenState(DemoGameScreen.LoadScreen);
        }

        private void OnAccountSelectorClicked(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.AccountSelection);
        }

        private void OnAccountClicked(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.OnBoarding);
        }
    }
}