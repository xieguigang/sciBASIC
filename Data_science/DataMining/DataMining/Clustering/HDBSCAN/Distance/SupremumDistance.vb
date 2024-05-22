#Region "Microsoft.VisualBasic::ac5030548b1cd5a74c1fdc468bff5f08, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Distance\SupremumDistance.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16 (76.19%)
    ' Comment Lines: 3 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (9.52%)
    '     File Size: 914 B


    '     Class SupremumDistance
    ' 
    '         Function: ComputeDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the supremum distance between two points, d = max[(x1-y1), (x2-y2), ... ,(xn-yn)].
    ''' </summary>
    Public Class SupremumDistance
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim distance As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                Dim difference = stdNum.Abs(attributesOne(i) - attributesTwo(i))
                If difference > distance Then distance = difference
                i += 1
            End While
            Return distance
        End Function
    End Class
End Namespace
