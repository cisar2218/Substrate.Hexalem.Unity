﻿using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class OnBoardingState: GameBaseState
    {
        public OnBoardingState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/OnboardingScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            var lblSubTitle = instance.Q<Label>("LblSubTitle");
            lblSubTitle.text = "Onboarding";

            // return box button
            var velReturnBox = instance.Q<VisualElement>("VelReturnBox");
            velReturnBox.style.visibility = Visibility.Visible;
            velReturnBox.RegisterCallback<ClickEvent>(OnClickReturn);

            // BtnCreateWallet
            var btnCreateWallet = instance.Q<Button>("BtnCreateWallet");
            btnCreateWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateWallet);

            // BtnImportJson
            var btnImportJson = instance.Q<Button>("BtnImportJson");
            btnImportJson.RegisterCallback<ClickEvent>(OnClickBtnImportJson);

            // BtnImportSeed
            var btnImportSeed = instance.Q<Button>("BtnImportSeed");
            btnImportSeed.RegisterCallback<ClickEvent>(OnClickBtnImportSeed);

            // Unload OnBoardStartElement from BottomBound

            // add container
            FlowController.VelContainer.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Current, StepState.None, StepState.None);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            if (FlowController.CurrentState == DemoGameScreen.StartScreen)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void OnClickReturn(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.StartScreen);
        }

        private void OnClickBtnCreateWallet(ClickEvent evt)
        {
            var randomBytes = new byte[16];
            FlowController.Random.GetBytes(randomBytes);
            FlowController.TempMnemonic = string.Join(" ", Mnemonic.MnemonicFromEntropy(randomBytes, Mnemonic.BIP39Wordlist.English));
            FlowController.TempAccount = Mnemonic.GetAccountFromMnemonic(FlowController.TempMnemonic, "", KeyType.Sr25519);
            Debug.Log($"Temporary account stored with keytype {FlowController.TempAccount.KeyType}.");

            FlowController.ChangeScreenState(DemoGameScreen.CreateWallet);
        }

        private void OnClickBtnImportJson(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.ImportJson);
        }

        private void OnClickBtnImportSeed(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoGameScreen.ImportSeed);
        }
    }
}