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

namespace org.gephi.layout.plugin.rotate
{
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// Sample layout that simply rotates the graph.
	/// 
	/// @author Helder Suzuki
	/// </summary>
	public class RotateLayout : AbstractLayout, Layout
	{

		private double angle;
		private Graph graph;

		public RotateLayout(LayoutBuilder layoutBuilder, double angle) : base(layoutBuilder)
		{
			this.angle = angle;
		}

		public override void initAlgo()
		{
			Converged = false;
		}

		public override void goAlgo()
		{
			graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				double sin = Math.Sin(-Angle.Value * Math.PI / 180);
				double cos = Math.Cos(-Angle.Value * Math.PI / 180);
				double px = 0f;
				double py = 0f;

				foreach (Node n in graph.Nodes)
				{
					if (!n.Fixed)
					{
						double dx = n.x() - px;
						double dy = n.y() - py;

						n.X = (float)(px + dx * cos - dy * sin);
						n.Y = (float)(py + dy * cos + dx * sin);
					}
				}
				Converged = true;
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override void endAlgo()
		{
		}

		public override void resetPropertiesValues()
		{
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "rotate.angle.name"), null, "clockwise.angle.name", NbBundle.getMessage(this.GetType(), "rotate.angle.desc"), "getAngle", "setAngle"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		/// <returns> the angle </returns>
		public virtual double? Angle
		{
			get
			{
				return angle;
			}
			set
			{
				this.angle = value.Value;
			}
		}

	}

}