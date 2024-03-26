using System;
using System.Collections;
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

namespace org.gephi.layout.plugin.force.quadtree
{
	using Column = org.gephi.graph.api.Column;
	using ColumnIterable = org.gephi.graph.api.ColumnIterable;
	using Graph = org.gephi.graph.api.Graph;
	using GraphView = org.gephi.graph.api.GraphView;
	using Interval = org.gephi.graph.api.Interval;
	using Node = org.gephi.graph.api.Node;
	using NodeProperties = org.gephi.graph.api.NodeProperties;
	using Table = org.gephi.graph.api.Table;
	using TextProperties = org.gephi.graph.api.TextProperties;
	using LayoutData = org.gephi.graph.spi.LayoutData;

	internal interface AddBehaviour
	{

		bool addNode(NodeProperties node);
	}

	/// <summary>
	/// @author Helder Suzuki
	/// </summary>
	public class QuadTree : Node
	{

		public static readonly float eps = (float) 1e-6;
		private readonly float posX;
		private readonly float posY;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly float size_Conflict;
		private readonly int maxLevel;
		private float centerMassX; // X and Y position of the center of mass
		private float centerMassY;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int mass_Conflict; // Mass of this tree (the number of nodes it contains)
		private AddBehaviour add;
		private IList<QuadTree> children;
		private bool isLeaf;

		public QuadTree(float posX, float posY, float size, int maxLevel)
		{
			this.posX = posX;
			this.posY = posY;
			this.size_Conflict = size;
			this.maxLevel = maxLevel;
			this.isLeaf = true;
			mass_Conflict = 0;
			add = new FirstAdd(this);
		}

		public static QuadTree buildTree(Graph graph, int maxLevel)
		{
			float minX = float.PositiveInfinity;
			float maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity;
			float maxY = float.NegativeInfinity;

			foreach (Node node in graph.Nodes)
			{
				minX = Math.Min(minX, node.x());
				maxX = Math.Max(maxX, node.x());
				minY = Math.Min(minY, node.y());
				maxY = Math.Max(maxY, node.y());
			}

			float size = Math.Max(maxY - minY, maxX - minX);
			QuadTree tree = new QuadTree(minX, minY, size, maxLevel);
			foreach (Node node in graph.Nodes)
			{
				tree.addNode(node);
			}

			return tree;
		}

		public override float size()
		{
			return size_Conflict;
		}

		private void divideTree()
		{
			float childSize = size_Conflict / 2;

			children = new List<QuadTree>();
			children.Add(new QuadTree(posX + childSize, posY + childSize, childSize, maxLevel - 1));
			children.Add(new QuadTree(posX, posY + childSize, childSize, maxLevel - 1));
			children.Add(new QuadTree(posX, posY, childSize, maxLevel - 1));
			children.Add(new QuadTree(posX + childSize, posY, childSize, maxLevel - 1));

			isLeaf = false;
		}

		private bool addToChildren(NodeProperties node)
		{
			foreach (QuadTree q in children)
			{
				if (q.addNode(node))
				{
					return true;
				}
			}
			return false;
		}

		private void assimilateNode(NodeProperties node)
		{
			centerMassX = (mass_Conflict * centerMassX + node.x()) / (mass_Conflict + 1);
			centerMassY = (mass_Conflict * centerMassY + node.y()) / (mass_Conflict + 1);
			mass_Conflict++;
		}

		public virtual IEnumerable<QuadTree> Children
		{
			get
			{
				return children;
			}
		}

		public override float x()
		{
			return centerMassX;
		}

		public override float y()
		{
			return centerMassY;
		}

		public virtual int mass()
		{
			return mass_Conflict;
		}

		public override float z()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public virtual bool addNode(NodeProperties node)
		{
			if (posX <= node.x() && node.x() <= posX + size_Conflict && posY <= node.y() && node.y() <= posY + size_Conflict)
			{
				return add.addNode(node);
			}
			else
			{
				return false;
			}
		}

		/// <returns> the isLeaf </returns>
		public virtual bool IsLeaf
		{
			get
			{
				return isLeaf;
			}
		}

		public override float r()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override float g()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override float b()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override int RGBA
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override Color Color
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}


		public override float alpha()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool Fixed
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}


		public override T getLayoutData<T>() where T : org.gephi.graph.spi.LayoutData
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}


		public override TextProperties TextProperties
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override int StoreId
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float X
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float Y
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float Z
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override void setPosition(float x, float y)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setPosition(float x, float y, float z)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override float R
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float G
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float B
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float Alpha
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override float Size
		{
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override object Id
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override string Label
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
			set
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}


		public override object getAttribute(string key)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(Column column)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object[] Attributes
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override ISet<string> AttributeKeys
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override ColumnIterable AttributeColumns
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override object removeAttribute(string key)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object removeAttribute(Column column)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(string key, object value)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(Column column, object value)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(string key, object value, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(Column column, object value, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool addTimestamp(double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool removeTimestamp(double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override double[] Timestamps
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override Interval TimeBounds
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override void clearAttributes()
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(string key, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(Column column, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(string key, GraphView view)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(Column column, GraphView view)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool hasTimestamp(double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(string key, Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object getAttribute(Column column, Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override IEnumerable<DictionaryEntry> getAttributes(Column column)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object removeAttribute(string key, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object removeAttribute(Column column, double timestamp)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override object removeAttribute(string key, Interval interval)
		{
			throw new System.NotSupportedException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
		}

		public override object removeAttribute(Column column, Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(string key, object value, Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override void setAttribute(Column column, object value, Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool addInterval(Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool removeInterval(Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override bool hasInterval(Interval interval)
		{
			throw new System.NotSupportedException("Not supported.");
		}

		public override Interval[] Intervals
		{
			get
			{
				throw new System.NotSupportedException("Not supported.");
			}
		}

		public override Table Table
		{
			get
			{
				throw new System.NotSupportedException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}
		}

		internal class FirstAdd : AddBehaviour
		{
			private readonly QuadTree outerInstance;

			public FirstAdd(QuadTree outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool addNode(NodeProperties node)
			{
				outerInstance.mass = 1;
				outerInstance.centerMassX = node.x();
				outerInstance.centerMassY = node.y();

				if (outerInstance.maxLevel == 0)
				{
					outerInstance.add = new LeafAdd(outerInstance);
				}
				else
				{
					outerInstance.add = new SecondAdd(outerInstance);
				}

				return true;
			}
		}

		internal class SecondAdd : AddBehaviour
		{
			private readonly QuadTree outerInstance;

			public SecondAdd(QuadTree outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool addNode(NodeProperties node)
			{
				outerInstance.divideTree();
				outerInstance.add = new RootAdd(outerInstance);
				/* This QuadTree represents one node, add it to a child accordingly
				 */
				outerInstance.addToChildren(outerInstance);
				return outerInstance.add.addNode(node);
			}
		}

		internal class LeafAdd : AddBehaviour
		{
			private readonly QuadTree outerInstance;

			public LeafAdd(QuadTree outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool addNode(NodeProperties node)
			{
				outerInstance.assimilateNode(node);
				return true;
			}
		}

		internal class RootAdd : AddBehaviour
		{
			private readonly QuadTree outerInstance;

			public RootAdd(QuadTree outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool addNode(NodeProperties node)
			{
				outerInstance.assimilateNode(node);
				return outerInstance.addToChildren(node);
			}
		}
	}

}