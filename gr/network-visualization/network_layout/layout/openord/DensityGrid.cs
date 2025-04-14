using System;
using System.Collections.Generic;

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

	/// <summary>
	/// @author Mathieu Bastian
	/// </summary>
	public class DensityGrid : ICloneable
	{

		private const int GRID_SIZE = 1000; // size of Density grid
		private const float VIEW_SIZE = 4000; // actual physical size of layout plane
		private const int RADIUS = 10; // radius for density fall-off:
		private const int HALF_VIEW = 2000;
		private const float VIEW_TO_GRID = 0.25f;
		private float[][] density;
		private float[][] fallOff;
		private LinkedList<Node>[][] bins;

		public static float ViewSize
		{
			get
			{
				return (VIEW_SIZE * 0.8f) - (RADIUS / 0.25f) * 2f;
			}
		}

		public virtual void init()
		{
			density = RectangularArrays.RectangularFloatArray(GRID_SIZE, GRID_SIZE);
			fallOff = RectangularArrays.RectangularFloatArray(RADIUS * 2 + 1, RADIUS * 2 + 1);
			bins = RectangularArrays.RectangularLinkedListArray(GRID_SIZE, GRID_SIZE);

			for (int i = -RADIUS; i <= RADIUS; i++)
			{
				for (int j = -RADIUS; j <= RADIUS; j++)
				{
					fallOff[i + RADIUS][j + RADIUS] = ((RADIUS - Math.Abs((float) i)) / RADIUS) * ((RADIUS - Math.Abs((float) j)) / RADIUS);
				}
			}

			/*for (int i = 0; i < GRID_SIZE; i++) {
			for (int j = 0; j < GRID_SIZE; j++) {
			bins[i][j] = new ArrayDeque<Node>();
			}
			}*/
		}

		public virtual float getDensity(float nX, float nY, bool fineDensity)
		{
			int xGrid, yGrid;
			float xDist, yDist, distance, density = 0;
			int boundary = 10; // boundary around plane

			xGrid = (int)((nX + HALF_VIEW + .5) * VIEW_TO_GRID);
			yGrid = (int)((nY + HALF_VIEW + .5) * VIEW_TO_GRID);

			// Check for edges of density grid (10000 is arbitrary high density)
			if (xGrid > GRID_SIZE - boundary || xGrid < boundary)
			{
				return 10000;
			}
			if (yGrid > GRID_SIZE - boundary || yGrid < boundary)
			{
				return 10000;
			}

			if (fineDensity)
			{
				for (int i = yGrid - 1; i <= yGrid + 1; i++)
				{
					for (int j = xGrid - 1; j <= xGrid + 1; j++)
					{
						LinkedList<Node> deque = bins[i][j];
						if (deque != null)
						{
							foreach (Node bi in deque)
							{
								xDist = nX - bi.x;
								yDist = nY - bi.y;
								distance = xDist * xDist + yDist * yDist;
								density += 1e-4 / (distance + 1e-50);
							}
						}
					}
				}
			}
			else
			{
				density = this.density[yGrid][xGrid];
				density *= density;
			}
			return density;
		}

		public virtual void add(Node n, bool fineDensity)
		{
			if (fineDensity)
			{
				fineAdd(n);
			}
			else
			{
				add(n);
			}
		}

		public virtual void substract(Node n, bool firstAdd, bool fineFirstAdd, bool fineDensity)
		{
			if (fineDensity && !fineFirstAdd)
			{
				fineSubstract(n);
			}
			else if (!firstAdd)
			{
				substract(n);
			}
		}

		private void substract(Node n)
		{
			int xGrid, yGrid, diam;

			xGrid = (int)((n.subX + HALF_VIEW + 0.5f) * VIEW_TO_GRID);
			yGrid = (int)((n.subY + HALF_VIEW + 0.5f) * VIEW_TO_GRID);
			xGrid -= RADIUS;
			yGrid -= RADIUS;
			diam = 2 * RADIUS;

			for (int i = 0; i <= diam; i++)
			{
				int oldXGrid = xGrid;
				for (int j = 0; j <= diam; j++)
				{
					density[yGrid][xGrid] -= fallOff[i][j];
					xGrid++;
				}
				yGrid++;
				xGrid = oldXGrid;
			}
		}

		private void add(Node n)
		{
			int xGrid, yGrid, diam;

			xGrid = (int)((n.x + HALF_VIEW + .5) * VIEW_TO_GRID);
			yGrid = (int)((n.y + HALF_VIEW + .5) * VIEW_TO_GRID);

			n.subX = n.x;
			n.subY = n.y;

			xGrid -= RADIUS;
			yGrid -= RADIUS;
			diam = 2 * RADIUS;

			if ((xGrid + RADIUS >= GRID_SIZE) || (xGrid < 0) || (yGrid + RADIUS >= GRID_SIZE) || (yGrid < 0))
			{
				throw new Exception("Error: Exceeded density grid with " + "xGrid = " + xGrid + " and yGrid = " + yGrid);
			}

			for (int i = 0; i <= diam; i++)
			{
				int oldXGrid = xGrid;
				for (int j = 0; j <= diam; j++)
				{
					density[yGrid][xGrid] += fallOff[i][j];
					xGrid++;
				}
				yGrid++;
				xGrid = oldXGrid;
			}
		}

		private void fineSubstract(Node n)
		{
			int xGrid, yGrid;

			xGrid = (int)((n.subX + HALF_VIEW + .5) * VIEW_TO_GRID);
			yGrid = (int)((n.subY + HALF_VIEW + .5) * VIEW_TO_GRID);
			LinkedList<Node> deque = bins[yGrid][xGrid];
			if (deque != null)
			{
				deque.RemoveFirst();
			}
		}

		private void fineAdd(Node n)
		{
			int xGrid, yGrid;

			xGrid = (int)((n.x + HALF_VIEW + .5) * VIEW_TO_GRID);
			yGrid = (int)((n.y + HALF_VIEW + .5) * VIEW_TO_GRID);

			n.subX = n.x;
			n.subY = n.y;
			LinkedList<Node> deque = bins[yGrid][xGrid];
			if (deque == null)
			{
				deque = new LinkedList<Node>();
				bins[yGrid][xGrid] = deque;
			}
			deque.AddLast(n);
		}

		/*@Override
		protected DensityGrid clone() {
		DensityGrid densityGrid = new DensityGrid();
		densityGrid.fallOff = this.fallOff;
		densityGrid.density = new float[GRID_SIZE][GRID_SIZE];
		densityGrid.bins = new ArrayDeque[GRID_SIZE][GRID_SIZE];
		for (int i = 0; i < GRID_SIZE; i++) {
		System.arraycopy(this.density[i], 0, densityGrid.density[i], 0, GRID_SIZE);
		for (int j = 0; j < GRID_SIZE; j++) {
		densityGrid.bins[i][j] = bins[i][j].clone();
		}
		}
		return densityGrid;
		}*/
	}

}