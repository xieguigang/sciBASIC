#Region "Microsoft.VisualBasic::f5706ef9fabe0406b0aef4a6e95a475b, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\Clustering.vb"

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

    '   Total Lines: 37
    '    Code Lines: 24
    ' Comment Lines: 6
    '   Blank Lines: 7
    '     File Size: 1.15 KB


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

Imports Microsoft.VisualBasic.Linq

Namespace KMeans.CompleteLinkage

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/halfjew22/Clustering/blob/master/src/com/lustig/model/Clustering.java
    ''' </remarks>
    Public MustInherit Class Clustering

        Friend _source As List(Of Point)
        Friend mNumDesiredClusters As Integer

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            _source = source.AsList
            mNumDesiredClusters = numClusters
        End Sub

        Public MustOverride Function Clustering() As List(Of Point)

        Public Overridable ReadOnly Property Points As List(Of Point)
            Get
                Return _source
            End Get
        End Property

        Protected Shared Sub __writeCluster(source As IEnumerable(Of Cluster(Of Point)))
            For Each c In source.SeqIterator
                For Each x As Point In c.value._innerList
                    Call x.CompleteLinkageCluster(c.i)
                Next
            Next
        End Sub
    End Class
End Namespace
