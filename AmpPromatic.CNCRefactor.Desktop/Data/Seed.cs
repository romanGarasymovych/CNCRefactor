using AmpPromatic.CNCRefactor.Desktop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AmpPromatic.CNCRefactor.Desktop.Data
{
    internal class Seed
    {
        public static void InitialiizeData(DatabaseContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if(context.Machines.Any())
            {
                return;
            }
            Machine pega = context.Machines.Add(new Machine { Name = "PEGA 345", Extension = ".pnc" }).Entity;
            Machine vipros = context.Machines.Add(new Machine { Name = "VIPROS 255", Extension = ".vnc" }).Entity;
            context.SaveChanges();

            // TOOLS
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T220  RE 1 0.25  AI  )", Text = "(*T227  RE 1 0.25  AI  )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T256  RE 0.75 0.06   )", Text = "(*T210  RE 1 0.25  AI   )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T342  RO 0.5  )", Text = "(*T216  RO 0.5  )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T354  RO 0.1875  )", Text = "(*T206  RO 0.1875  )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T105  OB 0.5 0.1875   )", Text = "(*T324  OB 0.5 0.1875   )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T306  OB 0.75 0.5 90 N=306   )", Text = "(*T317  OB 0.75 0.5 90 N=306   )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "(*T345  RO 0.125  )", Text = "(*T303  RO 0.125  )" });
            context.Replacements.Add(new Replacement { MachineId = vipros.MachineId, TextToReplace = "G92X50. Y39.37", Text = "G92X47.638 Y50." });

            // Insertions
            context.Insertions.Add(new Insertion { MachineId = vipros.MachineId, Text = "G06 A0.036 B0", Qualifier = "G20", InsertionType = InsertionType.NextLine });
            context.Insertions.Add(new Insertion { MachineId = vipros.MachineId, Text = "M690", Qualifier = "G90", InsertionType = InsertionType.NextLine });
            context.Insertions.Add(new Insertion { MachineId = vipros.MachineId, Text = "M500", Qualifier = "M690", InsertionType = InsertionType.NextLine });

            // Transitions
            context.Transitions.Add(new Transition { MachineId = vipros.MachineId, OldText = "227", NewText = "227" });
            context.Transitions.Add(new Transition { MachineId = vipros.MachineId, OldText = "227", NewText = "210" });

            context.SaveChanges();
        }
    }
}
