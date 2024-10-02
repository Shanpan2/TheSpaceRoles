﻿using AmongUs.GameOptions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static TheSpaceRoles.Helper;

namespace TheSpaceRoles
{
    [HarmonyPatch]
    public static class GameStarter
    {
        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch(nameof(GameManager.StartGame)), HarmonyPostfix]
        public static void Reset()
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                DataBase.ResetAndPrepare();
                Logger.Info("FreePlayStart");
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.TeamType == RoleTeamTypes.Impostor)
                    {
                        SendRpcSetRole(Roles.Impostor, pc.PlayerId);
                    }
                    else
                    {
                        SendRpcSetRole(Roles.Crewmate, pc.PlayerId);

                    }
                }
            }
        }
        [HarmonyPatch(typeof(RoleManager))]
        [HarmonyPatch(nameof(RoleManager.SelectRoles)), HarmonyPostfix]

        public static void Select()
        {
            AmongUsClient.Instance.FinishRpcImmediately(Rpc.SendRpc(Rpcs.DataBaseReset));
            DataBase.ResetAndPrepare();
            foreach (int pid in DataBase.AllPlayerControls().Select(x => x.PlayerId))
            {
                var name = DataBase.AllPlayerControls().First(x => x.PlayerId == pid).cosmetics.nameText.text;

                name = Regex.Replace(name, "<color[^>]*?>", string.Empty);
                name = Regex.Replace(name, "<\\color[^>]*?>", string.Empty);
                DataBase.AllPlayerControls().First(x => x.PlayerId == pid).cosmetics.nameText.color = new();


            }


            if (AmongUsClient.Instance.AmHost)
            {
                //Resetするべ
                //今回はC3 I1
                //Sheriff 1
                //です
                Logger.Info("Owner");
                Dictionary<Teams, List<Roles>> tr = new();


                List<int> players = DataBase.AllPlayerControls().Select(x => (int)x.PlayerId).ToList();

                Logger.Info(players.Count.ToString());
                foreach (var keyValuePair in CustomOptionsHolder.RoleOptions_Count)
                {
                    var role = keyValuePair.Key;
                    var customOption = keyValuePair.Value.selection();
                    Logger.Info(role + $" is {customOption}");

                    for (int i = 0; i < customOption; i++)

                    {
                        if (tr.ContainsKey(RoleData.GetCustomRoleFromRole(role).team))
                        {

                            tr[RoleData.GetCustomRoleFromRole(role).team].Add(RoleData.GetCustomRoleFromRole(role).Role);
                        }
                        else
                        {
                            tr.Add(RoleData.GetCustomRoleFromRole(role).team, [RoleData.GetCustomRoleFromRole(role).Role]);

                        }
                        Logger.Info($"{customOption}");
                    }


                }
                if (DataBase.AllPlayerControls().Where(x => x.Data.RoleType == AmongUs.GameOptions.RoleTypes.Impostor).Count() > CustomOptionsHolder.TeamOptions_Count[Teams.Impostor].selection())
                {
                    for (int i = 0; i < DataBase.AllPlayerControls().Where(x => x.Data.RoleType == AmongUs.GameOptions.RoleTypes.Impostor).Count() - CustomOptionsHolder.TeamOptions_Count[Teams.Impostor].selection(); i++)
                    {

                        var k = DataBase.AllPlayerControls().Where(x => x.Data.RoleType == AmongUs.GameOptions.RoleTypes.Impostor).ToList();
                        k[RandomNext(k.Count())].RpcSetRole(AmongUs.GameOptions.RoleTypes.Crewmate);
                    }
                }
                else if (DataBase.AllPlayerControls().Where(x => x.Data.RoleType == AmongUs.GameOptions.RoleTypes.Impostor).Count() < CustomOptionsHolder.TeamOptions_Count[Teams.Impostor].selection())
                {
                    for (int i = 0; i < CustomOptionsHolder.TeamOptions_Count[Teams.Impostor].selection() - DataBase.AllPlayerControls().Where(x => x.Data.RoleType == AmongUs.GameOptions.RoleTypes.Impostor).Count(); i++)
                    {

                        var k = DataBase.AllPlayerControls().Where(x => x.Data.RoleType != AmongUs.GameOptions.RoleTypes.Impostor).ToList();
                        k[RandomNext(k.Count())].RpcSetRole(AmongUs.GameOptions.RoleTypes.Impostor);
                    }
                }

                List<int> p = DataBase.AllPlayerControls().Where(x => x.Data.RoleType == RoleTypes.Impostor).Select(x => (int)x.PlayerId).ToList();
                var team = Teams.Impostor;
                var teammembers = CustomOptionsHolder.TeamOptions_Count[team].selection();
                var v = tr[team];
                for (int i = 0; i < teammembers; i++)
                {
                    if (p.Count == 0) continue;
                    Logger.Info($"{team},{teammembers},{v.Count}");
                    if (v.Count == 0)
                    {

                        var playertag = RandomNext(p.Count);
                        SendRpcSetRole(RoleData.GetCustomRole_NormalFromTeam(team).Role, p[playertag]);
                        Logger.Info(p[playertag].ToString() + $" is {RoleData.GetCustomRole_NormalFromTeam(team).Role}");
                        players.Remove(p[playertag]);
                    }
                    else
                    {
                        var r = Helper.RandomNext(v.Count);
                        var role = v[r];
                        var playertag = RandomNext(p.Count);
                        SendRpcSetRole(role, p[playertag]);
                        Logger.Info(p[playertag].ToString() + $" is {role}");
                        players.Remove(p[playertag]);
                        v.RemoveAt(r);

                    }
                }
                Logger.Info("playercount" + players.Count.ToString());
                foreach (var t in tr)
                {
                    Logger.Info($"{t.Key}:{string.Join(",", t.Value.Select(x => x.ToString()))}");
                }
                foreach (var item in tr)
                {
                    team = item.Key;
                    if (team == Teams.Crewmate) continue;
                    if (team == Teams.Impostor) continue;
                    teammembers = CustomOptionsHolder.TeamOptions_Count[team].selection();
                    v = item.Value;
                    for (int i = 0; i < teammembers; i++)
                    {
                        if (players.Count == 0) continue;
                        Logger.Info($"{team},{teammembers},{v.Count}");
                        if (v.Count == 0)
                        {

                            var player = RandomNext(players.Count);
                            SendRpcSetRole(RoleData.GetCustomRole_NormalFromTeam(team).Role, players[player]);
                            Logger.Info(players[player].ToString() + $" is {RoleData.GetCustomRole_NormalFromTeam(team).Role}");
                            players.RemoveAt(player);
                        }
                        else
                        {
                            var r = Helper.RandomNext(v.Count);
                            var role = v[r];
                            var player = RandomNext(players.Count);
                            SendRpcSetRole(role, players[player]);
                            Logger.Info(players[player].ToString() + $" is {role}");
                            players.RemoveAt(player);
                            v.RemoveAt(r);

                        }
                    }
                }
                Logger.Info("playercount" + players.Count.ToString());
                team = Teams.Crewmate;
                teammembers = players.Count;
                v = tr[team];
                while (players.Count != 0)
                {
                    Logger.Info($"{team},{teammembers},{v.Count}");
                    if (v.Count == 0)
                    {

                        var playertag = RandomNext(players.Count);
                        SendRpcSetRole(RoleData.GetCustomRole_NormalFromTeam(team).Role, players[playertag]);
                        Logger.Info(players[playertag].ToString() + $" is {RoleData.GetCustomRole_NormalFromTeam(team).Role}");
                        players.RemoveAt(playertag);
                    }
                    else
                    {
                        var r = Helper.RandomNext(v.Count);
                        var role = v[r];
                        var playertag = RandomNext(players.Count);
                        SendRpcSetRole(role, players[playertag]);
                        Logger.Info(players[playertag].ToString() + $" is {role}");
                        players.RemoveAt(playertag);
                        v.RemoveAt(r);

                    }
                }
            }
        }
        public static void RemainingPlayerSetRoles()
        {
            foreach (var item in DataBase.AllPlayerControls().Select(x => x.PlayerId))
            {
                if (DataBase.AllPlayerRoles.ContainsKey(item))
                {

                }
                else
                {

                    var roles = RoleData.GetCustomRole_NormalFromTeam(DataBase.AllPlayerTeams[item]).Role;
                    SetRole(item, (int)roles);

                    //Rpc
                    var writer = Rpc.SendRpc(Rpcs.SetRole);
                    writer.Write((int)item);
                    writer.Write((int)roles);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }

        }
        public static int SendRpcSetRole(Roles role, int playerId)
        {
            //設定作ってないけどここでほんとは分岐
            //ここではmainとして扱う
            SetRole(playerId, (int)role);

            //Rpc
            var writer = Rpc.SendRpc(Rpcs.SetRole);
            writer.Write(playerId);
            writer.Write((int)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            return playerId;
        }
        public static void SetRole(int playerId, int roleId)
        {

            Logger.Info($"Player:{DataBase.AllPlayerControls().First(x => x.PlayerId == playerId).cosmetics.nameText.text}({playerId}) -> Role:{(Roles)roleId}");


            //if (DataBase.AllPlayerRoles.ContainsKey(playerId))
            //{
            //    var list = DataBase.AllPlayerRoles[playerId].ToList();
            //    var pl = RoleData.GetCustomRoleFromRole((Roles)roleId);
            //    pl.ReSet(playerId);
            //    pl.CustomTeam.Role = pl;
            //    list.Add(pl);
            //    DataBase.AllPlayerRoles[playerId] = [.. list];
            //}
            //else
            //{
            var p = RoleData.GetCustomRoleFromRole((Roles)roleId);
            p.ReSet(playerId);
            p.CustomTeam.Role = p;
            DataBase.AllPlayerRoles.Add(playerId, [p]);

            //}
            if (DataBase.AllPlayerTeams.ContainsKey(playerId))
            {

                DataBase.AllPlayerTeams[playerId] = RoleData.GetCustomRoleFromRole((Roles)roleId).team;
            }
            else
            {

                DataBase.AllPlayerTeams.Add(playerId, RoleData.GetCustomRoleFromRole((Roles)roleId).team);
            }

        }

        public static void GameStartAndPrepare()
        {
            //CustomRole_GameStart.GameStart();
        }
    }
}
