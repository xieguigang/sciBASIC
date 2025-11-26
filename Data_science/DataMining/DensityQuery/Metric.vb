#Region "Microsoft.VisualBasic::453b0a0fab09415b7343d1090b9c8344, Data_science\DataMining\DensityQuery\Metric.vb"

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

    '   Total Lines: 47
    '    Code Lines: 36 (76.60%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (23.40%)
    '     File Size: 1.52 KB


    ' Class Metric
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '     Sub: setByDimension
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Public Class Metric : Inherits KdNodeAccessor(Of ClusterEntity)

    ReadOnly dims As Dictionary(Of String, Integer)
    ReadOnly dimNames As String()

    Sub New(dims As Integer)
        Me.dims = New Dictionary(Of String, Integer)

        For i As Integer = 0 To dims - 1
            Me.dims(i.ToString) = i
        Next

        Me.dimNames = Me.dims.Keys.ToArray
    End Sub

    Public Overrides Sub setByDimension(x As ClusterEntity, dimName As String, value As Double)
        x(dims(dimName)) = value
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return dimNames
    End Function

    Public Overrides Function metric(a As ClusterEntity, b As ClusterEntity) As Double
        Return DistanceMethods.EuclideanDistance(a.entityVector, b.entityVector)
    End Function

    Public Overrides Function getByDimension(x As ClusterEntity, dimName As String) As Double
        Return x(dims(dimName))
    End Function

    Public Overrides Function nodeIs(a As ClusterEntity, b As ClusterEntity) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As ClusterEntity
        Return New ClusterEntity With {
            .cluster = -1,
            .entityVector = New Double(dims.Count - 1) {},
            .uid = "n/a"
        }
    End Function
End Class
