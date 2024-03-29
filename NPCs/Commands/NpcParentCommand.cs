﻿using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using NPCs.Commands.SubCommands;

namespace NPCs.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class NpcParentCommand : ParentCommand
    {
        public NpcParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Spawn());
            RegisterCommand(new LookAtMe());
            RegisterCommand(new List());
            RegisterCommand(new Destroy());
            RegisterCommand(new AddItem());
            RegisterCommand(new Save());
            RegisterCommand(new Load());
        }

        public override string Command => "Npc";
        public override string[] Aliases { get; } = Array.Empty<string>();
        public override string Description => "The parent command NPC";

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "\nPlease enter a valid subcommand:";

            foreach (ICommand command in AllCommands)
            {
                if (sender.CheckPermission($"npc.{command.Command}"))
                    response = response + "\n\n<color=yellow><b>- " + command.Command + " (" + string.Join(", ", command.Aliases) + ")</b></color>\n<color=white>" + command.Description + "</color>";
            }

            return false;
        }
    }
}