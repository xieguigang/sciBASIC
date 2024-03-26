using System.Collections.Generic;

/*
Copyright 2008-2010 Gephi
Authors : Mathieu Bastian <mathieu.bastian@gephi.org>
Website : http://www.gephi.org

This file is part of Gephi.

DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.

Copyright 2011 Gephi Consortium. All rights reserved.

The contents of this file are subject to the terms of either the GNU
General Public License Version 3 only ("GPL") or the Common
Development and Distribution License("CDDL") (collectively, the
"License"). You may not use this file except in compliance with the
License. You can obtain a copy of the License at
http://gephi.org/about/legal/license-notice/
or /cddl-1.0.txt and /gpl-3.0.txt. See the License for the
specific language governing permissions and limitations under the
License.  When distributing the software, include this License Header
Notice in each file and include the License files at
/cddl-1.0.txt and /gpl-3.0.txt. If applicable, add the following below the
License Header, with the fields enclosed by brackets [] replaced by
your own identifying information:
"Portions Copyrighted [year] [name of copyright owner]"

If you wish your version of this file to be governed by only the CDDL
or only the GPL Version 3, indicate your decision by adding
"[Contributor] elects to include this software in this distribution
under the [CDDL or GPL Version 3] license." If you do not indicate a
single choice of license, a recipient has the option to distribute
your version of this file under either the CDDL, the GPL Version 3 or
to extend the choice of license to its licensees as provided above.
However, if you add GPL Version 3 code and therefore, elected the GPL
Version 3 license, then the option applies only if the new code is
made subject to such option by the copyright holder.

Contributor(s):

Portions Copyrighted 2011 Gephi Consortium.
 */

namespace org.gephi.layout.plugin.openord
{
	/// <summary>
	/// @author Mathieu Bastian
	/// </summary>
	public sealed class Params
	{

		public static readonly Params DEFAULT = new Params("DEFAULT", InnerEnum.DEFAULT, new Stage(0, 2000f, 10f, 1f), new Stage(0.25f, 2000f, 10f, 1f), new Stage(0.25f, 2000f, 2f, 1f), new Stage(0.25f, 2000f, 1f, 0.1f), new Stage(0.10f, 250f, 1f, 0.25f), new Stage(0.15f, 250f, 0.5f, 0f));
		public static readonly Params COARSEN = new Params("COARSEN", InnerEnum.COARSEN, new Stage(0, 2000f, 10f, 1f), new Stage(200, 2000f, 2f, 1f), new Stage(200, 2000f, 10f, 1f), new Stage(200, 2000f, 1f, 0.1f), new Stage(50, 250f, 1f, 0.25f), new Stage(100, 250f, 0.5f, 0f));
		public static readonly Params COARSEST = new Params("COARSEST", InnerEnum.COARSEST, new Stage(0, 2000f, 10f, 1f), new Stage(200, 2000f, 2f, 1f), new Stage(200, 2000f, 10f, 1f), new Stage(200, 2000f, 1f, 0.1f), new Stage(200, 250f, 1f, 0.25f), new Stage(100, 250f, 0.5f, 0f));
		public static readonly Params REFINE = new Params("REFINE", InnerEnum.REFINE, new Stage(0, 50f, 0.5f, 0f), new Stage(0, 2000f, 2f, 1f), new Stage(50, 500f, 0.1f, 0.25f), new Stage(50, 200f, 1f, 0.1f), new Stage(50, 250f, 1f, 0.25f), new Stage(0, 250f, 0.5f, 0f));
		public static readonly Params FINAL = new Params("FINAL", InnerEnum.FINAL, new Stage(0, 50f, 0.5f, 0f), new Stage(0, 2000f, 2f, 1f), new Stage(50, 50f, 0.1f, 0.25f), new Stage(50, 200f, 1f, 0.1f), new Stage(50, 250f, 1f, 0.25f), new Stage(25, 250f, 0.5f, 0f));

		private static readonly IList<Params> valueList = new List<Params>();

		static Params()
		{
			valueList.Add(DEFAULT);
			valueList.Add(COARSEN);
			valueList.Add(COARSEST);
			valueList.Add(REFINE);
			valueList.Add(FINAL);
		}

		public enum InnerEnum
		{
			DEFAULT,
			COARSEN,
			COARSEST,
			REFINE,
			FINAL
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;
		private readonly Stage initial;
		private readonly Stage liquid;
		private readonly Stage expansion;
		private readonly Stage cooldown;
		private readonly Stage crunch;
		private readonly Stage simmer;

		internal Params(string name, InnerEnum innerEnum, Stage initial, Stage liquid, Stage expansion, Stage cooldown, Stage crunch, Stage simmer)
		{
			this.initial = initial;
			this.liquid = liquid;
			this.expansion = expansion;
			this.cooldown = cooldown;
			this.crunch = crunch;
			this.simmer = simmer;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public Stage Cooldown
		{
			get
			{
				return cooldown;
			}
		}

		public Stage Crunch
		{
			get
			{
				return crunch;
			}
		}

		public Stage Expansion
		{
			get
			{
				return expansion;
			}
		}

		public Stage Initial
		{
			get
			{
				return initial;
			}
		}

		public Stage Liquid
		{
			get
			{
				return liquid;
			}
		}

		public Stage Simmer
		{
			get
			{
				return simmer;
			}
		}

		public float IterationsSum
		{
			get
			{
				return liquid.iterations + expansion.iterations + cooldown.iterations + crunch.iterations + simmer.iterations;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		public static class Stage
	//	{
	//
	//		private final float temperature;
	//		private final float attraction;
	//		private final float dampingMult;
	//		private float iterations;
	//
	//		Stage(float iterations, float temperature, float attraction, float dampingMult)
	//		{
	//			this.iterations = iterations;
	//			this.temperature = temperature;
	//			this.attraction = attraction;
	//			this.dampingMult = dampingMult;
	//		}
	//
	//		public float getAttraction()
	//		{
	//			return attraction;
	//		}
	//
	//		public float getDampingMult()
	//		{
	//			return dampingMult;
	//		}
	//
	//		public float getIterations()
	//		{
	//			return iterations;
	//		}
	//
	//		public void setIterations(float iterations)
	//		{
	//			this.iterations = iterations;
	//		}
	//
	//		public int getIterationsTotal(int totalIterations)
	//		{
	//			return (int)(iterations * totalIterations);
	//		}
	//
	//		public int getIterationsPercentage()
	//		{
	//			return (int)(iterations * 100f);
	//		}
	//
	//		public float getTemperature()
	//		{
	//			return temperature;
	//		}
	//	}

		public static IList<Params> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static Params valueOf(string name)
		{
			foreach (Params enumInstance in Params.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}