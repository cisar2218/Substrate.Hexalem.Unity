﻿using Substrate.NET.Wallet;
using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class VerifyPasswordState: GameBaseState
    {
        private Button _btnCreateWallet;

        public VerifyPasswordState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Elements/VerifyPasswordElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWallet = instance.Q<Button>("BtnCreateWallet");
            _btnCreateWallet.SetEnabled(false);
            _btnCreateWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateWallet);

            var txfVerifyPassword = instance.Q<HexalemTextField>("TxfVerifyPassword");
            txfVerifyPassword.TextField.RegisterValueChangedCallback(OnChangeEventVerifyPassword);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Done, StepState.Done, StepState.Current);

            //var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            //var imgLogo = Resources.Load<Texture2D>($"DemoGame/Icons/IconOnboardVerify");
            //velLogo.style.backgroundImage = imgLogo;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnClickBtnCreateWallet(ClickEvent evt)
        {
            Wallet wallet;
            try
            {
                wallet = Network.Keyring.AddFromUri(
                    FlowController.TempMnemonic, new 
                    Meta() { Name = FlowController.TempAccountName }, 
                    KeyType.Sr25519);

                var unlockSucceed = wallet.Unlock(FlowController.TempAccountPassword);

                Debug.Log($"[{nameof(VerifyPasswordState)}] - Wallet address = {wallet.Address} | Unlock succeed = {unlockSucceed} | IsLocked = {wallet.IsLocked} | IsStored = {wallet.IsStored}");

                if (!unlockSucceed)
                {
                    Debug.Log($"Wallet successfully load, but invalid password to unlock");
                    return;
                }

                if(!wallet.IsStored)
                {
                    //var walletFile = wallet.ToWalletFile(FlowController.TempAccountName, FlowController.TempAccountPassword);

                    //Debug.Log($"Try to save wallet | {FlowController.TempAccountName} - {FlowController.TempAccountPassword} | {wallet.AccountNamePolicy} | {wallet.PasswordPolicy} | {SystemInteraction.Persist} | Wallet File = {walletFile.ToJson()}");
                    
                    wallet.Save(FlowController.TempAccountName, FlowController.TempAccountPassword);
                }

            } catch(System.Exception ex)
            {
                Debug.Log($"Failed to create {FlowController.TempAccountName} wallet ! : {ex.Message}");
                return;
            }

            if (!Network.ChangeWallet(wallet))
            {
                Debug.Log($"Couldn't change to {FlowController.TempAccountName} wallet!");
                return;
            }

            Debug.Log($"Create wallet {FlowController.TempAccountName} with address {wallet.Address} successful!");
            FlowController.ChangeScreenState(DemoGameScreen.MainScreen);
        }

        private void OnChangeEventVerifyPassword(ChangeEvent<string> evt)
        {
            var accountPassword = evt.newValue;

            if (accountPassword != FlowController.TempAccountPassword)
            {
                _btnCreateWallet.SetEnabled(false);
                return;
            }

            _btnCreateWallet.SetEnabled(true);
        }
    }
}