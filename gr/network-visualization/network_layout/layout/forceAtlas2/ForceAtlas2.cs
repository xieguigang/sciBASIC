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
	using Edge = org.gephi.graph.api.Edge;
	using Graph = org.gephi.graph.api.Graph;
	using GraphModel = org.gephi.graph.api.GraphModel;
	using Interval = org.gephi.graph.api.Interval;
	using Node = org.gephi.graph.api.Node;
	using AttractionForce = org.gephi.layout.plugin.forceAtlas2.ForceFactory.AttractionForce;
	using RepulsionForce = org.gephi.layout.plugin.forceAtlas2.ForceFactory.RepulsionForce;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// ForceAtlas 2 Layout, manages each step of the computations.
	/// 
	/// @author Mathieu Jacomy
	/// </summary>
	public class ForceAtlas2 : Layout
	{

		private readonly ForceAtlas2Builder layoutBuilder;
		internal double outboundAttCompensation = 1;
		private GraphModel graphModel;
		private Graph graph;
		private double edgeWeightInfluence;
		private double jitterTolerance;
		private double scalingRatio;
		private double gravity;
		private double speed;
		private double speedEfficiency;
		private bool outboundAttractionDistribution;
		private bool adjustSizes;
		private bool barnesHutOptimize;
		private double barnesHutTheta;
		private bool linLogMode;
		private bool normalizeEdgeWeights;
		private bool strongGravityMode;
		private bool invertedEdgeWeightsMode;
		private int threadCount;
		private int currentThreadCount;
		private Region rootRegion;
		private ExecutorService pool;

		public ForceAtlas2(ForceAtlas2Builder layoutBuilder)
		{
			this.layoutBuilder = layoutBuilder;
			this.threadCount = Math.Min(4, Math.Max(1, Runtime.Runtime.availableProcessors() - 1));
		}

		public override void initAlgo()
		{
			AbstractLayout.ensureSafeLayoutNodePositions(graphModel);

			speed = 1.0;
			speedEfficiency = 1.0;

			graph = graphModel.GraphVisible;

			graph.readLock();
			try
			{
				Node[] nodes = graph.Nodes.toArray();

				// Initialise layout data
				foreach (Node n in nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is ForceAtlas2LayoutData))
					{
						ForceAtlas2LayoutData nLayout = new ForceAtlas2LayoutData();
						n.LayoutData = nLayout;
					}
					ForceAtlas2LayoutData nLayout = n.LayoutData;
					nLayout.mass = 1 + graph.getDegree(n);
					nLayout.old_dx = 0;
					nLayout.old_dy = 0;
					nLayout.dx = 0;
					nLayout.dy = 0;
				}

				pool = Executors.newFixedThreadPool(threadCount);
				currentThreadCount = threadCount;
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		private double getEdgeWeight(Edge edge, bool isDynamicWeight, Interval interval)
		{
			double w = edge.Weight;
			if (isDynamicWeight)
			{
				w = edge.getWeight(interval);
			}
			if (InvertedEdgeWeightsMode.Value)
			{
				return w == 0 ? 0 : 1 / w;
			}
			return w;
		}


		public override void goAlgo()
		{
			// Initialize graph data
			if (graphModel == null)
			{
				return;
			}
			graph = graphModel.GraphVisible;
			graph.readLock();
			bool isDynamicWeight = graphModel.EdgeTable.getColumn("weight").Dynamic;
			Interval interval = graph.View.TimeInterval;

			try
			{
				Node[] nodes = graph.Nodes.toArray();
				Edge[] edges = graph.Edges.toArray();

				// Initialise layout data
				foreach (Node n in nodes)
				{
					if (n.LayoutData == null || !(n.LayoutData is ForceAtlas2LayoutData))
					{
						ForceAtlas2LayoutData nLayout = new ForceAtlas2LayoutData();
						n.LayoutData = nLayout;
					}
					ForceAtlas2LayoutData nLayout = n.LayoutData;
					nLayout.mass = 1 + graph.getDegree(n);
					nLayout.old_dx = nLayout.dx;
					nLayout.old_dy = nLayout.dy;
					nLayout.dx = 0;
					nLayout.dy = 0;
				}

				// If Barnes Hut active, initialize root region
				if (BarnesHutOptimize.Value)
				{
					rootRegion = new Region(nodes);
					rootRegion.buildSubRegions();
				}

				// If outboundAttractionDistribution active, compensate.
				if (OutboundAttractionDistribution.Value)
				{
					outboundAttCompensation = 0;
					foreach (Node n in nodes)
					{
						ForceAtlas2LayoutData nLayout = n.LayoutData;
						outboundAttCompensation += nLayout.mass;
					}
					outboundAttCompensation /= nodes.Length;
				}

				// Repulsion (and gravity)
				// NB: Muti-threaded
				RepulsionForce Repulsion = ForceFactory.builder.buildRepulsion(AdjustSizes.Value, ScalingRatio.Value);

				int taskCount = 8 * currentThreadCount; // The threadPool Executor Service will manage the fetching of tasks and threads.
				// We make more tasks than threads because some tasks may need more time to compute.
				List<Future> threads = new List<object>();
				for (int t = taskCount; t > 0; t--)
				{
					int from = (int) Math.Floor(nodes.Length * (t - 1) / taskCount);
					int to = (int) Math.Floor(nodes.Length * t / taskCount);
					Future future = pool.submit(new NodesThread(nodes, from, to, BarnesHutOptimize.Value, BarnesHutTheta.Value, Gravity.Value, (StrongGravityMode) ? (ForceFactory.builder.getStrongGravity(ScalingRatio.Value)) : (Repulsion), ScalingRatio.Value, rootRegion, Repulsion));
					threads.Add(future);
				}
				foreach (Future future in threads)
				{
					try
					{
						future.get();
					}
					catch (Exception e)
					{
						throw new Exception("Unable to layout " + this.GetType().Name + ".", e);
					}
				}

				// Attraction
				AttractionForce Attraction = ForceFactory.builder.buildAttraction(LinLogMode.Value, OutboundAttractionDistribution.Value, AdjustSizes.Value, 1 * ((OutboundAttractionDistribution) ? (outboundAttCompensation) : (1)));
				if (EdgeWeightInfluence == 0)
				{
					foreach (Edge e in edges)
					{
						Attraction.apply(e.Source, e.Target, 1);
					}
				}
				else if (EdgeWeightInfluence == 1)
				{
					if (NormalizeEdgeWeights.Value)
					{
						double? w;
						double? edgeWeightMin = double.MaxValue;
						double? edgeWeightMax = double.Epsilon;
						foreach (Edge e in edges)
						{
							w = getEdgeWeight(e, isDynamicWeight, interval);
							edgeWeightMin = Math.Min(w, edgeWeightMin);
							edgeWeightMax = Math.Max(w, edgeWeightMax);
						}
						if (edgeWeightMin < edgeWeightMax)
						{
							foreach (Edge e in edges)
							{
								w = (getEdgeWeight(e, isDynamicWeight, interval) - edgeWeightMin.Value) / (edgeWeightMax.Value - edgeWeightMin.Value);
								Attraction.apply(e.Source, e.Target, w.Value);
							}
						}
						else
						{
							foreach (Edge e in edges)
							{
								Attraction.apply(e.Source, e.Target, 1.0);
							}
						}
					}
					else
					{
						foreach (Edge e in edges)
						{
							Attraction.apply(e.Source, e.Target, getEdgeWeight(e, isDynamicWeight, interval));
						}
					}
				}
				else
				{
					if (NormalizeEdgeWeights.Value)
					{
						double? w;
						double? edgeWeightMin = double.MaxValue;
						double? edgeWeightMax = double.Epsilon;
						foreach (Edge e in edges)
						{
							w = getEdgeWeight(e, isDynamicWeight, interval);
							edgeWeightMin = Math.Min(w, edgeWeightMin);
							edgeWeightMax = Math.Max(w, edgeWeightMax);
						}
						if (edgeWeightMin < edgeWeightMax)
						{
							foreach (Edge e in edges)
							{
								w = (getEdgeWeight(e, isDynamicWeight, interval) - edgeWeightMin.Value) / (edgeWeightMax.Value - edgeWeightMin.Value);
								Attraction.apply(e.Source, e.Target, Math.Pow(w, EdgeWeightInfluence));
							}
						}
						else
						{
							foreach (Edge e in edges)
							{
								Attraction.apply(e.Source, e.Target, 1.0);
							}
						}
					}
					else
					{
						foreach (Edge e in edges)
						{
							Attraction.apply(e.Source, e.Target, Math.Pow(getEdgeWeight(e, isDynamicWeight, interval), EdgeWeightInfluence));
						}
					}
				}

				// Auto adjust speed
				double totalSwinging = 0d; // How much irregular movement
				double totalEffectiveTraction = 0d; // Hom much useful movement
				foreach (Node n in nodes)
				{
					ForceAtlas2LayoutData nLayout = n.LayoutData;
					if (!n.Fixed)
					{
						double swinging = Math.Sqrt(Math.Pow(nLayout.old_dx - nLayout.dx, 2) + Math.Pow(nLayout.old_dy - nLayout.dy, 2));
						totalSwinging += nLayout.mass * swinging; // If the node has a burst change of direction, then it's not converging.
						totalEffectiveTraction += nLayout.mass * 0.5 * Math.Sqrt(Math.Pow(nLayout.old_dx + nLayout.dx, 2) + Math.Pow(nLayout.old_dy + nLayout.dy, 2));
					}
				}
				// We want that swingingMovement < tolerance * convergenceMovement

				// Optimize jitter tolerance
				// The 'right' jitter tolerance for this network. Bigger networks need more tolerance. Denser networks need less tolerance. Totally empiric.
				double estimatedOptimalJitterTolerance = 0.05 * Math.Sqrt(nodes.Length);
				double minJT = Math.Sqrt(estimatedOptimalJitterTolerance);
				double maxJT = 10;
				double jt = jitterTolerance * Math.Max(minJT, Math.Min(maxJT, estimatedOptimalJitterTolerance * totalEffectiveTraction / Math.Pow(nodes.Length, 2)));

				double minSpeedEfficiency = 0.05;

				// Protection against erratic behavior
				if (totalSwinging / totalEffectiveTraction > 2.0)
				{
					if (speedEfficiency > minSpeedEfficiency)
					{
						speedEfficiency *= 0.5;
					}
					jt = Math.Max(jt, jitterTolerance);
				}

				double targetSpeed = jt * speedEfficiency * totalEffectiveTraction / totalSwinging;

				// Speed efficiency is how the speed really corresponds to the swinging vs. convergence tradeoff
				// We adjust it slowly and carefully
				if (totalSwinging > jt * totalEffectiveTraction)
				{
					if (speedEfficiency > minSpeedEfficiency)
					{
						speedEfficiency *= 0.7;
					}
				}
				else if (speed < 1000)
				{
					speedEfficiency *= 1.3;
				}

				// But the speed shoudn't rise too much too quickly, since it would make the convergence drop dramatically.
				double maxRise = 0.5; // Max rise: 50%
				speed = speed + Math.Min(targetSpeed - speed, maxRise * speed);

				// Apply forces
				if (AdjustSizes.Value)
				{
					// If nodes overlap prevention is active, it's not possible to trust the swinging mesure.
					foreach (Node n in nodes)
					{
						ForceAtlas2LayoutData nLayout = n.LayoutData;
						if (!n.Fixed)
						{

							// Adaptive auto-speed: the speed of each node is lowered
							// when the node swings.
							double swinging = nLayout.mass * Math.Sqrt((nLayout.old_dx - nLayout.dx) * (nLayout.old_dx - nLayout.dx) + (nLayout.old_dy - nLayout.dy) * (nLayout.old_dy - nLayout.dy));
							double factor = 0.1 * speed / (1f + Math.Sqrt(speed * swinging));

							double df = Math.Sqrt(Math.Pow(nLayout.dx, 2) + Math.Pow(nLayout.dy, 2));
							factor = Math.Min(factor * df, 10.0) / df;

							double x = n.x() + nLayout.dx * factor;
							double y = n.y() + nLayout.dy * factor;

							n.X = (float) x;
							n.Y = (float) y;
						}
					}
				}
				else
				{
					foreach (Node n in nodes)
					{
						ForceAtlas2LayoutData nLayout = n.LayoutData;
						if (!n.Fixed)
						{

							// Adaptive auto-speed: the speed of each node is lowered
							// when the node swings.
							double swinging = nLayout.mass * Math.Sqrt((nLayout.old_dx - nLayout.dx) * (nLayout.old_dx - nLayout.dx) + (nLayout.old_dy - nLayout.dy) * (nLayout.old_dy - nLayout.dy));
							//double factor = speed / (1f + Math.sqrt(speed * swinging));
							double factor = speed / (1f + Math.Sqrt(speed * swinging));

							double x = n.x() + nLayout.dx * factor;
							double y = n.y() + nLayout.dy * factor;

							n.X = (float) x;
							n.Y = (float) y;
						}
					}
				}
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override bool canAlgo()
		{
			return graphModel != null;
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
				pool.shutdown();
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
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String FORCEATLAS2_TUNING = org.openide.util.NbBundle.getMessage(getClass(), "ForceAtlas2.tuning");
				string FORCEATLAS2_TUNING = NbBundle.getMessage(this.GetType(), "ForceAtlas2.tuning");
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String FORCEATLAS2_BEHAVIOR = org.openide.util.NbBundle.getMessage(getClass(), "ForceAtlas2.behavior");
				string FORCEATLAS2_BEHAVIOR = NbBundle.getMessage(this.GetType(), "ForceAtlas2.behavior");
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String FORCEATLAS2_PERFORMANCE = org.openide.util.NbBundle.getMessage(getClass(), "ForceAtlas2.performance");
				string FORCEATLAS2_PERFORMANCE = NbBundle.getMessage(this.GetType(), "ForceAtlas2.performance");
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String FORCEATLAS2_THREADS = org.openide.util.NbBundle.getMessage(getClass(), "ForceAtlas2.threads");
				string FORCEATLAS2_THREADS = NbBundle.getMessage(this.GetType(), "ForceAtlas2.threads");
    
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "ForceAtlas2.scalingRatio.name"), FORCEATLAS2_TUNING, "ForceAtlas2.scalingRatio.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.scalingRatio.desc"), "getScalingRatio", "setScalingRatio"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.strongGravityMode.name"), FORCEATLAS2_TUNING, "ForceAtlas2.strongGravityMode.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.strongGravityMode.desc"), "isStrongGravityMode", "setStrongGravityMode"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "ForceAtlas2.gravity.name"), FORCEATLAS2_TUNING, "ForceAtlas2.gravity.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.gravity.desc"), "getGravity", "setGravity"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.distributedAttraction.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.distributedAttraction.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.distributedAttraction.desc"), "isOutboundAttractionDistribution", "setOutboundAttractionDistribution"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.linLogMode.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.linLogMode.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.linLogMode.desc"), "isLinLogMode", "setLinLogMode"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.adjustSizes.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.adjustSizes.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.adjustSizes.desc"), "isAdjustSizes", "setAdjustSizes"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "ForceAtlas2.edgeWeightInfluence.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.edgeWeightInfluence.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.edgeWeightInfluence.desc"), "getEdgeWeightInfluence", "setEdgeWeightInfluence"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.normalizeEdgeWeights.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.normalizeEdgeWeights.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.normalizeEdgeWeights.desc"), "isNormalizeEdgeWeights", "setNormalizeEdgeWeights"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.invertedEdgeWeightsMode.name"), FORCEATLAS2_BEHAVIOR, "ForceAtlas2.invertedEdgeWeightsMode.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.invertedEdgeWeightsMode.desc"), "isInvertedEdgeWeightsMode", "setInvertedEdgeWeightsMode"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "ForceAtlas2.jitterTolerance.name"), FORCEATLAS2_PERFORMANCE, "ForceAtlas2.jitterTolerance.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.jitterTolerance.desc"), "getJitterTolerance", "setJitterTolerance"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Boolean), NbBundle.getMessage(this.GetType(), "ForceAtlas2.barnesHutOptimization.name"), FORCEATLAS2_PERFORMANCE, "ForceAtlas2.barnesHutOptimization.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.barnesHutOptimization.desc"), "isBarnesHutOptimize", "setBarnesHutOptimize"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Double), NbBundle.getMessage(this.GetType(), "ForceAtlas2.barnesHutTheta.name"), FORCEATLAS2_PERFORMANCE, "ForceAtlas2.barnesHutTheta.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.barnesHutTheta.desc"), "getBarnesHutTheta", "setBarnesHutTheta"));
    
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(this.GetType(), "ForceAtlas2.threads.name"), FORCEATLAS2_THREADS, "ForceAtlas2.threads.name", NbBundle.getMessage(this.GetType(), "ForceAtlas2.threads.desc"), "getThreadsCount", "setThreadsCount"));
    
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
			int nodesCount = 0;

			if (graphModel != null)
			{
				nodesCount = graphModel.GraphVisible.NodeCount;
			}

			// Tuning
			if (nodesCount >= 100)
			{
				ScalingRatio = 2.0;
			}
			else
			{
				ScalingRatio = 10.0;
			}
			StrongGravityMode = false;
			InvertedEdgeWeightsMode = false;
			Gravity = 1.0;

			// Behavior
			OutboundAttractionDistribution = false;
			LinLogMode = false;
			AdjustSizes = false;
			EdgeWeightInfluence = 1.0;
			NormalizeEdgeWeights = false;

			// Performance
			JitterTolerance = 1d;
			BarnesHutOptimize = nodesCount >= 1000;
			BarnesHutTheta = 1.2;
			ThreadsCount = Math.Max(1, Runtime.Runtime.availableProcessors() - 1);
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
				// Trick: reset here to take the profile of the graph in account for default values
				resetPropertiesValues();
			}
		}

		public virtual double? BarnesHutTheta
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


		public virtual double? EdgeWeightInfluence
		{
			get
			{
				return edgeWeightInfluence;
			}
			set
			{
				this.edgeWeightInfluence = value.Value;
			}
		}


		public virtual double? JitterTolerance
		{
			get
			{
				return jitterTolerance;
			}
			set
			{
				this.jitterTolerance = value.Value;
			}
		}


		public virtual bool? LinLogMode
		{
			get
			{
				return linLogMode;
			}
			set
			{
				this.linLogMode = value.Value;
			}
		}


		public virtual bool? NormalizeEdgeWeights
		{
			get
			{
				return normalizeEdgeWeights;
			}
			set
			{
				this.normalizeEdgeWeights = value.Value;
			}
		}


		public virtual double? ScalingRatio
		{
			get
			{
				return scalingRatio;
			}
			set
			{
				this.scalingRatio = value.Value;
			}
		}


		public virtual bool? StrongGravityMode
		{
			get
			{
				return strongGravityMode;
			}
			set
			{
				this.strongGravityMode = value.Value;
			}
		}



		public virtual bool? InvertedEdgeWeightsMode
		{
			get
			{
				return invertedEdgeWeightsMode;
			}
			set
			{
				this.invertedEdgeWeightsMode = value.Value;
			}
		}


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


		public virtual int? ThreadsCount
		{
			get
			{
				return threadCount;
			}
			set
			{
				this.threadCount = Math.Max(1, value);
			}
		}


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


		public virtual bool? BarnesHutOptimize
		{
			get
			{
				return barnesHutOptimize;
			}
			set
			{
				this.barnesHutOptimize = value.Value;
			}
		}

	}

}