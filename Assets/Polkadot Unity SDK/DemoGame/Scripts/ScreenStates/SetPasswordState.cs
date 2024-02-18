using Substrate.NET.Wallet;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class SetPasswordState: GameBaseState
    {
        private Button _btnCreateWallet;

        public SetPasswordState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("BottomBound");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Elements/SetPasswordElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWallet = instance.Q<Button>("BtnCreateWallet");
            _btnCreateWallet.SetEnabled(false);
            _btnCreateWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateWallet);

            var txfAccountPassword = instance.Q<HexalemTextField>("TxfAccountPassword");
            txfAccountPassword.TextField.RegisterValueChangedCallback(OnChangeEventAccountPassword);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Done, StepState.Done, StepState.Current);

            var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            var imgLogo = Resources.Load<Texture2D>($"DemoGame/Icons/IconOnboardPassword");
            velLogo.style.backgroundImage = imgLogo;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            if (FlowController.CurrentState == DemoGameScreen.UnlockWallet)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void OnClickBtnCreateWallet(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.VerifyPassword);
        }

        private void OnChangeEventAccountPassword(ChangeEvent<string> evt)
        {
            var accountPassword = evt.newValue;

            var isValidPassword = Wallet.IsValidPassword(accountPassword);
            Debug.Log($"IsPasswordValid = {isValidPassword}");

            if (!isValidPassword)
            {
                FlowController.TempAccountPassword = null;
                _btnCreateWallet.SetEnabled(false);
                return;
            }

            FlowController.TempAccountPassword = accountPassword;
            _btnCreateWallet.SetEnabled(true);
        }
    }
}