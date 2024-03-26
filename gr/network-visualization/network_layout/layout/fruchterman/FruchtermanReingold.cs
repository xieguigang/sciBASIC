using System;
using System.Collections.Generic;

/*
 Copyright 2008-2010 Gephi
 Authors : Mathieu Jacomy
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

namespace org.gephi.layout.plugin.fruchterman
{
	using Edge = org.gephi.graph.api.Edge;
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// @author Mathieu Jacomy
	/// </summary>
	public class FruchtermanReingold : AbstractLayout, Layout
	{

		private const float SPEED_DIVISOR = 800;
		private const float AREA_MULTIPLICATOR = 10000;
		//Graph
		protected internal Graph graph;
		//Properties
		private float area;
		private double gravity;
		private double speed;

		public FruchtermanReingold(LayoutBuilder layoutBuilder) : base(layoutBuilder)
		{
		}

		public override void resetPropertiesValues()
		{
			speed = 1;
			area = 10000;
			gravity = 10;
		}

		public override void initAlgo()
		{
		}

		public override void goAlgo()
		{
			this.graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				Node[] nodes = graph.Nodes.toArray();
				Edge[] edges = graph.Edges.toArray();

				foreach (Node n in nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is ForceVectorNodeLayoutData))
					{
						n.LayoutData = new ForceVectorNodeLayoutData();
					}
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					layoutData.dx = 0;
					layoutData.dy = 0;
				}

				float maxDisplace = (float)(Math.Sqrt(AREA_MULTIPLICATOR * area) / 10f); // Déplacement limite : on peut le calibrer...
				float k = (float) Math.Sqrt((AREA_MULTIPLICATOR * area) / (1f + nodes.Length)); // La variable k, l'idée principale du layout.

				foreach (Node N1 in nodes)
				{
					foreach (Node N2 in nodes)
					{ // On fait toutes les paires de noeuds
						if (N1 != N2)
						{
							float xDist = N1.x() - N2.x(); // distance en x entre les deux noeuds
							float yDist = N1.y() - N2.y();
							float dist = (float) Math.Sqrt(xDist * xDist + yDist * yDist); // distance tout court

							if (dist > 0)
							{
								float repulsiveF = k * k / dist; // Force de répulsion
								ForceVectorNodeLayoutData layoutData = N1.LayoutData;
								layoutData.dx += xDist / dist * repulsiveF; // on l'applique...
								layoutData.dy += yDist / dist * repulsiveF;
							}
						}
					}
				}
				foreach (Edge E in edges)
				{
					// Idem, pour tous les noeuds on applique la force d'attraction

					Node Nf = E.Source;
					Node Nt = E.Target;

					float xDist = Nf.x() - Nt.x();
					float yDist = Nf.y() - Nt.y();
					float dist = (float) Math.Sqrt(xDist * xDist + yDist * yDist);

					float attractiveF = dist * dist / k;

					if (dist > 0)
					{
						ForceVectorNodeLayoutData sourceLayoutData = Nf.LayoutData;
						ForceVectorNodeLayoutData targetLayoutData = Nt.LayoutData;
						sourceLayoutData.dx -= xDist / dist * attractiveF;
						sourceLayoutData.dy -= yDist / dist * attractiveF;
						targetLayoutData.dx += xDist / dist * attractiveF;
						targetLayoutData.dy += yDist / dist * attractiveF;
					}
				}
				// gravity
				foreach (Node n in nodes)
				{
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					float d = (float) Math.Sqrt(n.x() * n.x() + n.y() * n.y());
					float gf = 0.01f * k * (float) gravity * d;
					layoutData.dx -= gf * n.x() / d;
					layoutData.dy -= gf * n.y() / d;
				}
				// speed
				foreach (Node n in nodes)
				{
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					layoutData.dx *= (float)(speed / SPEED_DIVISOR);
					layoutData.dy *= (float)(speed / SPEED_DIVISOR);
				}
				foreach (Node n in nodes)
				{
					// Maintenant on applique le déplacement calculé sur les noeuds.
					// nb : le déplacement à chaque passe "instantanné" correspond à la force : c'est une sorte d'accélération.
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					float xDist = layoutData.dx;
					float yDist = layoutData.dy;
					float dist = (float) Math.Sqrt(layoutData.dx * layoutData.dx + layoutData.dy * layoutData.dy);
					if (dist > 0 && !n.Fixed)
					{
						float limitedDist = Math.Min(maxDisplace * ((float) speed / SPEED_DIVISOR), dist);
						n.X = n.x() + xDist / dist * limitedDist;
						n.Y = n.y() + yDist / dist * limitedDist;
					}
				}
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override void endAlgo()
		{
			graph.readLock();
			try
			{
				foreach (Node n in graph.Nodes)
				{
					n.LayoutData = null;
				}
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override bool canAlgo()
		{
			return true;
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				const string FRUCHTERMAN_REINGOLD = "Fruchterman Reingold";
    
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.area.name"), FRUCHTERMAN_REINGOLD, "fruchtermanReingold.area.name", NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.area.desc"), "getArea", "setArea"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.gravity.name"), FRUCHTERMAN_REINGOLD, "fruchtermanReingold.gravity.name", NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.gravity.desc"), "getGravity", "setGravity"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.speed.name"), FRUCHTERMAN_REINGOLD, "fruchtermanReingold.speed.name", NbBundle.getMessage(typeof(FruchtermanReingold), "fruchtermanReingold.speed.desc"), "getSpeed", "setSpeed"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
    
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		public virtual float? Area
		{
			get
			{
				return area;
			}
			set
			{
				this.area = value.Value;
			}
		}


		/// <returns> the gravity </returns>
		public virtual double? Gravity
		{
			get
			{
				return gravity;
			}
			set
			{
				this.gravity = value.Value;
			}
		}


		/// <returns> the speed </returns>
		public virtual double? Speed
		{
			get
			{
				return speed;
			}
			set
			{
				this.speed = value.Value;
			}
		}

	}

}