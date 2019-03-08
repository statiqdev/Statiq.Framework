using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Spectre.Cli;

namespace Wyam.App.Commands
{
    internal class BuildCommand : Command<BuildSettings>
    {
        public override int Execute(CommandContext context, BuildSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
