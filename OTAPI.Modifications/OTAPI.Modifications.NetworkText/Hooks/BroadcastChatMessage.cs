﻿using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OTAPI.Modifications.NetworkText;

namespace OTAPI
{
	public static partial class Hooks
	{
		public static class BroadcastChatMessage
		{
			public static BeforeChatMessageHandler BeforeBroadcastChatMessage;
			public static AfterChatMessageHandler AfterBroadcastChatMessage;
		}
	}
}
