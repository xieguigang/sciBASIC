using System;

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

namespace org.gephi.layout.plugin.force
{
	using LayoutData = org.gephi.graph.spi.LayoutData;

	/// <summary>
	/// @author Helder Suzuki
	/// </summary>
	public class ForceVector : LayoutData
	{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal float x_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal float y_Conflict;

		public ForceVector(ForceVector vector)
		{
			this.x_Conflict = vector.x();
			this.y_Conflict = vector.y();
		}

		public ForceVector(float x, float y)
		{
			this.x_Conflict = x;
			this.y_Conflict = y;
		}

		public ForceVector()
		{
			this.x_Conflict = 0;
			this.y_Conflict = 0;
		}

		public virtual float x()
		{
			return x_Conflict;
		}

		public virtual float y()
		{
			return y_Conflict;
		}

		public virtual float z()
		{
			throw new System.NotSupportedException("Not supported yet.");
		}

		public virtual float X
		{
			set
			{
				this.x_Conflict = value;
			}
		}

		public virtual float Y
		{
			set
			{
				this.y_Conflict = value;
			}
		}

		public virtual void add(ForceVector f)
		{
			if (f != null)
			{
				x_Conflict += f.x();
				y_Conflict += f.y();
			}
		}

		public virtual void multiply(float s)
		{
			x_Conflict *= s;
			y_Conflict *= s;
		}

		public virtual void subtract(ForceVector f)
		{
			if (f != null)
			{
				x_Conflict -= f.x();
				y_Conflict -= f.y();
			}
		}

		public virtual float Energy
		{
			get
			{
				return x_Conflict * x_Conflict + y_Conflict * y_Conflict;
			}
		}

		public virtual float Norm
		{
			get
			{
				return (float) Math.Sqrt(Energy);
			}
		}

		public virtual ForceVector normalize()
		{
			float norm = Norm;
			return new ForceVector(x_Conflict / norm, y_Conflict / norm);
		}

		public override string ToString()
		{
			return "(" + x_Conflict + ", " + y_Conflict + ")";
		}
	}

}