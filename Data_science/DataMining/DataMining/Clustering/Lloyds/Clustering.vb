#Region "Microsoft.VisualBasic::7c225f87c1ecc5deb854db7520d24f3b, Data_science\DataMining\DataMining\Clustering\Lloyds\Clustering.vb"

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

    '   Total Lines: 38
    '    Code Lines: 25 (65.79%)
    ' Comment Lines: 6 (15.79%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 7 (18.42%)
    '     File Size: 1.18 KB


    '     Class Clustering
    ' 
    '         Properties: Points
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: __writeCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq

Namespace Lloyds

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/halfjew22/Clustering/blob/master/src/com/lustig/model/Clustering.java
    ''' </remarks>
    Public MustInherit Class Clustering

        Friend _source As List(Of Point)
        Friend mNumDesiredClusters As Integer

        Public Overridable ReadOnly Property Points As List(Of Point)
            Get
                Return _source
            End Get
        End Property

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            _source = source.AsList
            mNumDesiredClusters = numClusters
        End Sub

        Public MustOverride Function Clustering() As List(Of Point)

        Protected Shared Sub __writeCluster(source As IEnumerable(Of Cluster(Of Point)))
            For Each c In source.SeqIterator
                For Each x As Point In c.value.m_innerList
                    Call x.CompleteLinkageCluster(c.i)
                Next
            Next
        End Sub
    End Class
End Namespace
