﻿using static TheSpaceRoles.Helper;
namespace TheSpaceRoles
{
    public class Sidekick : CustomRole
    {
        public Sidekick()
        {
            Team = Teams.Jackal;
            Role = Roles.Sidekick;
            Color = ColorFromColorcode("#00b4eb");
            CanUseVent = true;
        }
        public override void APDie(PlayerControl pc)
        {
            if (DataBase.AllPlayerRoles[pc.PlayerId].Team == Teams.Jackal)
            {
                if (DataBase.AllPlayerRoles[pc.PlayerId].Role != Roles.Sidekick)
                {
                    RoleSelect.ChangeMainRole(PlayerId, (int)Roles.Jackal);
                }
            }

        }
    }
}
