using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class StartScreen : GameBaseState
    {
        private readonly Texture2D _portraitAlice;
        private readonly Texture2D _portraitBob;
        private readonly Texture2D _portraitCharlie;
        private readonly Texture2D _portraitDave;
        private readonly Texture2D _portraitCustom;

        private VisualElement _velPortrait;

        private Label _lblPlayerName;
        private Label _lblNodeType;

        private Button _btnEnter;

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

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/StartScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            _velPortrait = instance.Q<VisualElement>("VelPortrait");
            _lblPlayerName = instance.Q<Label>("LblPlayerName");

            _btnEnter = instance.Q<Button>("BtnEnter");
            _btnEnter.RegisterCallback<ClickEvent>(OnEnterClicked);

            _lblNodeType = instance.Q<Label>("LblNodeType");
            _lblNodeType.RegisterCallback<ClickEvent>(OnNodeTypeClicked);

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
            _lblPlayerName.style.color = GameConstant.FontLight;

            if (direction == Vector3.right)
            {
                switch (Network.CurrentAccountType)
                {
                    case AccountType.Alice:
                        Network.SetAccount(AccountType.Bob);
                        _velPortrait.style.backgroundImage = _portraitBob;
                        _lblPlayerName.text = AccountType.Bob.ToString();
                        break;

                    case AccountType.Bob:
                        Network.SetAccount(AccountType.Charlie);
                        _velPortrait.style.backgroundImage = _portraitCharlie;
                        _lblPlayerName.text = AccountType.Charlie.ToString();
                        break;

                    case AccountType.Charlie:
                        Network.SetAccount(AccountType.Dave);
                        _velPortrait.style.backgroundImage = _portraitDave;
                        _lblPlayerName.text = AccountType.Dave.ToString();
                        break;
                    case AccountType.Dave:
                        Network.SetAccount(AccountType.Custom);
                        _velPortrait.style.backgroundImage = _portraitCustom;
                        _lblPlayerName.text = AccountType.Custom.ToString();
                        _lblPlayerName.style.color = GameConstant.FontGreenish;
                         break;

                    case AccountType.Custom:
                    default:
                        break;
                }
            }
            else if (direction == Vector3.left)
            {
                switch (Network.CurrentAccountType)
                {
                    case AccountType.Bob:
                        Network.SetAccount(AccountType.Alice);
                        _velPortrait.style.backgroundImage = _portraitAlice;
                        _lblPlayerName.text = AccountType.Alice.ToString();
                        break;

                    case AccountType.Charlie:
                        Network.SetAccount(AccountType.Bob);
                        _velPortrait.style.backgroundImage = _portraitBob;
                        _lblPlayerName.text = AccountType.Bob.ToString();
                        break;

                    case AccountType.Dave:
                        Network.SetAccount(AccountType.Charlie);
                        _velPortrait.style.backgroundImage = _portraitCharlie;
                        _lblPlayerName.text = AccountType.Charlie.ToString();
                        break;

                    case AccountType.Custom:
                        Network.SetAccount(AccountType.Dave);
                        _velPortrait.style.backgroundImage = _portraitDave;
                        _lblPlayerName.text = AccountType.Dave.ToString();
                        break;

                    case AccountType.Alice:
                    default:
                        break;
                }
            }
        }

        private void OnEnterClicked(ClickEvent evt)
        {
            Debug.Log("Clicked enter button!");

            FlowController.ChangeScreenState(DemoGameScreen.MainScreen);
        }

        private void OnNodeTypeClicked(ClickEvent evt)
        {
            Network.ToggleNodeType();
            _lblNodeType.text = Network.CurrentNodeType.ToString();
        }
    }
}