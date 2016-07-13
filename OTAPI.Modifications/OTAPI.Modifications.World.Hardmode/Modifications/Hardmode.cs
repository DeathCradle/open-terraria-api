﻿using OTAPI.Patcher.Engine.Extensions;
using OTAPI.Patcher.Engine.Modification;
using System.Linq;

namespace OTAPI.Patcher.Engine.Modifications.Hooks.World
{
	public class Hardmode : ModificationBase
	{
		public override string Description => "Hooking WorldGen.StartHardmode()...";
		public override void Run()
		{
			var vanilla = SourceDefinition.Type("Terraria.WorldGen").Methods.Single(
				x => x.Name == "StartHardmode"
				&& x.Parameters.Count() == 0
			);


			var cbkBegin = ModificationDefinition.Type("OTAPI.Core.Callbacks.Terraria.WorldGen").Method("HardmodeBegin", parameters: vanilla.Parameters);
			var cbkEnd = ModificationDefinition.Type("OTAPI.Core.Callbacks.Terraria.WorldGen").Method("HardmodeEnd", parameters: vanilla.Parameters);

			vanilla.Wrap
			(
				beginCallback: cbkBegin,
				endCallback: cbkEnd,
				beginIsCancellable: true,
				noEndHandling: false,
				allowCallbackInstance: false
			);
		}
	}
}