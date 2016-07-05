﻿using Mono.Cecil;
using NDesk.Options;
using System;

namespace OTAPI.Patcher.Engine.Modification
{
	public abstract class ModificationBase
	{
		public AssemblyDefinition SourceDefinition { get; internal set; }
		public AssemblyDefinition ModificationDefinition { get; private set; }

		public const Int32 DefaultOrder = 5;

		protected ModificationBase()
		{
			var moduleLocation = this.GetType().Assembly.Location;
			ModificationDefinition = AssemblyDefinition.ReadAssembly(moduleLocation);
		}

		/// <summary>
		/// Occurs when the modification is triggered to run
		/// </summary>
		/// <param name="options"></param>
		public abstract void Run();

		/// <summary>
		/// Description of the modification running
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Determines the sort order for the current modification.
		/// </summary>
		/// <returns></returns>
		internal int GetOrder()
		{
			var attr = (OrderedAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(OrderedAttribute), true);
			if (attr != null)
				return attr.Order;

			return DefaultOrder;
		}
	}
}