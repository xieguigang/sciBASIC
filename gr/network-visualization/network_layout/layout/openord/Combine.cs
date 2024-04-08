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
	using Graph = org.gephi.graph.api.Graph;
	using Exceptions = org.openide.util.Exceptions;

	/// <summary>
	/// @author Mathieu Bastian
	/// </summary>
	public class Combine : ThreadStart
	{

		private readonly OpenOrdLayout layout;
		private readonly object @lock = new object();
		private readonly Control control;

		public Combine(OpenOrdLayout layout)
		{
			this.layout = layout;
			this.control = layout.Control;
		}

		public override void run()
		{
			//System.out.println("Combine results");

			Worker[] workers = layout.Workers;

			//Gather positions
			Node[] positions = null;
			foreach (Worker w in workers)
			{
				if (positions == null)
				{
					positions = w.Positions;
				}
				else
				{
					Node[] workerPositions = w.Positions;
					for (int i = w.Id; i < positions.Length; i += workers.Length)
					{
						positions[i] = workerPositions[i];
					}
				}
			}

			//Unfix positions if necessary
			if (!control.RealFixed)
			{
				foreach (Node n in positions)
				{
					n.@fixed = false;
				}
			}

			//Combine density
			foreach (Worker w in workers)
			{
				DensityGrid densityGrid = w.DensityGrid;
				bool fineDensity = w.FineDensity;
				bool firstAdd = w.FirstAdd;
				bool fineFirstAdd = w.FineFirstAdd;
				Node[] wNodes = w.Positions;
				foreach (Worker z in workers)
				{
					if (w != z)
					{
						Node[] zNodes = w.Positions;
						for (int i = z.Id; i < wNodes.Length; i += workers.Length)
						{
							densityGrid.substract(wNodes[i], firstAdd, fineFirstAdd, fineDensity);
							densityGrid.add(zNodes[i], fineDensity);
						}
					}
				}
			}

			//Redistribute positions to workers
			if (workers.Length > 1)
			{
				foreach (Worker w in workers)
				{
					Node[] positionsCopy = new Node[positions.Length];
					for (int i = 0; i < positions.Length; i++)
					{
						positionsCopy[i] = positions[i].clone();
					}
					w.Positions = positionsCopy;
				}
			}

			float totEnergy = TotEnergy;
			bool done = !control.udpateStage(totEnergy);

			//Params
			foreach (Worker w in layout.Workers)
			{
				control.initWorker(w);
			}

			//Write positions to nodes
			Graph graph = layout.Graph;
			foreach (org.gephi.graph.api.Node n in graph.Nodes)
			{
				if (n.LayoutData != null && n.LayoutData is OpenOrdLayoutData)
				{
					OpenOrdLayoutData layoutData = n.LayoutData;
					Node node = positions[layoutData.nodeId];
					n.X = node.x * 10f;
					n.Y = node.y * 10f;
				}
			}

			//Finish
			if (!layout.canAlgo() || done)
			{
				foreach (Worker w in layout.Workers)
				{
					w.Done = true;
				}
				layout.Running = false;
			}

			//Synchronize with layout goAlgo()
			lock (@lock)
			{
				Monitor.Pulse(@lock);
			}
		}

		private void printPositions(Node[] nodes)
		{
			NumberFormat formatter = DecimalFormat.Instance;
			formatter.MaximumFractionDigits = 2;
			foreach (Node node in nodes)
			{
				string xStr = formatter.format(node.x);
				string yStr = formatter.format(node.y);
			}
		}

		public virtual float TotEnergy
		{
			get
			{
				float totEnergy = 0;
				foreach (Worker w in layout.Workers)
				{
					totEnergy += w.TotEnergy;
				}
				return totEnergy;
			}
		}

		public virtual void waitForIteration()
		{
			try
			{
				lock (@lock)
				{
					Monitor.Wait(@lock);
				}
			}
			catch (InterruptedException ex)
			{
				Exceptions.printStackTrace(ex);
			}
		}
	}

}