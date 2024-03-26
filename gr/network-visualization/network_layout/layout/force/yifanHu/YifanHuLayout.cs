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

namespace org.gephi.layout.plugin.force.yifanHu
{
	using Edge = org.gephi.graph.api.Edge;
	using Graph = org.gephi.graph.api.Graph;
	using Node = org.gephi.graph.api.Node;
	using BarnesHut = org.gephi.layout.plugin.force.quadtree.BarnesHut;
	using QuadTree = org.gephi.layout.plugin.force.quadtree.QuadTree;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// Hu's basic algorithm
	/// 
	/// @author Helder Suzuki
	/// </summary>
	public class YifanHuLayout : AbstractLayout, Layout
	{

		private float optimalDistance;
		private float relativeStrength;
		private float step;
		private float initialStep;
		private int progress;
		private float stepRatio;
		private int quadTreeMaxLevel;
		private float barnesHutTheta;
		private float convergenceThreshold;
		private bool adaptiveCooling;
		private readonly Displacement displacement;
		private double energy0;
		private double energy;
		private Graph graph;

		public YifanHuLayout(LayoutBuilder layoutBuilder, Displacement displacement) : base(layoutBuilder)
		{
			this.displacement = displacement;
		}

		protected internal virtual void postAlgo()
		{
			updateStep();
			if (Math.Abs((energy - energy0) / energy) < ConvergenceThreshold)
			{
				Converged = true;
			}
		}

		private Displacement Displacement
		{
			get
			{
				displacement.Step = step;
				return displacement;
			}
		}

		private AbstractForce EdgeForce
		{
			get
			{
				return new SpringForce(this, OptimalDistance.Value);
			}
		}

		private AbstractForce NodeForce
		{
			get
			{
				return new ElectricalForce(this, RelativeStrength.Value, OptimalDistance.Value);
			}
		}

		private void updateStep()
		{
			if (AdaptiveCooling.Value)
			{
				if (energy < energy0)
				{
					progress++;
					if (progress >= 5)
					{
						progress = 0;
						Step = step / StepRatio.Value;
					}
				}
				else
				{
					progress = 0;
					Step = step * StepRatio.Value;
				}
			}
			else
			{
				Step = step * StepRatio.Value;
			}
		}

		public override void resetPropertiesValues()
		{
			StepRatio = (float) 0.95;
			RelativeStrength = (float) 0.2;
			if (graph != null)
			{
				OptimalDistance = (float)(Math.Pow(RelativeStrength, 1.0 / 3) * getAverageEdgeLength(graph));
			}
			else
			{
				OptimalDistance = 100.0f;
			}

			InitialStep = optimalDistance / 5;
			Step = initialStep;
			QuadTreeMaxLevel = 10;
			BarnesHutTheta = 1.2f;
			AdaptiveCooling = true;
			ConvergenceThreshold = 1e-4f;
		}

		public virtual float getAverageEdgeLength(Graph graph)
		{
			float edgeLength = 0;
			int count = 1;
			foreach (Edge e in graph.Edges)
			{
				edgeLength += ForceVectorUtils.distance(e.Source, e.Target);
				count++;
			}

			return edgeLength / count;
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				const string YIFANHU_CATEGORY = "Yifan Hu's properties";
				const string BARNESHUT_CATEGORY = "Barnes-Hut's properties";
    
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.optimalDistance.name"), YIFANHU_CATEGORY, "YifanHu.optimalDistance.name", NbBundle.getMessage(this.GetType(), "YifanHu.optimalDistance.desc"), "getOptimalDistance", "setOptimalDistance"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.relativeStrength.name"), YIFANHU_CATEGORY, "YifanHu.relativeStrength.name", NbBundle.getMessage(this.GetType(), "YifanHu.relativeStrength.desc"), "getRelativeStrength", "setRelativeStrength"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.initialStepSize.name"), YIFANHU_CATEGORY, "YifanHu.initialStepSize.name", NbBundle.getMessage(this.GetType(), "YifanHu.initialStepSize.desc"), "getInitialStep", "setInitialStep"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.stepRatio.name"), YIFANHU_CATEGORY, "YifanHu.stepRatio.name", NbBundle.getMessage(this.GetType(), "YifanHu.stepRatio.desc"), "getStepRatio", "setStepRatio"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "YifanHu.adaptativeCooling.name"), YIFANHU_CATEGORY, "YifanHu.adaptativeCooling.name", NbBundle.getMessage(this.GetType(), "YifanHu.adaptativeCooling.desc"), "isAdaptiveCooling", "setAdaptiveCooling"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.convergenceThreshold.name"), YIFANHU_CATEGORY, "YifanHu.convergenceThreshold.name", NbBundle.getMessage(this.GetType(), "YifanHu.convergenceThreshold.desc"), "getConvergenceThreshold", "setConvergenceThreshold"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(this.GetType(), "YifanHu.quadTreeMaxLevel.name"), BARNESHUT_CATEGORY, "YifanHu.quadTreeMaxLevel.name", NbBundle.getMessage(this.GetType(), "YifanHu.quadTreeMaxLevel.desc"), "getQuadTreeMaxLevel", "setQuadTreeMaxLevel"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(this.GetType(), "YifanHu.theta.name"), BARNESHUT_CATEGORY, "YifanHu.theta.name", NbBundle.getMessage(this.GetType(), "YifanHu.theta.desc"), "getBarnesHutTheta", "setBarnesHutTheta"));
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
    
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		public override void initAlgo()
		{
			if (graphModel == null)
			{
				return;
			}
			graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				energy = float.PositiveInfinity;
				foreach (Node n in graph.Nodes)
				{
					n.LayoutData = new ForceVector();
				}
				progress = 0;
				Converged = false;
				Step = initialStep;
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

		public override void goAlgo()
		{
			graph = graphModel.GraphVisible;
			graph.readLock();
			try
			{
				Node[] nodes = graph.Nodes.toArray();
				foreach (Node n in nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is ForceVector))
					{
						n.LayoutData = new ForceVector();
					}
				}

				// Evaluates n^2 inter node forces using BarnesHut.
				QuadTree tree = QuadTree.buildTree(graph, QuadTreeMaxLevel.Value);

				//        double electricEnergy = 0; ///////////////////////
				//        double springEnergy = 0; ///////////////////////
				BarnesHut barnes = new BarnesHut(NodeForce);
				barnes.Theta = BarnesHutTheta.Value;
				foreach (Node node in nodes)
				{
					ForceVector layoutData = node.LayoutData;

					ForceVector f = barnes.calculateForce(node, tree);
					layoutData.add(f);
					//            electricEnergy += f.getEnergy();
				}

				// Apply edge forces.
				foreach (Edge e in graph.Edges)
				{
					if (!e.Source.Equals(e.Target))
					{
						Node n1 = e.Source;
						Node n2 = e.Target;
						ForceVector f1 = n1.LayoutData;
						ForceVector f2 = n2.LayoutData;

						ForceVector f = EdgeForce.calculateForce(n1, n2);
						f1.add(f);
						f2.subtract(f);
					}
				}

				// Calculate energy and max force.
				energy0 = energy;
				energy = 0;
				double maxForce = 1;
				foreach (Node n in nodes)
				{
					ForceVector force = n.LayoutData;

					energy += force.Norm;
					maxForce = Math.Max(maxForce, force.Norm);
				}

				// Apply displacements on nodes.
				foreach (Node n in nodes)
				{
					if (!n.Fixed)
					{
						ForceVector force = n.LayoutData;

						force.multiply((float)(1.0 / maxForce));
						Displacement.moveNode(n, force);
					}
				}
				postAlgo();
			}
			finally
			{
				graph.readUnlockAll();
			}
		}


		/* Maximum level for Barnes-Hut's quadtree */
		public virtual int? QuadTreeMaxLevel
		{
			get
			{
				return quadTreeMaxLevel;
			}
			set
			{
				this.quadTreeMaxLevel = value.Value;
			}
		}


		/* theta is the parameter for Barnes-Hut opening criteria */
		public virtual float? BarnesHutTheta
		{
			get
			{
				return barnesHutTheta;
			}
			set
			{
				this.barnesHutTheta = value.Value;
			}
		}


		/// <returns> the optimalDistance </returns>
		public virtual float? OptimalDistance
		{
			get
			{
				return optimalDistance;
			}
			set
			{
				this.optimalDistance = value.Value;
			}
		}


		/// <returns> the relativeStrength </returns>
		public virtual float? RelativeStrength
		{
			get
			{
				return relativeStrength;
			}
			set
			{
				this.relativeStrength = value.Value;
			}
		}


		/// <param name="step"> the step to set </param>
		public virtual float? Step
		{
			set
			{
				this.step = value.Value;
			}
		}

		/// <returns> the adaptiveCooling </returns>
		public virtual bool? AdaptiveCooling
		{
			get
			{
				return adaptiveCooling;
			}
			set
			{
				this.adaptiveCooling = value.Value;
			}
		}


		/// <returns> the stepRatio </returns>
		public virtual float? StepRatio
		{
			get
			{
				return stepRatio;
			}
			set
			{
				this.stepRatio = value.Value;
			}
		}


		/// <returns> the convergenceThreshold </returns>
		public virtual float? ConvergenceThreshold
		{
			get
			{
				return convergenceThreshold;
			}
			set
			{
				this.convergenceThreshold = value.Value;
			}
		}


		/// <returns> the initialStep </returns>
		public virtual float? InitialStep
		{
			get
			{
				return initialStep;
			}
			set
			{
				this.initialStep = value.Value;
			}
		}


		/// <summary>
		/// Fa = (n2 - n1) * ||n2 - n1|| / K
		/// 
		/// @author Helder Suzuki
		/// </summary>
		public class SpringForce : AbstractForce
		{
			private readonly YifanHuLayout outerInstance;


			internal float optimalDistance;

			public SpringForce(YifanHuLayout outerInstance, float optimalDistance)
			{
				this.outerInstance = outerInstance;
				this.optimalDistance = optimalDistance;
			}

			public override ForceVector calculateForce(Node node1, Node node2, float distance)
			{
				ForceVector f = new ForceVector(node2.x() - node1.x(), node2.y() - node1.y());
				f.multiply(distance / optimalDistance);
				return f;
			}

			public virtual float? OptimalDistance
			{
				get
				{
					return optimalDistance;
				}
				set
				{
					this.optimalDistance = value.Value;
				}
			}

		}

		/// <summary>
		/// Fr = -C*K*K*(n2-n1)/||n2-n1||
		/// 
		/// @author Helder Suzuki
		/// </summary>
		public class ElectricalForce : AbstractForce
		{
			private readonly YifanHuLayout outerInstance;


			internal readonly float relativeStrength;
			internal readonly float optimalDistance;

			public ElectricalForce(YifanHuLayout outerInstance, float relativeStrength, float optimalDistance)
			{
				this.outerInstance = outerInstance;
				this.relativeStrength = relativeStrength;
				this.optimalDistance = optimalDistance;
			}

			public override ForceVector calculateForce(Node node1, Node node2, float distance)
			{
				ForceVector f = new ForceVector(node2.x() - node1.x(), node2.y() - node1.y());
				float scale = -relativeStrength * optimalDistance * optimalDistance / (distance * distance);
				if (float.IsNaN(scale) || float.IsInfinity(scale))
				{
					scale = -1;
				}

				f.multiply(scale);
				return f;
			}
		}
	}

}