using Assets.Scripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Polkadot_Unity_SDK.DemoGame.Scripts.ScreenStates
{
    /// <summary>
    /// Component display when no accounts are currently saved (basically the first time the game is launch)
    /// </summary>
    public class FirstTimePlayState : GameBaseState
    {
        public FirstTimePlayState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var container = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            container.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Elements/FirstTimePlayElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            Button _btnCreateOrLoadWallet = instance.Q<Button>("BtnCreateOrLoadWallet");
            _btnCreateOrLoadWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateOrLoadWallet);

            container.Add(instance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnClickBtnCreateOrLoadWallet(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.OnBoarding);
        }
    }
}
