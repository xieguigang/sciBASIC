#Region "Microsoft.VisualBasic::17fd1f67edd1e058d8e9bcc29df89dae, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTThreeBucketWeightCalculator.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 24
    '    Code Lines: 16 (66.67%)
    ' Comment Lines: 3 (12.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 851 B


    ' 	Class LTThreeBucketWeightCalculator
    ' 
    ' 	    Sub: calcWeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DownSampling.LargestTriangleBucket


	''' <summary>
	''' Weight = Area of triangle (point A: the previous selected event, point B: this event; point C: average event int the next bucket)
	''' </summary>
	Public Class LTThreeBucketWeightCalculator
		Implements LTWeightCalculator

		Public Overridable Sub calcWeight(triangle As Triangle, buckets As IList(Of LTWeightedBucket)) Implements LTWeightCalculator.calcWeight
			For i As Integer = 1 To buckets.Count - 2
				Dim bucket As LTWeightedBucket = buckets(i)
				Dim last As WeightedEvent = buckets(i - 1).select()(0)
				Dim [next] As WeightedEvent = buckets(i + 1).average()
				For j As Integer = 0 To bucket.size() - 1
					Dim curr As WeightedEvent = bucket.get(j)
					triangle.calc(last, curr, [next])
				Next j
			Next i
		End Sub

	End Class

End Namespace
