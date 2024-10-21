﻿using System.Linq;
using UnityEngine;
using static TheSpaceRoles.CustomOption;
using static TheSpaceRoles.CustomOptionsHolder;
using static TheSpaceRoles.Helper;
using static TheSpaceRoles.Ranges;

namespace TheSpaceRoles
{
    public class Vampire : CustomRole
    {
        public PlayerControl BittenPlayerControl;
        public Vampire()
        {

            team = Teams.Impostor;
            Role = Roles.Vampire;
            Color = Palette.ImpostorRed;
            HasKillButton = false;
        }
        public static CustomButton VampireBitebutton;
        public static CustomOption KillDelayTime;
        public static CustomOption KillCoolDown;
        public static CustomOption KillDistance;
        public static CustomOption UseGarlic;
        public static CustomOption GarlicAreaSize;
        public override void OptionCreate()
        {
            if (KillDelayTime != null) return;

            KillDelayTime = CustomOption.Create(CustomOption.OptionType.Impostor, "role.vampire.killdelaytime", new CustomFloatRange(1, 20, 1), 9);
            KillDistance = Create(CustomOption.OptionType.Impostor, "role.vampire.killdistance", KillDistanceRange(), 4);
            KillCoolDown = Create(CustomOption.OptionType.Impostor, "role.vampire.killcooldown", new CustomFloatRange(2.5f, 60, 2.5f), 12);
            UseGarlic = Create(CustomOption.OptionType.Impostor, "role.vampire.usegarlic", true);
            GarlicAreaSize = Create(CustomOption.OptionType.Impostor, "role.vampire.garlicareasize", new CustomFloatRange(1,10,1), 4, Show: UseGarlic.GetBoolValue);

            //キル遅延時間、キルク、ニンニク内だと特殊キルができなくなりその上でさらに通常キルもできなくなるかの設定
            Options = [KillDelayTime, KillCoolDown, GarlicAreaSize, KillDistance];
        }
        public override void HudManagerStart(HudManager __instance)
        {
            Logger.Warning($"{KillCoolDown.GetValue()},{KillDistance.GetValue()}");
            DataBase.AllPlayerRoles.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var r);
            VampireBitebutton = new CustomButton(
                __instance, "VampireKillButton",
                ButtonPos.Kill,
                KeyCode.Q,
                KillCoolDown.GetFloatValue(),
                () => KillButtons.KillButtonSetTarget(KillDistance.GetFloatValue(), Color, [Teams.Impostor]),
                __instance.KillButton.graphic.sprite,
                () =>
                {
                    BittenPlayerControl = GetPlayerControlFromId(KillButtons.KillButtonSetTarget(KillDistance.GetValue(), Color, [Teams.Impostor]));
                },
                () => VampireBitebutton.Timer = VampireBitebutton.maxTimer,
                "Kill",
                true, false, 10f, OnEffectEnd: () =>
                {
                    UnCheckedMurderPlayer.RpcMurder(PlayerControl.LocalPlayer, BittenPlayerControl, DeathReason.BittenByVampire, false);
                }); 

        }
    }
}
