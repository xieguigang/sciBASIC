#Region "Microsoft.VisualBasic::de0a267cdaffb2a3ac16224f44c1b3d1, Data_science\Graph\Model\Tree\KdTree\KdUtils.vb"

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

    '   Total Lines: 48
    '    Code Lines: 28 (58.33%)
    ' Comment Lines: 12 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (16.67%)
    '     File Size: 1.90 KB


    '     Module KdUtils
    ' 
    '         Function: AverageDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace KdTree

    Public Module KdUtils

        ''' <summary>
        ''' take sampling of some nodes and then evaluated the near neighbor distance
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="kdtree"></param>
        ''' <param name="sample">
        ''' number of the random samples 
        ''' </param>
        ''' <param name="k">
        ''' k for knn search based on the kd-tree
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function AverageDistance(Of T As New)(kdtree As KdTree(Of T), sample As Integer,
                                                     Optional k As Integer = 32,
                                                     <Out>
                                                     Optional ByRef sampleDist As Double() = Nothing) As Double

            Dim dist As Double() = New Double(sample - 1) {}
            Dim sampledata As T() = kdtree.GetPointSample(sample).ToArray

            Call System.Threading.Tasks.Parallel.For(0, sample,
                 Sub(i)
                     Dim knn = kdtree.nearest(sampledata(i), k).ToArray
                     Dim mean As Double = Aggregate x As KdNodeHeapItem(Of T)
                                          In knn
                                          Order By x.distance
                                          Take CInt(k / 2)
                                          Let d = x.distance
                                          Into Average(d) ' evaluate average distance inside this knn dataset
                     dist(i) = mean
                 End Sub)

            sampleDist = dist

            Return dist.Average
        End Function

    End Module
End Namespace
