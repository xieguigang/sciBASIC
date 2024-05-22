#Region "Microsoft.VisualBasic::2f7257d214192e2217db3c09de280ee2, Data_science\DataMining\DensityQuery\KDQuery.vb"

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

    '   Total Lines: 43
    '    Code Lines: 36 (83.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.55 KB


    ' Class KDQuery
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: QueryDensity, Raw
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.KMeans

Public Class KDQuery : Implements IQueryDensity(Of ClusterEntity)

    ReadOnly tree As KdTree(Of ClusterEntity)
    ReadOnly raws As ClusterEntity()

    <DebuggerStepThrough>
    Sub New(raw As IEnumerable(Of ClusterEntity))
        Me.raws = raw.ToArray
        Me.tree = New KdTree(Of ClusterEntity)(Me.raws, New Metric(Me.raws.First.Length))
    End Sub

    Public Function QueryDensity(row As ClusterEntity, k As Integer) As NamedValue(Of Double) Implements IQueryDensity(Of ClusterEntity).QueryDensity
        Dim nearest As Double() = tree.nearest(row, maxNodes:=k + 1) _
            .Where(Function(p) Not p.node.data Is row) _
            .Select(Function(p) p.distance) _
            .Take(k) _
            .ToArray
        Dim mean As Double

        If nearest.Length = 0 Then
            mean = 10000
        Else
            mean = nearest.Average
        End If

        Return New NamedValue(Of Double) With {
            .Name = row.uid,
            .Value = 1 / mean,
            .Description = nearest _
                .Select(Function(di) di.ToString("F2")) _
                .JoinBy("; ")
        }
    End Function

    Public Function Raw() As IEnumerable(Of ClusterEntity) Implements IQueryDensity(Of ClusterEntity).Raw
        Return raws
    End Function
End Class
