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

namespace org.gephi.layout.plugin
{
	using GraphModel = org.gephi.graph.api.GraphModel;
	using Layout = org.gephi.layout.spi.Layout;
	using LayoutProperty = org.gephi.layout.spi.LayoutProperty;
	using Property = org.openide.nodes.Node.Property;
	using Exceptions = org.openide.util.Exceptions;

	/// <summary>
	/// Class to build layout scenario that runs for a certain duration. Multiple
	/// layout can be chained and their duration ratio set. Moreover layout
	/// property can be mananaged automatically and set in advance.
	/// <para>
	/// <b>Example:</b>
	/// </para>
	/// <para>
	/// This will execute ForceAtlas for the first 80%, and LabelAdjust for remaining 20%
	/// <pre>
	/// AutoLayout autoLayout = new AutoLayout(10, TimeUnit.SECONDS);
	/// ForceAtlasLayout forceAtlasLayout = new ForceAtlasLayout(null);
	/// AutoLayout.DynamicProperty gravity = AutoLayout.createDynamicProperty("Gravity", new Double[]{80., 400.0}, new float[]{0f, 1f}, AutoLayout.Interpolation.LINEAR);
	/// AutoLayout.DynamicProperty speed = AutoLayout.createDynamicProperty("Speed", new Double[]{1.2, 0.3}, new float[]{0f, 1f}, AutoLayout.Interpolation.LINEAR);
	/// AutoLayout.DynamicProperty repulsion = AutoLayout.createDynamicProperty("Repulsion strength", new Double[]{3000.0, 6000.}, new float[]{0f, 1f}, AutoLayout.Interpolation.LINEAR);
	/// AutoLayout.DynamicProperty freeze = AutoLayout.createDynamicProperty("Autostab Strength", new Double(100.0), 0f);
	/// autoLayout.addLayout(forceAtlasLayout, 0.8f, new AutoLayout.DynamicProperty[]{gravity, speed, repulsion, freeze});
	/// 
	/// //LabelAdjust
	/// LabelAdjust labelAdjust = new LabelAdjust(null);
	/// AutoLayout.DynamicProperty speed2 = AutoLayout.createDynamicProperty("Speed", new Double[]{0.5, 0.2}, new float[]{0f, 1f}, AutoLayout.Interpolation.LINEAR);
	/// autoLayout.addLayout(labelAdjust, 0.2f, new AutoLayout.DynamicProperty[]{speed2});
	/// </pre>
	/// Work in Progress
	/// 
	/// @author Mathieu Bastian
	/// </para>
	/// </summary>
	public class AutoLayout
	{

		private readonly float duration;
		private readonly IList<LayoutScenario> layouts;
		private GraphModel graphModel;
		//Flags
		private long startTime = 0;
		private long lastExecutionTime;
		private float currentRatio;
		private int innerIteration;
		private float innerStart;
		private float innerRatio;
		private LayoutScenario currentLayout;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool cancel_Conflict;

		public AutoLayout(long duration, TimeUnit timeUnit)
		{
			this.duration = TimeUnit.MILLISECONDS.convert(duration, timeUnit);
			this.layouts = new List<LayoutScenario>();
		}

		public static DynamicProperty createDynamicProperty(string propertyName, object value, float ratio)
		{
			return new SingleDynamicProperty(propertyName, value, ratio);
		}

		public static DynamicProperty createDynamicProperty(string propertyName, object[] value, float[] ratio)
		{
			return new MultiDynamicProperty(propertyName, value, ratio);
		}

		public static DynamicProperty createDynamicProperty(string propertyName, Number[] value, float[] ratio, Interpolation interpolation)
		{
			return new InterpolateDynamicProperty(propertyName, value, ratio, interpolation);
		}

		public virtual void addLayout(Layout layout, float ratio)
		{
			layouts.Add(new LayoutScenario(layout, ratio));
		}

		public virtual void addLayout(Layout layout, float ratio, DynamicProperty[] properties)
		{
			for (int i = 0; i < properties.Length; i++)
			{
				AbstractDynamicProperty property = (AbstractDynamicProperty) properties[i];
				foreach (LayoutProperty lp in layout.Properties)
				{
					if (lp.CanonicalName.equalsIgnoreCase(property.CanonicalName))
					{
						property.Property = lp.Property;
						break;
					}
				}
				if (property.Property == null)
				{
					throw new System.ArgumentException(property.CanonicalName + " property cannot be found in layout");
				}
			}
			layouts.Add(new LayoutScenario(layout, ratio, properties));
		}

		public virtual void execute()
		{
			//System.out.println("execute");
			cancel_Conflict = false;
			verifiy();
			LayoutScenario layout;
			while (!cancel_Conflict && (layout = setLayout()) != null)
			{
				setProperties();
				layout.layout.goAlgo();
			}
			//System.out.println("finished");
		}

		public virtual void cancel()
		{
			cancel_Conflict = true;
		}

		private void setProperties()
		{
			//String log = currentLayout.layout.toString() + ": ";
			for (int i = 0; i < currentLayout.properties.Length; i++)
			{
				DynamicProperty d = currentLayout.properties[i];
				object val = d.getValue(innerRatio);
				if (val != null)
				{
					try
					{
						if (val != d.Property.Value)
						{
							//log += d.getProperty().getDisplayName() + "=" + val+"   ";
							d.Property.Value = val;
						}
					}
					catch (Exception ex)
					{
						Exceptions.printStackTrace(ex);
					}
				}
			}
			//System.out.println(log);
		}

		private LayoutScenario setLayout()
		{
			long elapsedTime = ElapsedTime;
			long diff = elapsedTime - lastExecutionTime;
			//System.out.println(diff);
			lastExecutionTime = elapsedTime;
			currentRatio = elapsedTime / duration;
			float ratio = currentRatio + (diff / duration); //Don't start a layout that will overcome ratio
			if (ratio >= 1f)
			{
				currentLayout = null;
				return currentLayout;
			}
			LayoutScenario layout = null;
			float sumRatio = 0;
			float sum = 0;
			for (int i = 0; i < layouts.Count; i++)
			{
				LayoutScenario l = layouts[i];
				if (sum <= ratio)
				{
					layout = l;
					sumRatio = sum;
				}
				sum += l.ratio;
			}
			if (currentLayout != layout)
			{
				innerStart = currentRatio;
				innerIteration = 0;
				layout.layout.GraphModel = graphModel;
				layout.layout.resetPropertiesValues();
				layout.layout.initAlgo();
			}
			else
			{
				innerIteration++;
			}
			currentLayout = layout;

			float start = innerStart;
			float end = sumRatio + layout.ratio;
			float averageIteration = innerIteration == 0 ? 0 : (currentRatio - start) / innerIteration;
			int totalIteration = averageIteration == 0 ? 0 : (int)((end - start) / averageIteration) - 1;
			innerRatio = totalIteration == 0 ? 0 : innerIteration / (float) totalIteration;

			return currentLayout;
		}

		private long ElapsedTime
		{
			get
			{
				if (startTime == 0)
				{
					startTime = DateTimeHelper.CurrentUnixTimeMillis();
					return 0;
				}
				return DateTimeHelper.CurrentUnixTimeMillis() - startTime;
			}
		}

		public virtual GraphModel GraphModel
		{
			set
			{
				this.graphModel = value;
			}
		}

		private void verifiy()
		{
			float sum = 0;
			foreach (LayoutScenario l in layouts)
			{
				sum += l.ratio;
			}
			if (sum != 1)
			{
				throw new Exception("Ratio sum is not 1");
			}
		}

		public enum Interpolation
		{

			LINEAR,
			LOG
		}

		public interface DynamicProperty
		{

			object getValue(float ratio);

			Property Property {get;}

			string CanonicalName {get;}
		}

		private abstract class AbstractDynamicProperty : DynamicProperty
		{
			public abstract object getValue(float ratio);

			internal readonly string propertyCanonicalName;
			protected internal Property property;

			public AbstractDynamicProperty(string propertyName)
			{
				this.propertyCanonicalName = propertyName;
			}

			public virtual Property Property
			{
				get
				{
					return property;
				}
				set
				{
					this.property = value;
				}
			}


			public virtual string CanonicalName
			{
				get
				{
					return propertyCanonicalName;
				}
			}
		}

		private class SingleDynamicProperty : AbstractDynamicProperty
		{

			internal readonly object value;
			internal readonly float threshold;

			internal SingleDynamicProperty(string propertyName, object value, float ratio) : base(propertyName)
			{
				this.value = value;
				this.threshold = ratio;
			}

			public override object getValue(float ratio)
			{
				try
				{
					if (ratio >= threshold)
					{
						return value;
					}
					return property.Value;
				}
				catch (Exception e)
				{
					Exceptions.printStackTrace(e);
				}
				return null;
			}
		}

		private class MultiDynamicProperty : AbstractDynamicProperty
		{

			internal readonly object[] value;
			internal readonly float[] thresholds;
			internal int currentIndex = 0;

			internal MultiDynamicProperty(string propertyName, object[] value, float[] ratio) : base(propertyName)
			{
				this.value = value;
				this.thresholds = ratio;
				if (value.Length != ratio.Length)
				{
					throw new System.ArgumentException("Value and ratio arrays must have same length");
				}
			}

			public override object getValue(float ratio)
			{
				while (currentIndex < thresholds.Length && thresholds[currentIndex] < ratio)
				{
					currentIndex++;
				}
				return value[currentIndex];
			}
		}

		private class InterpolateDynamicProperty : AbstractDynamicProperty
		{

			internal readonly Number[] value;
			internal readonly float[] thresholds;
			internal readonly Interpolation interpolation;
			internal int currentIndex = 0;

			internal InterpolateDynamicProperty(string propertyName, Number[] value, float[] ratio, Interpolation interpolation) : base(propertyName)
			{
				this.value = value;
				this.thresholds = ratio;
				this.interpolation = interpolation;
				if (value.Length != ratio.Length)
				{
					throw new System.ArgumentException("Value and ratio arrays must have same length");
				}
			}

			public override object getValue(float ratio)
			{
				while (currentIndex < thresholds.Length && thresholds[currentIndex] < ratio)
				{
					currentIndex++;
				}
				if (currentIndex > 0)
				{
					float r = 1 / (thresholds[currentIndex] - thresholds[currentIndex - 1]);
					ratio = ((ratio - thresholds[currentIndex - 1]) * r);
					return value[currentIndex - 1].doubleValue() + (value[currentIndex].doubleValue() - value[currentIndex - 1].doubleValue()) * ratio;
				}
				return value[currentIndex];
			}
		}

		private class LayoutScenario
		{

			internal readonly Layout layout;
			internal readonly float ratio;
			internal readonly DynamicProperty[] properties;

			public LayoutScenario(Layout layout, float ratio, DynamicProperty[] properties)
			{
				this.layout = layout;
				this.ratio = ratio;
				this.properties = properties;
			}

			public LayoutScenario(Layout layout, float ratio) : this(layout, ratio, new DynamicProperty[0])
			{
			}
		}
	}

}