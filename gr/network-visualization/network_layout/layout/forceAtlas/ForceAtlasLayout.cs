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

namespace org.gephi.layout.plugin.forceAtlas
{
	using Edge = org.gephi.graph.api.Edge;
	using Graph = org.gephi.graph.api.Graph;
	using Interval = org.gephi.graph.api.Interval;
	using Node = org.gephi.graph.api.Node;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// @author Mathieu Jacomy
	/// </summary>
	public class ForceAtlasLayout : AbstractLayout, Layout
	{

		//Properties
		public double inertia;
		//Graph
		protected internal Graph graph;
		private double repulsionStrength;
		private double attractionStrength;
		private double maxDisplacement;
		private bool freezeBalance;
		private double freezeStrength;
		private double freezeInertia;
		private double gravity;
		private double speed;
		private double cooling;
		private bool outboundAttractionDistribution;
		private bool adjustSizes;

		public ForceAtlasLayout(LayoutBuilder layoutBuilder) : base(layoutBuilder)
		{
		}

		public override void resetPropertiesValues()
		{
			inertia = 0.1;
			RepulsionStrength = 200d;
			AttractionStrength = 10d;
			MaxDisplacement = 10d;
			FreezeBalance = true;
			FreezeStrength = 80d;
			FreezeInertia = 0.2;
			Gravity = 30d;
			OutboundAttractionDistribution = false;
			AdjustSizes = false;
			Speed = 1d;
			Cooling = 1d;
		}

		public override void initAlgo()
		{
			ensureSafeLayoutNodePositions(graphModel);
		}

		private double getEdgeWeight(Edge edge, bool isDynamicWeight, Interval interval)
		{
			if (isDynamicWeight)
			{
				return edge.getWeight(interval);
			}
			else
			{
				return edge.Weight;
			}
		}

		public override void goAlgo()
		{
			this.graph = graphModel.GraphVisible;
			graph.readLock();
			bool isDynamicWeight = graphModel.EdgeTable.getColumn("weight").Dynamic;
			Interval interval = graph.View.TimeInterval;

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
				}

				foreach (Node n in nodes)
				{
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					layoutData.old_dx = layoutData.dx;
					layoutData.old_dy = layoutData.dy;
					layoutData.dx *= (float)inertia;
					layoutData.dy *= (float)inertia;
				}
				// repulsion
				if (AdjustSizes.Value)
				{
					foreach (Node n1 in nodes)
					{
						foreach (Node n2 in nodes)
						{
							if (n1 != n2)
							{
								ForceVectorUtils.fcBiRepulsor_noCollide(n1, n2, RepulsionStrength.Value * (1 + graph.getDegree(n1)) * (1 + graph.getDegree(n2)));
							}
						}
					}
				}
				else
				{
					foreach (Node n1 in nodes)
					{
						foreach (Node n2 in nodes)
						{
							if (n1 != n2)
							{
								ForceVectorUtils.fcBiRepulsor(n1, n2, RepulsionStrength.Value * (1 + graph.getDegree(n1)) * (1 + graph.getDegree(n2)));
							}
						}
					}
				}
				// attraction
				if (AdjustSizes.Value)
				{
					if (OutboundAttractionDistribution.Value)
					{
						foreach (Edge e in edges)
						{
							Node nf = e.Source;
							Node nt = e.Target;
							double bonus = (nf.Fixed || nt.Fixed) ? (100) : (1);
							bonus *= getEdgeWeight(e, isDynamicWeight, interval);
							ForceVectorUtils.fcBiAttractor_noCollide(nf, nt, bonus * AttractionStrength.Value / (1 + graph.getDegree(nf)));
						}
					}
					else
					{
						foreach (Edge e in edges)
						{
							Node nf = e.Source;
							Node nt = e.Target;
							double bonus = (nf.Fixed || nt.Fixed) ? (100) : (1);
							bonus *= getEdgeWeight(e, isDynamicWeight, interval);
							ForceVectorUtils.fcBiAttractor_noCollide(nf, nt, bonus * AttractionStrength.Value);
						}
					}
				}
				else
				{
					if (OutboundAttractionDistribution.Value)
					{
						foreach (Edge e in edges)
						{
							Node nf = e.Source;
							Node nt = e.Target;
							double bonus = (nf.Fixed || nt.Fixed) ? (100) : (1);
							bonus *= getEdgeWeight(e, isDynamicWeight, interval);
							ForceVectorUtils.fcBiAttractor(nf, nt, bonus * AttractionStrength.Value / (1 + graph.getDegree(nf)));
						}
					}
					else
					{
						foreach (Edge e in edges)
						{
							Node nf = e.Source;
							Node nt = e.Target;
							double bonus = (nf.Fixed || nt.Fixed) ? (100) : (1);
							bonus *= getEdgeWeight(e, isDynamicWeight, interval);
							ForceVectorUtils.fcBiAttractor(nf, nt, bonus * AttractionStrength.Value);
						}
					}
				}
				// gravity
				foreach (Node n in nodes)
				{

					float nx = n.x();
					float ny = n.y();
					double d = 0.0001 + Math.Sqrt(nx * nx + ny * ny);
					double gf = 0.0001 * Gravity.Value * d;
					ForceVectorNodeLayoutData layoutData = n.LayoutData;
					layoutData.dx -= (float)(gf * nx / d);
					layoutData.dy -= (float)(gf * ny / d);
				}
				// speed
				if (FreezeBalance.Value)
				{
					foreach (Node n in nodes)
					{
						ForceVectorNodeLayoutData layoutData = n.LayoutData;
						layoutData.dx *= Speed.Value * 10f;
						layoutData.dy *= Speed.Value * 10f;
					}
				}
				else
				{
					foreach (Node n in nodes)
					{
						ForceVectorNodeLayoutData layoutData = n.LayoutData;
						layoutData.dx *= Speed;
						layoutData.dy *= Speed;
					}
				}
				// apply forces
				foreach (Node n in nodes)
				{
					ForceVectorNodeLayoutData nLayout = n.LayoutData;
					if (!n.Fixed)
					{
						double d = 0.0001 + Math.Sqrt(nLayout.dx * nLayout.dx + nLayout.dy * nLayout.dy);
						float ratio;
						if (FreezeBalance.Value)
						{
							nLayout.freeze = (float)(FreezeInertia.Value * nLayout.freeze + (1 - FreezeInertia.Value) * 0.1 * FreezeStrength.Value * (Math.Sqrt(Math.Sqrt((nLayout.old_dx - nLayout.dx) * (nLayout.old_dx - nLayout.dx) + (nLayout.old_dy - nLayout.dy) * (nLayout.old_dy - nLayout.dy)))));
							ratio = (float) Math.Min((d / (d * (1f + nLayout.freeze))), MaxDisplacement.Value / d);
						}
						else
						{
							ratio = (float) Math.Min(1, MaxDisplacement.Value / d);
						}
						nLayout.dx *= ratio / Cooling.Value;
						nLayout.dy *= ratio / Cooling.Value;
						float x = n.x() + nLayout.dx;
						float y = n.y() + nLayout.dy;

						n.X = x;
						n.Y = y;
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
				const string FORCE_ATLAS = "Force Atlas";
    
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.inertia.name"), FORCE_ATLAS, "forceAtlas.inertia.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.inertia.desc"), "getInertia", "setInertia"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.repulsionStrength.name"), FORCE_ATLAS, "forceAtlas.repulsionStrength.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.repulsionStrength.desc"), "getRepulsionStrength", "setRepulsionStrength"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.attractionStrength.name"), FORCE_ATLAS, "forceAtlas.attractionStrength.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.attractionStrength.desc"), "getAttractionStrength", "setAttractionStrength"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.maxDisplacement.name"), FORCE_ATLAS, "forceAtlas.maxDisplacement.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.maxDisplacement.desc"), "getMaxDisplacement", "setMaxDisplacement"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeBalance.name"), FORCE_ATLAS, "forceAtlas.freezeBalance.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeBalance.desc"), "isFreezeBalance", "setFreezeBalance"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeStrength.name"), FORCE_ATLAS, "forceAtlas.freezeStrength.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeStrength.desc"), "getFreezeStrength", "setFreezeStrength"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeInertia.name"), FORCE_ATLAS, "forceAtlas.freezeInertia.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.freezeInertia.desc"), "getFreezeInertia", "setFreezeInertia"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.gravity.name"), FORCE_ATLAS, "forceAtlas.gravity.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.gravity.desc"), "getGravity", "setGravity"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.outboundAttractionDistribution.name"), FORCE_ATLAS, "forceAtlas.outboundAttractionDistribution.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.outboundAttractionDistribution.desc"), "isOutboundAttractionDistribution", "setOutboundAttractionDistribution"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.adjustSizes.name"), FORCE_ATLAS, "forceAtlas.adjustSizes.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.adjustSizes.desc"), "isAdjustSizes", "setAdjustSizes"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.speed.name"), FORCE_ATLAS, "forceAtlas.speed.name", NbBundle.getMessage(typeof(ForceAtlasLayout), "forceAtlas.speed.desc"), "getSpeed", "setSpeed"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
    
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		public virtual double? Inertia
		{
			get
			{
				return inertia;
			}
			set
			{
				this.inertia = value.Value;
			}
		}


		/// <returns> the repulsionStrength </returns>
		public virtual double? RepulsionStrength
		{
			get
			{
				return repulsionStrength;
			}
			set
			{
				this.repulsionStrength = value.Value;
			}
		}


		/// <returns> the attractionStrength </returns>
		public virtual double? AttractionStrength
		{
			get
			{
				return attractionStrength;
			}
			set
			{
				this.attractionStrength = value.Value;
			}
		}


		/// <returns> the maxDisplacement </returns>
		public virtual double? MaxDisplacement
		{
			get
			{
				return maxDisplacement;
			}
			set
			{
				this.maxDisplacement = value.Value;
			}
		}


		/// <returns> the freezeBalance </returns>
		public virtual bool? FreezeBalance
		{
			get
			{
				return freezeBalance;
			}
			set
			{
				this.freezeBalance = value.Value;
			}
		}


		/// <returns> the freezeStrength </returns>
		public virtual double? FreezeStrength
		{
			get
			{
				return freezeStrength;
			}
			set
			{
				this.freezeStrength = value.Value;
			}
		}


		/// <returns> the freezeInertia </returns>
		public virtual double? FreezeInertia
		{
			get
			{
				return freezeInertia;
			}
			set
			{
				this.freezeInertia = value.Value;
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


		/// <returns> the cooling </returns>
		public virtual double? Cooling
		{
			get
			{
				return cooling;
			}
			set
			{
				this.cooling = value.Value;
			}
		}


		/// <returns> the outboundAttractionDistribution </returns>
		public virtual bool? OutboundAttractionDistribution
		{
			get
			{
				return outboundAttractionDistribution;
			}
			set
			{
				this.outboundAttractionDistribution = value.Value;
			}
		}


		/// <returns> the adjustSizes </returns>
		public virtual bool? AdjustSizes
		{
			get
			{
				return adjustSizes;
			}
			set
			{
				this.adjustSizes = value.Value;
			}
		}

	}

}