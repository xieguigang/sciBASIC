using System;
using System.Collections.Generic;

/*
 Copyright 2008-2011 Gephi
 Authors : Mathieu Jacomy <mathieu.jacomy@gmail.com>
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

namespace org.gephi.layout.plugin.forceAtlas2
{
	using Node = org.gephi.graph.api.Node;
	using RepulsionForce = org.gephi.layout.plugin.forceAtlas2.ForceFactory.RepulsionForce;

	/// <summary>
	/// Barnes Hut optimization
	/// 
	/// @author Mathieu Jacomy
	/// </summary>
	public class Region
	{

		private readonly IList<Node> nodes;
		private readonly IList<Region> subregions = new List<Region>();
		private double mass;
		private double massCenterX;
		private double massCenterY;
		private double size;

		public Region(Node[] nodes)
		{
			this.nodes = new List<Node>();
			((List<Node>)this.nodes).AddRange(Arrays.asList(nodes));
			updateMassAndGeometry();
		}

		public Region(List<Node> nodes)
		{
			this.nodes = new List<Node>(nodes);
			updateMassAndGeometry();
		}

		private void updateMassAndGeometry()
		{
			if (nodes.Count > 1)
			{
				// Compute Mass
				mass = 0;
				double massSumX = 0;
				double massSumY = 0;
				foreach (Node n in nodes)
				{
					ForceAtlas2LayoutData nLayout = n.LayoutData;
					mass += nLayout.mass;
					massSumX += n.x() * nLayout.mass;
					massSumY += n.y() * nLayout.mass;
				}
				massCenterX = massSumX / mass;
				massCenterY = massSumY / mass;

				// Compute size
				size = double.Epsilon;
				foreach (Node n in nodes)
				{
					double distance = Math.Sqrt((n.x() - massCenterX) * (n.x() - massCenterX) + (n.y() - massCenterY) * (n.y() - massCenterY));
					size = Math.Max(size, 2 * distance);
				}
			}
		}

		public virtual void buildSubRegions()
		{
			lock (this)
			{
				if (nodes.Count > 1)
				{
					List<Node> leftNodes = new List<Node>();
					List<Node> rightNodes = new List<Node>();
					foreach (Node n in nodes)
					{
						List<Node> nodesColumn = (n.x() < massCenterX) ? (leftNodes) : (rightNodes);
						nodesColumn.Add(n);
					}
        
					List<Node> topleftNodes = new List<Node>();
					List<Node> bottomleftNodes = new List<Node>();
					foreach (Node n in leftNodes)
					{
						List<Node> nodesLine = (n.y() < massCenterY) ? (topleftNodes) : (bottomleftNodes);
						nodesLine.Add(n);
					}
        
					List<Node> bottomrightNodes = new List<Node>();
					List<Node> toprightNodes = new List<Node>();
					foreach (Node n in rightNodes)
					{
						List<Node> nodesLine = (n.y() < massCenterY) ? (toprightNodes) : (bottomrightNodes);
						nodesLine.Add(n);
					}
        
					if (topleftNodes.Count > 0)
					{
						if (topleftNodes.Count < nodes.Count)
						{
							Region subregion = new Region(topleftNodes);
							subregions.Add(subregion);
						}
						else
						{
							foreach (Node n in topleftNodes)
							{
								List<Node> oneNodeList = new List<Node>();
								oneNodeList.Add(n);
								Region subregion = new Region(oneNodeList);
								subregions.Add(subregion);
							}
						}
					}
					if (bottomleftNodes.Count > 0)
					{
						if (bottomleftNodes.Count < nodes.Count)
						{
							Region subregion = new Region(bottomleftNodes);
							subregions.Add(subregion);
						}
						else
						{
							foreach (Node n in bottomleftNodes)
							{
								List<Node> oneNodeList = new List<Node>();
								oneNodeList.Add(n);
								Region subregion = new Region(oneNodeList);
								subregions.Add(subregion);
							}
						}
					}
					if (bottomrightNodes.Count > 0)
					{
						if (bottomrightNodes.Count < nodes.Count)
						{
							Region subregion = new Region(bottomrightNodes);
							subregions.Add(subregion);
						}
						else
						{
							foreach (Node n in bottomrightNodes)
							{
								List<Node> oneNodeList = new List<Node>();
								oneNodeList.Add(n);
								Region subregion = new Region(oneNodeList);
								subregions.Add(subregion);
							}
						}
					}
					if (toprightNodes.Count > 0)
					{
						if (toprightNodes.Count < nodes.Count)
						{
							Region subregion = new Region(toprightNodes);
							subregions.Add(subregion);
						}
						else
						{
							foreach (Node n in toprightNodes)
							{
								List<Node> oneNodeList = new List<Node>();
								oneNodeList.Add(n);
								Region subregion = new Region(oneNodeList);
								subregions.Add(subregion);
							}
						}
					}
        
					foreach (Region subregion in subregions)
					{
						subregion.buildSubRegions();
					}
				}
			}
		}

		public virtual void applyForce(Node n, RepulsionForce Force, double theta)
		{
			if (nodes.Count < 2)
			{
				Node regionNode = nodes[0];
				Force.apply(n, regionNode);
			}
			else
			{
				double distance = Math.Sqrt((n.x() - massCenterX) * (n.x() - massCenterX) + (n.y() - massCenterY) * (n.y() - massCenterY));
				if (distance * theta > size)
				{
					Force.apply(n, this);
				}
				else
				{
					foreach (Region subregion in subregions)
					{
						subregion.applyForce(n, Force, theta);
					}
				}
			}
		}

		public virtual double Mass
		{
			get
			{
				return mass;
			}
			set
			{
				this.mass = value;
			}
		}


		public virtual double MassCenterX
		{
			get
			{
				return massCenterX;
			}
			set
			{
				this.massCenterX = value;
			}
		}


		public virtual double MassCenterY
		{
			get
			{
				return massCenterY;
			}
			set
			{
				this.massCenterY = value;
			}
		}

	}

}