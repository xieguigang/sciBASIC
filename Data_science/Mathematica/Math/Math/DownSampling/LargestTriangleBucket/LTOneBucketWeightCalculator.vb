#Region "Microsoft.VisualBasic::662d9ca8c50867fc16f3795131e4cef3, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTOneBucketWeightCalculator.vb"

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

    '   Total Lines: 18
    '    Code Lines: 11 (61.11%)
    ' Comment Lines: 3 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 704 B


    '     Class LTOneBucketWeightCalculator
    ' 
    '         Sub: calcWeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DownSampling.LargestTriangleBucket

    ''' <summary>
    ''' Weight = Area of triangle (point A: the previous event, point B: this event; point C: the next event)
    ''' </summary>
    Public Class LTOneBucketWeightCalculator : Implements LTWeightCalculator

        Public Overridable Sub calcWeight(triangle As Triangle, buckets As IList(Of LTWeightedBucket)) Implements LTWeightCalculator.calcWeight
            For Each bucket As LTWeightedBucket In buckets
                For Each ITimeSignal As WeightedEvent In bucket
                    triangle.calc(ITimeSignal)
                Next ITimeSignal
            Next bucket
        End Sub

    End Class

End Namespace
