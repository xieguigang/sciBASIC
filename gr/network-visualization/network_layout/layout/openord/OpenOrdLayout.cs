using System;
using System.Collections.Generic;
using System.Threading;

/*
Copyright 2008-2010 Gephi
Authors : Mathieu Bastian <mathieu.bastian@gephi.org>
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

namespace org.gephi.layout.plugin.openord
{
	using TIntFloatIterator = gnu.trove.iterator.TIntFloatIterator;
	using TIntFloatHashMap = gnu.trove.map.hash.TIntFloatHashMap;
	using TIntIntHashMap = gnu.trove.map.hash.TIntIntHashMap;
	using Edge = org.gephi.graph.api.Edge;
	using Graph = org.gephi.graph.api.Graph;
	using GraphModel = org.gephi.graph.api.GraphModel;
	using Interval = org.gephi.graph.api.Interval;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutBuilder = org.gephi.layout.spi.LayoutBuilder;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using LongTask = org.gephi.utils.longtask.spi.LongTask;
	using ProgressTicket = org.gephi.utils.progress.ProgressTicket;
	using Exceptions = org.openide.util.Exceptions;
	using NbBundle = org.openide.util.NbBundle;

	/// <summary>
	/// @author Mathieu Bastian
	/// </summary>
	public class OpenOrdLayout : Layout, LongTask
	{

		//Architecture
		private readonly LayoutBuilder builder;
		private GraphModel graphModel;
		private bool running = true;
		private ProgressTicket progressTicket;
		//Settings
		private Params param = Params.DEFAULT;
		private float edgeCut;
		private int numThreads;
		private long randSeed;
		private int numIterations;
		private float realTime;
		//Layout
		private Worker[] workers;
		private Combine combine;
		private Control control;
		private CyclicBarrier barrier;
		private Graph graph;
		private bool firstIteration = true;

		public OpenOrdLayout(LayoutBuilder builder)
		{
			this.builder = builder;
		}

		public override void resetPropertiesValues()
		{
			edgeCut = 0.8f;
			numIterations = 750;
			numThreads = Math.Max(1, Runtime.Runtime.availableProcessors() - 1);
			Random r = new Random();
			randSeed = r.nextLong();
			running = true;
			realTime = 0.2f;
			param = Params.DEFAULT;
		}

		public override void initAlgo()
		{
			//Verify param
			if (param.IterationsSum != 1f)
			{
				param = Params.DEFAULT;
				//throw new RuntimeException("The sum of the time for each stage must be equal to 1");
			}

			//Get graph
			graph = graphModel.UndirectedGraphVisible;
			graph.readLock();
			bool isDynamicWeight = graphModel.EdgeTable.getColumn("weight").Dynamic;
			Interval interval = graph.View.TimeInterval;

			try
			{
				int numNodes = graph.NodeCount;

				//Prepare data structure - nodes and neighbors map
				Node[] nodes = new Node[numNodes];
				TIntFloatHashMap[] neighbors = new TIntFloatHashMap[numNodes];

				//Load nodes and edges
				TIntIntHashMap idMap = new TIntIntHashMap(numNodes, 1f);
				org.gephi.graph.api.Node[] graphNodes = graph.Nodes.toArray();
				for (int i = 0; i < numNodes; i++)
				{
					org.gephi.graph.api.Node n = graphNodes[i];
					nodes[i] = new Node(i);
					nodes[i].x = n.x();
					nodes[i].y = n.y();
					nodes[i].@fixed = n.Fixed;
					OpenOrdLayoutData layoutData = new OpenOrdLayoutData(i);
					n.LayoutData = layoutData;
					idMap.put(n.StoreId, i);
				}
				float highestSimilarity = float.NegativeInfinity;
				foreach (Edge e in graph.Edges)
				{
					int source = idMap.get(e.Source.StoreId);
					int target = idMap.get(e.Target.StoreId);
					if (source != target)
					{ //No self-loop
						float weight = (float)(isDynamicWeight ? e.getWeight(interval) : e.Weight);
						if (neighbors[source] == null)
						{
							neighbors[source] = new TIntFloatHashMap();
						}
						if (neighbors[target] == null)
						{
							neighbors[target] = new TIntFloatHashMap();
						}
						neighbors[source].put(target, weight);
						neighbors[target].put(source, weight);
						highestSimilarity = Math.Max(highestSimilarity, weight);
					}
				}

				//Reset position
				bool someFixed = false;
				foreach (Node n in nodes)
				{
					if (!n.@fixed)
					{
						n.x = 0;
						n.y = 0;
					}
					else
					{
						someFixed = true;
					}
				}

				//Recenter fixed nodes and rescale to fit into grid
				if (someFixed)
				{
					float minX = float.PositiveInfinity;
					float maxX = float.NegativeInfinity;
					float minY = float.PositiveInfinity;
					float maxY = float.NegativeInfinity;
					foreach (Node n in nodes)
					{
						if (n.@fixed)
						{
							minX = Math.Min(minX, n.x);
							maxX = Math.Max(maxX, n.x);
							minY = Math.Min(minY, n.y);
							maxY = Math.Max(maxY, n.y);
						}
					}
					float shiftX = minX + (maxX - minX) / 2f;
					float shiftY = minY + (maxY - minY) / 2f;
					float ratio = Math.Min(DensityGrid.ViewSize / (maxX - minX), DensityGrid.ViewSize / (maxY - minY));
					ratio = Math.Min(1f, ratio);
					foreach (Node n in nodes)
					{
						if (n.@fixed)
						{
							n.x = (n.x - shiftX) * ratio;
							n.y = (n.y - shiftY) * ratio;
						}
					}
				}

				//Init control and workers
				control = new Control();
				combine = new Combine(this);
				barrier = new CyclicBarrier(numThreads, combine);
				control.EdgeCut = edgeCut;
				control.RealParm = realTime;
				control.ProgressTicket = progressTicket;
				control.initParams(param, numIterations);
				control.NumNodes = numNodes;
				control.HighestSimilarity = highestSimilarity;

				workers = new Worker[numThreads];
				for (int i = 0; i < numThreads; ++i)
				{
					workers[i] = new Worker(i, numThreads, barrier);
					workers[i].Random = new Random(randSeed);
					control.initWorker(workers[i]);
				}

				//Load workers with data
				//Deep copy of all nodes positions
				//Deep copy of a partition of all neighbors for each workers
				foreach (Worker w in workers)
				{
					Node[] nodesCopy = new Node[nodes.Length];
					for (int i = 0; i < nodes.Length; i++)
					{
						nodesCopy[i] = nodes[i].clone();
					}
					TIntFloatHashMap[] neighborsCopy = new TIntFloatHashMap[numNodes];
					for (int i = 0; i < neighbors.Length; i++)
					{
						if (i % numThreads == w.Id && neighbors[i] != null)
						{
							int neighborsCount = neighbors[i].size();
							neighborsCopy[i] = new TIntFloatHashMap(neighborsCount, 1f);
							for (TIntFloatIterator itr = neighbors[i].GetEnumerator(); itr.hasNext();)
							{
								itr.advance();
								float weight = normalizeWeight(itr.value(), highestSimilarity);
								neighborsCopy[i].put(itr.key(), weight);
							}
						}
					}
					w.Positions = nodesCopy;
					w.Neighbors = neighborsCopy;
				}

				//Add real nodes
				foreach (Node n in nodes)
				{
					if (n.@fixed)
					{
						foreach (Worker w in workers)
						{
							w.DensityGrid.add(n, w.FineDensity);
						}
					}
				}

				running = true;
				firstIteration = true;
			}
			finally
			{
				graph.readUnlockAll();
			}
		}

		public override void goAlgo()
		{
			if (firstIteration)
			{
				for (int i = 0; i < numThreads; ++i)
				{
					Thread t = new Thread(workers[i]);
					t.Daemon = true;
					t.Start();
				}
				firstIteration = false;
			}

			combine.waitForIteration();
		}

		public override void endAlgo()
		{
			running = false;
			combine = null;
		}

		private float normalizeWeight(float weight, float highestSimilarity)
		{
			weight /= highestSimilarity;
			weight = weight * Math.Abs(weight);
			return weight;
		}

		public override bool canAlgo()
		{
			return running;
		}

		public override GraphModel GraphModel
		{
			set
			{
				this.graphModel = value;
			}
		}

		public override LayoutProperty[] Properties
		{
			get
			{
				IList<LayoutProperty> properties = new List<LayoutProperty>();
				const string OPENORD = "OpenOrd";
				const string STAGE = "Stages";
    
				try
				{
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.edgecut.name"), OPENORD, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.edgecut.description"), "getEdgeCut", "setEdgeCut"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.numthreads.name"), OPENORD, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.numthreads.description"), "getNumThreads", "setNumThreads"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.numiterations.name"), OPENORD, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.numiterations.description"), "getNumIterations", "setNumIterations"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Float), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.realtime.name"), OPENORD, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.realtime.description"), "getRealTime", "setRealTime"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Long), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.seed.name"), OPENORD, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.seed.description"), "getRandSeed", "setRandSeed"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.liquid.name"), STAGE, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.liquid.description"), "getLiquidStage", "setLiquidStage"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.expansion.name"), STAGE, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.expansion.description"), "getExpansionStage", "setExpansionStage"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.cooldown.name"), STAGE, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.cooldown.description"), "getCooldownStage", "setCooldownStage"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.crunch.name"), STAGE, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.crunch.description"), "getCrunchStage", "setCrunchStage"));
					properties.Add(LayoutProperty.createProperty(this, typeof(Integer), NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.simmer.name"), STAGE, NbBundle.getMessage(typeof(OpenOrdLayout), "OpenOrd.properties.stage.simmer.description"), "getSimmerStage", "setSimmerStage"));
    
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
    
				return ((List<LayoutProperty>)properties).ToArray();
			}
		}

		public virtual float? EdgeCut
		{
			get
			{
				return edgeCut;
			}
			set
			{
				value = Math.Min(1f, value);
				value = Math.Max(0, value);
				this.edgeCut = value.Value;
			}
		}


		public virtual int? NumThreads
		{
			get
			{
				return numThreads;
			}
			set
			{
				value = Math.Max(1, value);
				this.numThreads = value.Value;
			}
		}


		public virtual long? RandSeed
		{
			get
			{
				return randSeed;
			}
			set
			{
				this.randSeed = value.Value;
			}
		}


		public virtual bool? Running
		{
			set
			{
				this.running = value.Value;
			}
		}

		public virtual int? NumIterations
		{
			get
			{
				return numIterations;
			}
			set
			{
				value = Math.Max(100, value);
				this.numIterations = value.Value;
			}
		}


		public virtual float? RealTime
		{
			get
			{
				return realTime;
			}
			set
			{
				value = Math.Min(1f, value);
				value = Math.Max(0, value);
				this.realTime = value.Value;
			}
		}


		public virtual int? LiquidStage
		{
			get
			{
				return param.Liquid.IterationsPercentage;
			}
			set
			{
				int v = Math.Min(100, value);
				v = Math.Max(0, v);
				param.Liquid.Iterations = v / 100f;
			}
		}


		public virtual int? ExpansionStage
		{
			get
			{
				return param.Expansion.IterationsPercentage;
			}
			set
			{
				int v = Math.Min(100, value);
				v = Math.Max(0, v);
				param.Expansion.Iterations = v / 100f;
			}
		}


		public virtual int? CooldownStage
		{
			get
			{
				return param.Cooldown.IterationsPercentage;
			}
			set
			{
				int v = Math.Min(100, value);
				v = Math.Max(0, v);
				param.Cooldown.Iterations = v / 100f;
			}
		}


		public virtual int? CrunchStage
		{
			get
			{
				return param.Crunch.IterationsPercentage;
			}
			set
			{
				int v = Math.Min(100, value);
				v = Math.Max(0, v);
				param.Crunch.Iterations = v / 100f;
			}
		}


		public virtual int? SimmerStage
		{
			get
			{
				return param.Simmer.IterationsPercentage;
			}
			set
			{
				int v = Math.Min(100, value);
				v = Math.Max(0, v);
				param.Simmer.Iterations = v / 100f;
			}
		}


		public override LayoutBuilder Builder
		{
			get
			{
				return builder;
			}
		}

		public virtual Worker[] Workers
		{
			get
			{
				return workers;
			}
		}

		public virtual Graph Graph
		{
			get
			{
				return graph;
			}
		}

		public virtual Control Control
		{
			get
			{
				return control;
			}
		}

		public override bool cancel()
		{
			return true;
		}

		public override ProgressTicket ProgressTicket
		{
			set
			{
				this.progressTicket = value;
			}
		}
	}

}