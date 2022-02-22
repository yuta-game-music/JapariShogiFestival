using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.SettingPage.Detail
{
    public class DetailRowInt : DetailRow
    {
        public IntValue Value;
        public override string CurrentValueToString()
        {
            return $"{CurrentValue}<size=70%><color=#a0a0a0> / {MaxValue} </color></size>";
        }

        public int CurrentValue { get => GetValue(); set => SetValue(value); }
        public int MaxValue { get => GetMaxValue(); }
        public int MinValue { get => GetMinValue(); }

        public override bool ValidateValue()
        {
            var _prev = CurrentValue;
            CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
            return CurrentValue != _prev;
        }

        public void OnClickButton(int diff)
        {
            var _prev = CurrentValue;
            CurrentValue += diff;
            ValidateValue();
            if (CurrentValue != _prev)
            {
                UpdateAllValue();
            }
        }

        private int GetValue()
        {
            switch (Value)
            {
                case IntValue.BoardW: return GlobalVariable.BoardW;
                case IntValue.BoardH: return GlobalVariable.BoardH;
                case IntValue.BoardRealmH: return GlobalVariable.BoardRealmHeight;
                case IntValue.FriendsCount: return GlobalVariable.FriendsCount;
                case IntValue.InitialSandstar: return GlobalVariable.InitialSandstar;
                case IntValue.SandstarPerTurn: return GlobalVariable.GettingSandstarPerTurn;
                case IntValue.SandstarOnStay: return GlobalVariable.GettingSandstarOnWait;
                default:
                    Debug.LogWarning("Unknown Parameter: " + Value);
                    return 0;
            }
        }
        private void SetValue(int value)
        {
            switch (Value)
            {
                case IntValue.BoardW: GlobalVariable.BoardW = value; break;
                case IntValue.BoardH: GlobalVariable.BoardH = value; break;
                case IntValue.BoardRealmH: GlobalVariable.BoardRealmHeight = value; break;
                case IntValue.FriendsCount: GlobalVariable.FriendsCount = value; break;
                case IntValue.InitialSandstar: GlobalVariable.InitialSandstar = value; break;
                case IntValue.SandstarPerTurn: GlobalVariable.GettingSandstarPerTurn = value; break;
                case IntValue.SandstarOnStay: GlobalVariable.GettingSandstarOnWait = value; break;
                default:
                    Debug.LogWarning("Unknown Parameter: " + Value);
                    return;
            }
        }
        private int GetMaxValue()
        {
            switch (Value)
            {
                case IntValue.BoardW: return GlobalVariable.MAX_BOARD_W;
                case IntValue.BoardH: return GlobalVariable.MAX_BOARD_H;
                case IntValue.BoardRealmH: return GlobalVariable.BoardH / 2;
                case IntValue.FriendsCount: return GlobalVariable.BoardW * GlobalVariable.BoardRealmHeight;
                case IntValue.InitialSandstar: return 15;
                case IntValue.SandstarPerTurn: return 15;
                case IntValue.SandstarOnStay: return 15;
                default: return 0;
            }
        }
        private int GetMinValue()
        {
            switch (Value)
            {
                case IntValue.BoardW: return GlobalVariable.MIN_BOARD_W;
                case IntValue.BoardH: return GlobalVariable.MIN_BOARD_H;
                case IntValue.BoardRealmH: return 1;
                case IntValue.FriendsCount: return 2;
                case IntValue.InitialSandstar: return 0;
                case IntValue.SandstarPerTurn: return 0;
                case IntValue.SandstarOnStay: return 0;
                default: return 0;
            }
        }
    }

    public enum IntValue
    {
        BoardW, BoardH, BoardRealmH,
        FriendsCount,
        InitialSandstar, SandstarPerTurn, SandstarOnStay
    }

}