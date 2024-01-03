using Assets.Scripts.Network;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public abstract class ScreenBaseState : IScreenState
    {
        public enum StepState
        {
            None,
            Current,
            Done
        }

        protected HexalemController FlowController { get; private set; }

        protected ScreenBaseState ParentState { get; private set; }

        protected FaucetManager Faucet => FaucetManager.GetInstance();
        protected NetworkManager Network => NetworkManager.GetInstance();

        protected StorageManager Storage => StorageManager.GetInstance();

        protected GridManager Grid => GridManager.GetInstance();

        protected ScreenBaseState(HexalemController flowController, ScreenBaseState parentState = null)
        {
            FlowController = flowController;
            ParentState = parentState;
        }

        public abstract void EnterState();

        public abstract void ExitState();

        public void UpdateState()
        {
            Debug.Log("Not implemented Updated currently ... ");
        }

        internal TemplateContainer ElementInstance(string elementPath, int widthPerc = 100, int heightPerc = 100)
        {
            var element = Resources.Load<VisualTreeAsset>(elementPath);
            var elementInstance = element.Instantiate();
            elementInstance.style.width = new Length(widthPerc, LengthUnit.Percent);
            elementInstance.style.height = new Length(heightPerc, LengthUnit.Percent);
            return elementInstance;
        }
    }
}