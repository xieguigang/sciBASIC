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
	using Progress = org.gephi.utils.progress.Progress;
	using ProgressTicket = org.gephi.utils.progress.ProgressTicket;

	/// <summary>
	/// @author Mathieu Bastian
	/// </summary>
	public class Control
	{

		//Settings
		private int STAGE;
		private int iterations;
		private float temperature;
		private float attraction;
		private float dampingMult;
		private float minEdges;
		private float cutEnd;
		private float cutLengthEnd;
		private float cutOffLength;
		private float cutRate;
		private bool fineDensity;
		//Vars
		private float edgeCut;
		private float realParm;
		//Exec
		private long startTime;
		private long stopTime;
		private int numNodes;
		private float highestSimilarity;
		private int realIterations;
		private bool realFixed;
		private int totIterations;
		private int totExpectedIterations;
		private long totalTime;
		private Params @params;
		private ProgressTicket progressTicket;

		public virtual void initParams(Params @params, int totalIterations)
		{
			this.@params = @params;
			STAGE = 0;
			iterations = 0;
			initStage(@params.Initial);
			minEdges = 20;
			fineDensity = false;

			cutEnd = cutLengthEnd = 40000f * (1f - edgeCut);
			if (cutLengthEnd <= 1f)
			{
				cutLengthEnd = 1f;
			}

			float cutLengthStart = 4f * cutLengthEnd;

			cutOffLength = cutLengthStart;
			cutRate = (cutLengthStart - cutLengthEnd) / 400f;

			totExpectedIterations = totalIterations;

			int fullCompIters = totExpectedIterations + 3;

			if (realParm < 0)
			{
				realIterations = (int) realParm;
			}
			else if (realParm == 1)
			{
				realIterations = fullCompIters + @params.Simmer.getIterationsTotal(totalIterations) + 100;
			}
			else
			{
				realIterations = (int)(realParm * fullCompIters);
			}
			Logger.getLogger("").info("Real iterations " + realIterations);

			realFixed = realIterations > 0;

			Progress.switchToDeterminate(progressTicket, totExpectedIterations);
		}

		private void initStage(Params.Stage stage)
		{
			temperature = stage.Temperature;
			attraction = stage.Attraction;
			dampingMult = stage.DampingMult;
		}

		public virtual void initWorker(Worker worker)
		{
			worker.Attraction = attraction;
			worker.CutOffLength = cutOffLength;
			worker.DampingMult = dampingMult;
			worker.MinEdges = minEdges;
			worker.STAGE = STAGE;
			worker.Temperature = temperature;
			worker.FineDensity = fineDensity;
		}

		public virtual bool udpateStage(float totEnergy)
		{
			int MIN = 1;

			totIterations++;
			if (totIterations >= realIterations)
			{
				realFixed = false;
			}

			Progress.progress(progressTicket, totIterations);
			//System.out.println("Progress "+progress+"%");

			if (STAGE == 0)
			{

				if (iterations == 0)
				{
					startTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					Logger.getLogger("").info("Entering liquid stage...");
				}

				if (iterations < @params.Liquid.getIterationsTotal(totExpectedIterations))
				{
					initStage(@params.Liquid);
					iterations++;
				}
				else
				{
					stopTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					long timeElapsed = (stopTime - startTime);
					totalTime += timeElapsed;
					initStage(@params.Expansion);
					iterations = 0;

					Logger.getLogger("").info(string.Format("Liquid stage completed in {0:D} seconds, total energy = {1:F}", timeElapsed, totEnergy));

					STAGE = 1;
					startTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;

					Logger.getLogger("").info("Entering expansion stage...");
				}
			}

			if (STAGE == 1)
			{

				if (iterations < @params.Expansion.getIterationsTotal(totExpectedIterations))
				{
					// Play with vars
					if (attraction > 1)
					{
						attraction -= .05;
					}
					if (minEdges > 12)
					{
						minEdges -= .05;
					}
					cutOffLength -= cutRate;
					if (dampingMult > .1)
					{
						dampingMult -= .005;
					}
					iterations++;

				}
				else
				{

					stopTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					long timeElapsed = (stopTime - startTime);
					totalTime += timeElapsed;

					Logger.getLogger("").info(string.Format("Expansion stage completed in {0:D} seconds, total energy = {1:F}", timeElapsed, totEnergy));

					STAGE = 2;
					minEdges = 12;
					initStage(@params.Cooldown);
					iterations = 0;
					startTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;

					Logger.getLogger("").info("Entering cool-down stage...");
				}
			}
			else if (STAGE == 2)
			{

				if (iterations < @params.Cooldown.getIterationsTotal(totExpectedIterations))
				{

					// Reduce temperature
					if (temperature > 50)
					{
						temperature -= 10;
					}

					// Reduce cut length
					if (cutOffLength > cutLengthEnd)
					{
						cutLengthEnd -= cutRate * 2;
					}
					if (minEdges > MIN)
					{
						minEdges -= .2;
					}
					//min_edges = 99;
					iterations++;

				}
				else
				{

					stopTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					long timeElapsed = (stopTime - startTime);
					totalTime += timeElapsed;

					cutOffLength = cutLengthEnd;
					minEdges = MIN;
					//min_edges = 99; // In other words: no more cutting

					Logger.getLogger("").info(string.Format("Cool-down stage completed in {0:D} seconds, total energy = {1:F}", timeElapsed, totEnergy));

					STAGE = 3;
					iterations = 0;
					initStage(@params.Crunch);
					startTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;

					Logger.getLogger("").info("Entering crunch stage...");
				}
			}
			else if (STAGE == 3)
			{

				if (iterations < @params.Crunch.getIterationsTotal(totExpectedIterations))
				{
					iterations++;
				}
				else
				{
					stopTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					long timeElapsed = (stopTime - startTime);
					totalTime += timeElapsed;

					iterations = 0;
					initStage(@params.Simmer);
					minEdges = 99;
					fineDensity = true;

					Logger.getLogger("").info(string.Format("Crunch stage completed in {0:D} seconds, total energy = {1:F}", timeElapsed, totEnergy));

					STAGE = 5;
					startTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;

					Logger.getLogger("").info("Entering simmer stage...");
				}
			}
			else if (STAGE == 5)
			{

				if (iterations < @params.Simmer.getIterationsTotal(totExpectedIterations))
				{
					if (temperature > 50)
					{
						temperature -= 2;
					}
					iterations++;

				}
				else
				{
					stopTime = DateTimeHelper.CurrentUnixTimeMillis() / 1000;
					long timeElapsed = (stopTime - startTime);
					totalTime += timeElapsed;

					Logger.getLogger("").info(string.Format("Simmer stage completed in {0:D} seconds, total energy = {1:F}", timeElapsed, totEnergy));

					STAGE = 6;

					Logger.getLogger("").info(string.Format("Layout completed in {0:D} seconds with {1:D} iterations", totalTime, totIterations));
				}
			}
			else
			{
				return STAGE != 6;
			}

			return true;
		}

		public virtual bool RealFixed
		{
			get
			{
				return realFixed;
			}
		}

		public virtual float HighestSimilarity
		{
			get
			{
				return highestSimilarity;
			}
			set
			{
				this.highestSimilarity = value;
			}
		}


		public virtual int NumNodes
		{
			set
			{
				this.numNodes = value;
			}
		}

		public virtual float EdgeCut
		{
			set
			{
				this.edgeCut = value;
			}
		}

		public virtual float RealParm
		{
			set
			{
				this.realParm = value;
			}
		}

		public virtual ProgressTicket ProgressTicket
		{
			set
			{
				this.progressTicket = value;
			}
		}
	}

}