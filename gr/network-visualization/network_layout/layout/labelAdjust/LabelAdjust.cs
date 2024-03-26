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

namespace org.gephi.layout.plugin.labelAdjust
{
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using TextProperties = org.gephi.graph.api.TextProperties;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// @author Mathieu Jacomy
	/// </summary>
	public class LabelAdjust : AbstractLayout, Layout
	{

		//Graph
		protected internal Graph graph;
		//Settings
		private double speed = 1;
		private bool adjustBySize = true;
		private float radiusScale = 1.1f;
		//Graph size
		private float xmin;
		private float xmax;
		private float ymin;
		private float ymax;

		public LabelAdjust(LayoutBuilder layoutBuilder) : base(layoutBuilder)
		{
		}

		public override void resetPropertiesValues()
		{
			speed = 1;
			radiusScale = 1.1f;
			adjustBySize = true;
		}

		public override void initAlgo()
		{
			Converged = false;
		}

		public override void goAlgo()
		{
			this.graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				Node[] nodes = graph.Nodes.toArray();

				//Reset Layout Data
				foreach (Node n in nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is LabelAdjustLayoutData))
					{
						n.LayoutData = new LabelAdjustLayoutData();
					}
					LabelAdjustLayoutData layoutData = n.LayoutData;
					layoutData.freeze = 0;
					layoutData.dx = 0;
					layoutData.dy = 0;
				}

				// Get xmin, xmax, ymin, ymax
				xmin = float.MaxValue;
				xmax = float.Epsilon;
				ymin = float.MaxValue;
				ymax = float.Epsilon;

				IList<Node> correctNodes = new List<Node>();
				foreach (Node n in nodes)
				{
					float x = n.x();
					float y = n.y();
					TextProperties t = n.TextProperties;
					float w = t.Width;
					float h = t.Height;
					float radius = n.size() / 2f;

					if (w > 0 && h > 0)
					{
						// Get the rectangle occupied by the node (size + label)
						float nxmin = Math.Min(x - w / 2, x - radius);
						float nxmax = Math.Max(x + w / 2, x + radius);
						float nymin = Math.Min(y - h / 2, y - radius);
						float nymax = Math.Max(y + h / 2, y + radius);

						// Update global boundaries
						xmin = Math.Min(this.xmin, nxmin);
						xmax = Math.Max(this.xmax, nxmax);
						ymin = Math.Min(this.ymin, nymin);
						ymax = Math.Max(this.ymax, nymax);

						correctNodes.Add(n);
					}
				}

				if (correctNodes.Count == 0 || xmin == xmax || ymin == ymax)
				{
					return;
				}

				long timeStamp = 1;
				bool someCollision = false;

				//Add all nodes in the quadtree
				QuadTree quadTree = new QuadTree(this, correctNodes.Count, (xmax - xmin) / (ymax - ymin));
				foreach (Node n in correctNodes)
				{
					quadTree.add(n);
				}

				//Compute repulsion - with neighbours in the 8 quadnodes around the node
				foreach (Node n in correctNodes)
				{
					timeStamp++;
					LabelAdjustLayoutData layoutData = n.LayoutData;
					QuadNode quad = quadTree.getQuadNode(layoutData.labelAdjustQuadNode);

					//Repulse with adjacent quad - but only one per pair of nodes, timestamp is guaranteeing that
					foreach (Node neighbour in quadTree.getAdjacentNodes(quad.row, quad.col))
					{
						LabelAdjustLayoutData neighborLayoutData = neighbour.LayoutData;
						if (neighbour != n && neighborLayoutData.freeze < timeStamp)
						{
							bool collision = repulse(n, neighbour);
							someCollision = someCollision || collision;
						}
						neighborLayoutData.freeze = timeStamp; //Use the existing freeze float variable to set timestamp
					}
				}

				if (!someCollision)
				{
					Converged = true;
				}
				else
				{
					// apply forces
					foreach (Node n in correctNodes)
					{
						LabelAdjustLayoutData layoutData = n.LayoutData;
						if (!n.Fixed)
						{
							layoutData.dx *= (float)speed;
							layoutData.dy *= (float)speed;
							float x = n.x() + layoutData.dx;
							float y = n.y() + layoutData.dy;

							n.X = x;
							n.Y = y;
						}
					}
				}
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		private bool repulse(Node n1, Node n2)
		{
			bool collision = false;
			float n1x = n1.x();
			float n1y = n1.y();
			float n2x = n2.x();
			float n2y = n2.y();
			TextProperties t1 = n1.TextProperties;
			TextProperties t2 = n2.TextProperties;
			float n1w = t1.Width;
			float n2w = t2.Width;
			float n1h = t1.Height;
			float n2h = t2.Height;
			LabelAdjustLayoutData n2Data = n2.LayoutData;

			double n1xmin = n1x - 0.5 * n1w;
			double n2xmin = n2x - 0.5 * n2w;
			double n1ymin = n1y - 0.5 * n1h;
			double n2ymin = n2y - 0.5 * n2h;
			double n1xmax = n1x + 0.5 * n1w;
			double n2xmax = n2x + 0.5 * n2w;
			double n1ymax = n1y + 0.5 * n1h;
			double n2ymax = n2y + 0.5 * n2h;

			//Sphere repulsion
			if (adjustBySize)
			{
				double xDist = n2x - n1x;
				double yDist = n2y - n1y;
				double dist = Math.Sqrt(xDist * xDist + yDist * yDist);
				bool sphereCollision = dist < radiusScale * (n1.size() + n2.size());
				if (sphereCollision)
				{
					double f = 0.1 * n1.size() / dist;
					if (dist > 0)
					{
						n2Data.dx = (float)(n2Data.dx + xDist / dist * f);
						n2Data.dy = (float)(n2Data.dy + yDist / dist * f);
					}
					else
					{
						n2Data.dx = (float)(n2Data.dx + 0.01 * (0.5 - GlobalRandom.NextDouble));
						n2Data.dy = (float)(n2Data.dy + 0.01 * (0.5 - GlobalRandom.NextDouble));
					}
					collision = true;
				}
			}

			double upDifferential = n1ymax - n2ymin;
			double downDifferential = n2ymax - n1ymin;
			double labelCollisionXleft = n2xmax - n1xmin;
			double labelCollisionXright = n1xmax - n2xmin;

			if (upDifferential > 0 && downDifferential > 0)
			{ // Potential collision
				if (labelCollisionXleft > 0 && labelCollisionXright > 0)
				{ // Collision
					if (upDifferential > downDifferential)
					{
						// N1 pushes N2 up
						n2Data.dy = (float)(n2Data.dy - 0.02 * n1h * (0.8 + 0.4 * GlobalRandom.NextDouble));
						collision = true;
					}
					else
					{
						// N1 pushes N2 down
						n2Data.dy = (float)(n2Data.dy + 0.02 * n1h * (0.8 + 0.4 * GlobalRandom.NextDouble));
						collision = true;
					}
					if (labelCollisionXleft > labelCollisionXright)
					{
						// N1 pushes N2 right
						n2Data.dx = (float)(n2Data.dx + 0.01 * (n1h * 2) * (0.8 + 0.4 * GlobalRandom.NextDouble));
						collision = true;
					}
					else
					{
						// N1 pushes N2 left
						n2Data.dx = (float)(n2Data.dx - 0.01 * (n1h * 2) * (0.8 + 0.4 * GlobalRandom.NextDouble));
						collision = true;
					}
				}
			}

			return collision;
		}

		public override void endAlgo()
		{
			foreach (Node n in graph.Nodes)
			{
				n.LayoutData = null;
			}
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				const string LABELADJUST_CATEGORY = "LabelAdjust";
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "LabelAdjust.speed.name"), LABELADJUST_CATEGORY, "LabelAdjust.speed.name", NbBundle.getMessage(this.GetType(), "LabelAdjust.speed.desc"), "getSpeed", "setSpeed"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "LabelAdjust.adjustBySize.name"), LABELADJUST_CATEGORY, "LabelAdjust.adjustBySize.name", NbBundle.getMessage(this.GetType(), "LabelAdjust.adjustBySize.desc"), "isAdjustBySize", "setAdjustBySize"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

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


		public virtual bool? AdjustBySize
		{
			get
			{
				return adjustBySize;
			}
			set
			{
				this.adjustBySize = value.Value;
			}
		}


		private class QuadNode
		{

			internal readonly int index;
			internal readonly int row;
			internal readonly int col;
			internal readonly IList<Node> nodes;

			public QuadNode(int index, int row, int col)
			{
				this.index = index;
				this.row = row;
				this.col = col;
				this.nodes = new List<Node>();
			}

			public virtual IList<Node> Nodes
			{
				get
				{
					return nodes;
				}
			}

			public virtual void add(Node n)
			{
				nodes.Add(n);
			}
		}

		private class QuadTree
		{
			private readonly LabelAdjust outerInstance;


			internal readonly QuadNode[] quads;
			internal readonly int COLUMNS;
			internal readonly int ROWS;

			public QuadTree(LabelAdjust outerInstance, int numberNodes, float aspectRatio)
			{
				this.outerInstance = outerInstance;
				if (aspectRatio > 0)
				{
					COLUMNS = (int) Math.Ceiling(numberNodes / 50f);
					ROWS = (int) Math.Ceiling(COLUMNS / aspectRatio);
				}
				else
				{
					ROWS = (int) Math.Ceiling(numberNodes / 50f);
					COLUMNS = (int) Math.Ceiling(ROWS / aspectRatio);
				}
				quads = new QuadNode[COLUMNS * ROWS];
				for (int row = 0; row < ROWS; row++)
				{
					for (int col = 0; col < COLUMNS; col++)
					{
						quads[row * COLUMNS + col] = new QuadNode(row * COLUMNS + col, row, col);
					}
				}
			}

			public virtual void add(Node node)
			{
				float x = node.x();
				float y = node.y();
				TextProperties t = node.TextProperties;
				float w = t.Width;
				float h = t.Height;
				float radius = node.size();

				// Get the rectangle occupied by the node (size + label)
				float nxmin = Math.Min(x - w / 2, x - radius);
				float nxmax = Math.Max(x + w / 2, x + radius);
				float nymin = Math.Min(y - h / 2, y - radius);
				float nymax = Math.Max(y + h / 2, y + radius);

				// Get the rectangle as boxes
				int minXbox = (int) Math.Floor((COLUMNS - 1) * (nxmin - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin));
				int maxXbox = (int) Math.Floor((COLUMNS - 1) * (nxmax - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin));
				int minYbox = (int) Math.Floor((ROWS - 1) * (((outerInstance.ymax - outerInstance.ymin) - (nymax - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)));
				int maxYbox = (int) Math.Floor((ROWS - 1) * (((outerInstance.ymax - outerInstance.ymin) - (nymin - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)));
				for (int col = minXbox; col <= maxXbox && col < COLUMNS && col >= 0; col++)
				{
					for (int row = minYbox; row <= maxYbox && row < ROWS && row >= 0; row++)
					{
						quads[row * COLUMNS + col].add(node);
					}
				}

				//Get the node center
				int centerX = (int) Math.Floor((COLUMNS - 1) * (x - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin));
				int centerY = (int) Math.Floor((ROWS - 1) * (((outerInstance.ymax - outerInstance.ymin) - (y - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)));
				LabelAdjustLayoutData layoutData = node.LayoutData;
				layoutData.labelAdjustQuadNode = quads[centerY * COLUMNS + centerX].index;
			}

			public virtual IList<Node> get(int row, int col)
			{
				return quads[row * ROWS + col].Nodes;
			}

			public virtual IList<Node> getAdjacentNodes(int row, int col)
			{
				if (quads.Length == 1)
				{
					return quads[0].Nodes;
				}

				IList<Node> adjNodes = new List<Node>();
				int left = Math.Max(0, col - 1);
				int top = Math.Max(0, row - 1);
				int right = Math.Min(COLUMNS - 1, col + 1);
				int bottom = Math.Min(ROWS - 1, row + 1);
				for (int i = left; i <= right; i++)
				{
					for (int j = top; j <= bottom; j++)
					{
						((List<Node>)adjNodes).AddRange(quads[j * COLUMNS + i].Nodes);
					}
				}
				return adjNodes;
			}

			public virtual QuadNode getQuadNode(int index)
			{
				return quads[index];
			}
		}
	}

}