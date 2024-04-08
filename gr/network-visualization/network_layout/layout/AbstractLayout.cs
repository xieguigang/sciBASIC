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

namespace org.gephi.layout.plugin
{
	using Graph = org.gephi.graph.api.Graph;
	using GraphModel = org.gephi.graph.api.GraphModel;
	using Node = org.gephi.graph.api.Node;
	using NodeIterable = org.gephi.graph.api.NodeIterable;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;

	/// <summary>
	/// Base class for layout algorithms.
	/// 
	/// @author Helder Suzuki
	/// </summary>
	public abstract class AbstractLayout : Layout
	{

		private readonly LayoutBuilder layoutBuilder;
		protected internal GraphModel graphModel;
		private bool converged;

		public AbstractLayout(LayoutBuilder layoutBuilder)
		{
			this.layoutBuilder = layoutBuilder;
		}

		/// <summary>
		/// See https://github.com/gephi/gephi/issues/603 Nodes position to NaN on applied layout
		/// </summary>
		/// <param name="graphModel"> </param>
		public static void ensureSafeLayoutNodePositions(GraphModel graphModel)
		{
			Graph graph = graphModel.Graph;
			NodeIterable nodesIterable = graph.Nodes;
			foreach (Node node in nodesIterable)
			{
				if (node.x() != 0 || node.y() != 0)
				{
					nodesIterable.doBreak();
					return;
				}
			}

			//All at 0.0, init some random positions
			nodesIterable = graph.Nodes;
			foreach (Node node in nodesIterable)
			{
				node.X = (float)((0.01 + GlobalRandom.NextDouble) * 1000) - 500;
				node.Y = (float)((0.01 + GlobalRandom.NextDouble) * 1000) - 500;
			}
		}

		public override LayoutBuilder Builder
		{
			get
			{
				return layoutBuilder;
			}
		}

		public override GraphModel GraphModel
		{
			set
			{
				this.graphModel = value;
			}
		}

		public override bool canAlgo()
		{
			return !Converged && graphModel != null;
		}

		public virtual bool Converged
		{
			get
			{
				return converged;
			}
			set
			{
				this.converged = value;
			}
		}

	}

}