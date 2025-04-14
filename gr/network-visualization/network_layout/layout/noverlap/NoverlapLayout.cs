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

namespace org.gephi.layout.plugin.noverlap
{
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using NodeIterable = org.gephi.graph.api.NodeIterable;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using LongTask = org.gephi.utils.longtask.spi.LongTask;
	using ProgressTicket = org.gephi.utils.progress.ProgressTicket;
	using Exceptions = org.openide.util.Exceptions;

	/// <summary>
	/// @author Mathieu Jacomy
	/// </summary>
	public class NoverlapLayout : AbstractLayout, Layout, LongTask
	{

		protected internal bool cancel_Conflict;
		protected internal Graph graph;
		private double speed;
		private double ratio;
		private double margin;
		private double xmin;
		private double xmax;
		private double ymin;
		private double ymax;

		public NoverlapLayout(LayoutBuilder layoutBuilder) : base(layoutBuilder)
		{
		}

		public override void initAlgo()
		{
			this.graph = graphModel.GraphVisible;
			Converged = false;
			cancel_Conflict = false;
		}

		public override void goAlgo()
		{
			Converged = true;
			this.graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				//Reset Layout Data
				foreach (Node n in graph.Nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is NoverlapLayoutData))
					{
						n.LayoutData = new NoverlapLayoutData();
					}
					NoverlapLayoutData layoutData = n.LayoutData;
					layoutData.neighbours.Clear();
					layoutData.dx = 0;
					layoutData.dy = 0;
				}

				// Get xmin, xmax, ymin, ymax
				this.xmin = double.MaxValue;
				this.xmax = double.Epsilon;
				this.ymin = double.MaxValue;
				this.ymax = double.Epsilon;

				foreach (Node n in graph.Nodes)
				{
					float x = n.x();
					float y = n.y();
					float radius = n.size();

					// Get the rectangle occupied by the node
					double nxmin = x - (radius * ratio + margin);
					double nxmax = x + (radius * ratio + margin);
					double nymin = y - (radius * ratio + margin);
					double nymax = y + (radius * ratio + margin);

					// Update global boundaries
					this.xmin = Math.Min(this.xmin, nxmin);
					this.xmax = Math.Max(this.xmax, nxmax);
					this.ymin = Math.Min(this.ymin, nymin);
					this.ymax = Math.Max(this.ymax, nymax);
				}

				// Secure the bounds
				double xwidth = this.xmax - this.xmin;
				double yheight = this.ymax - this.ymin;
				double xcenter = (this.xmin + this.xmax) / 2;
				double ycenter = (this.ymin + this.ymax) / 2;
				double securityRatio = 1.1;
				this.xmin = xcenter - securityRatio * xwidth / 2;
				this.xmax = xcenter + securityRatio * xwidth / 2;
				this.ymin = ycenter - securityRatio * yheight / 2;
				this.ymax = ycenter + securityRatio * yheight / 2;

				SpatialGrid grid = new SpatialGrid(this);

				// Put nodes in their boxes
				foreach (Node n in graph.Nodes)
				{
					grid.add(n);
				}

				// Now we have cells with nodes in it. Nodes that are in the same cell, or in adjacent cells, are tested for repulsion.
				// But they are not repulsed several times, even if they are in several cells...
				// So we build a relation of proximity between nodes.
				// Build proximities
				for (int row = 0; row < grid.countRows() && !cancel_Conflict; row++)
				{
					for (int col = 0; col < grid.countColumns() && !cancel_Conflict; col++)
					{
						foreach (Node n in grid.getContent(row, col))
						{
							NoverlapLayoutData lald = n.LayoutData;

							// For node n in the box "box"...
							// We search nodes that are in the boxes that are adjacent or the same.
							for (int row2 = Math.Max(0, row - 1); row2 <= Math.Min(row + 1, grid.countRows() - 1); row2++)
							{
								for (int col2 = Math.Max(0, col - 1); col2 <= Math.Min(col + 1, grid.countColumns() - 1); col2++)
								{
									foreach (Node n2 in grid.getContent(row2, col2))
									{
										if (n2 != n && !lald.neighbours.Contains(n2))
										{
											lald.neighbours.Add(n2);
										}
									}
								}
							}
						}
					}
				}

				// Proximities are built !
				// Apply repulsion force - along proximities...
				NodeIterable nodesIterable = graph.Nodes;
				foreach (Node n1 in nodesIterable)
				{
					NoverlapLayoutData lald = n1.LayoutData;
					foreach (Node n2 in lald.neighbours)
					{
						float n1x = n1.x();
						float n1y = n1.y();
						float n2x = n2.x();
						float n2y = n2.y();
						float n1radius = n1.size();
						float n2radius = n2.size();

						// Check sizes (spheric)
						double xDist = n2x - n1x;
						double yDist = n2y - n1y;
						double dist = Math.Sqrt(xDist * xDist + yDist * yDist);
						bool collision = dist < (n1radius * ratio + margin) + (n2radius * ratio + margin);
						if (collision)
						{
							Converged = false;
							// n1 repulses n2, as strongly as it is big
							NoverlapLayoutData layoutData = n2.LayoutData;
							double f = 1.0 + n1.size();
							if (dist > 0)
							{
								layoutData.dx += (float)(xDist / dist * f);
								layoutData.dy += (float)(yDist / dist * f);
							}
							else
							{
								// Same exact position, divide by zero impossible: jitter
								layoutData.dx += (float)(0.01 * (0.5 - GlobalRandom.NextDouble));
								layoutData.dy += (float)(0.01 * (0.5 - GlobalRandom.NextDouble));
							}
						}
						if (cancel_Conflict)
						{
							break;
						}
					}
					if (cancel_Conflict)
					{
						nodesIterable.doBreak();
						break;
					}
				}

				// apply forces
				foreach (Node n in graph.Nodes)
				{
					NoverlapLayoutData layoutData = n.LayoutData;
					if (!n.Fixed)
					{
						layoutData.dx *= (float)(0.1 * speed);
						layoutData.dy *= (float)(0.1 * speed);
						float x = n.x() + layoutData.dx;
						float y = n.y() + layoutData.dy;

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

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				const string NOVERLAP_CATEGORY = "Noverlap";
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), "speed", NOVERLAP_CATEGORY, "speed", "getSpeed", "setSpeed"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), "ratio", NOVERLAP_CATEGORY, "ratio", "getRatio", "setRatio"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), "margin", NOVERLAP_CATEGORY, "margin", "getMargin", "setMargin"));
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
			Speed = 3.0;
			Ratio = 1.2;
			Margin = 5.0;
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


		public virtual double? Ratio
		{
			get
			{
				return ratio;
			}
			set
			{
				this.ratio = value.Value;
			}
		}


		public virtual double? Margin
		{
			get
			{
				return margin;
			}
			set
			{
				this.margin = value.Value;
			}
		}


		public override bool cancel()
		{
			cancel_Conflict = true;
			return cancel_Conflict;
		}

		public override ProgressTicket ProgressTicket
		{
			set
			{
			}
		}

		private class Cell
		{

			internal readonly int row;
			internal readonly int col;

			public Cell(int row, int col)
			{
				this.row = row;
				this.col = col;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}

				Cell other = (Cell) obj;
				if (this.row != other.row)
				{
					return false;
				}
				return this.col == other.col;
			}

			public override int GetHashCode()
			{
				int hash = 7;
				hash = 11 * hash + this.row;
				hash = 11 * hash + this.col;
				return hash;
			}
		}

		private class SpatialGrid
		{
			private readonly NoverlapLayout outerInstance;


			//Param
			internal readonly int COLUMNS_ROWS = 20;
			//Data
			internal readonly IDictionary<Cell, IList<Node>> data = new Dictionary<Cell, IList<Node>>();

			public SpatialGrid(NoverlapLayout outerInstance)
			{
				this.outerInstance = outerInstance;
				for (int row = 0; row < COLUMNS_ROWS; row++)
				{
					for (int col = 0; col < COLUMNS_ROWS; col++)
					{
						IList<Node> localnodes = new List<Node>();
						data[new Cell(row, col)] = localnodes;
					}
				}
			}

			public virtual IEnumerable<Node> getContent(int row, int col)
			{
				return data[new Cell(row, col)];
			}

			public virtual int countColumns()
			{
				return COLUMNS_ROWS;
			}

			public virtual int countRows()
			{
				return COLUMNS_ROWS;
			}

			public virtual void add(Node node)
			{
				float x = node.x();
				float y = node.y();
				float radius = node.size();

				// Get the rectangle occupied by the node
				double nxmin = x - (radius * outerInstance.ratio + outerInstance.margin);
				double nxmax = x + (radius * outerInstance.ratio + outerInstance.margin);
				double nymin = y - (radius * outerInstance.ratio + outerInstance.margin);
				double nymax = y + (radius * outerInstance.ratio + outerInstance.margin);

				// Get the rectangle as boxes
				int minXbox = (int) Math.Floor((COLUMNS_ROWS - 1) * (nxmin - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin));
				int maxXbox = (int) Math.Floor((COLUMNS_ROWS - 1) * (nxmax - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin));
				int minYbox = (int) Math.Floor((COLUMNS_ROWS - 1) * (nymin - outerInstance.ymin) / (outerInstance.ymax - outerInstance.ymin));
				int maxYbox = (int) Math.Floor((COLUMNS_ROWS - 1) * (nymax - outerInstance.ymin) / (outerInstance.ymax - outerInstance.ymin));
				for (int col = minXbox; col <= maxXbox; col++)
				{
					for (int row = minYbox; row <= maxYbox; row++)
					{
						try
						{
							data[new Cell(row, col)].Add(node);
						}
						catch (Exception)
						{
							//Exceptions.printStackTrace(e);
							if (nxmin < outerInstance.xmin || nxmax > outerInstance.xmax)
							{
							}
							if (nymin < outerInstance.ymin || nymax > outerInstance.ymax)
							{
							}
						}
					}
				}
			}
		}
	}

}