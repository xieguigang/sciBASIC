using System;
using System.Collections.Generic;

/*
 Copyright 2008-2010 Gephi
 Authors : Helder Suzuki <heldersuzuki@gephi.org>
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

namespace org.gephi.layout.plugin.random
{
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// @author Helder Suzuki
	/// </summary>
	public class RandomLayout : AbstractLayout, Layout
	{

		private readonly System.Random random;
		private Graph graph;
		private bool converged;
		private double size;

		public RandomLayout(LayoutBuilder layoutBuilder, double size) : base(layoutBuilder)
		{
			this.size = size;
			random = new System.Random();
		}

		public override void initAlgo()
		{
			converged = false;
		}

		public override void goAlgo()
		{
			graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				foreach (Node n in graph.Nodes)
				{
					if (!n.Fixed)
					{
						n.X = (float)(-size / 2 + size * random.NextDouble());
						n.Y = (float)(-size / 2 + size * random.NextDouble());
					}
				}
				converged = true;
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override bool canAlgo()
		{
			return !converged;
		}

		public override void endAlgo()
		{
			graph = null;
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "Random.spaceSize.name"), null, "Random.spaceSize.name", NbBundle.getMessage(this.GetType(), "Random.spaceSize.desc"), "getSize", "setSize"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		public override void resetPropertiesValues()
		{
		}

		public virtual double? Size
		{
			get
			{
				return size;
			}
			set
			{
				this.size = value.Value;
			}
		}

	}

}