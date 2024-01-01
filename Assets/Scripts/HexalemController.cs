using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public enum HexalemScreen
    {
        StartScreen,
        MainScreen,
        PlayScreen,
    }

    public enum HexalemSubScreen
    {
        MainChoose,
        Play,
        PlaySelect,
        PlayTileSelect,
        PlayTileUpgrade,
        PlayNextTurn,
        PlayFinish,
        PlayWaiting,
        PlayRanking,
        PlayTarget
    }

    public class HexalemController : ScreenStateMachine<HexalemScreen, HexalemSubScreen>
    {
        internal NetworkManager Network => NetworkManager.GetInstance();
        internal StorageManager Storage => StorageManager.GetInstance();

        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public Vector2 ScrollOffset { get; set; }

        public CacheData CacheData { get; private set; }

        public VisualElement VelContainer { get; private set; }

        internal string TempAccountName { get; set; } // TODO: remove this ....
        internal string TempAccountPassword { get; set; } // TODO: remove this ....
        internal string TempMnemonic { get; set; }  // TODO: remove this ....

        private void Awake()
        {
            base.Awake();
            // code after here

            CacheData = new CacheData();

        }

        protected override void InitializeStates()
        {
            // Initialize states
            _stateDictionary.Add(HexalemScreen.StartScreen, new StartScreen(this));

            var mainScreen = new MainScreenState(this);
            _stateDictionary.Add(HexalemScreen.MainScreen, mainScreen);

            var mainScreenSubStates = new Dictionary<HexalemSubScreen, IScreenState>
            {
                { HexalemSubScreen.MainChoose, new MainChooseSubState(this, mainScreen) },
                { HexalemSubScreen.Play, new MainPlaySubState(this, mainScreen) },
            };
            _subStateDictionary.Add(HexalemScreen.MainScreen, mainScreenSubStates);

            var playScreen = new PlayScreenState(this);
            _stateDictionary.Add(HexalemScreen.PlayScreen, playScreen);

            var playScreenSubStates = new Dictionary<HexalemSubScreen, IScreenState>
            {
                { HexalemSubScreen.PlaySelect, new PlaySelectSubState(this, playScreen) },
                { HexalemSubScreen.PlayTileSelect, new PlayTileSelectSubState(this, playScreen) },
                { HexalemSubScreen.PlayTileUpgrade, new PlayTileUpgradeSubState(this, playScreen) },
                { HexalemSubScreen.PlayNextTurn, new PlayNextTurnSubState(this, playScreen) },
                { HexalemSubScreen.PlayFinish, new PlayFinishSubState(this, playScreen) },
                { HexalemSubScreen.PlayWaiting, new PlayWaitingSubState(this, playScreen) },
                { HexalemSubScreen.PlayRanking, new PlayRankingSubState(this, playScreen) },
                { HexalemSubScreen.PlayTarget, new PlayTargetSubState(this, playScreen) },
            };
            _subStateDictionary.Add(HexalemScreen.PlayScreen, playScreenSubStates);
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            VelContainer = root.Q<VisualElement>("VelContainer");

            if (VelContainer.childCount > 1)
            {
                Debug.Log("Plaese remove development work, before starting!");
                return;
            }

            // call insital flow state
            ChangeScreenState(HexalemScreen.StartScreen);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Method intentionally left empty.
        }

    }
}