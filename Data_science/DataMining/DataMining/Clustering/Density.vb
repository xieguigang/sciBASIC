#Region "Microsoft.VisualBasic::a0688dbbe0ff7624012dd803b309de7f, Data_science\DataMining\DataMining\Clustering\Density.vb"

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

    '     Class Density
    ' 
    '         Function: (+2 Overloads) GetDensity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Namespace Clustering

    ''' <summary>
    ''' evaluate point density
    ''' </summary>
    Public Class Density

        ''' <summary>
        ''' 密度为到k个最近邻的平均距离的倒数。如果该距离小，则密度高
        ''' </summary>
        ''' <param name="dataset"></param>
        ''' <param name="k"></param>
        ''' <returns>
        ''' a collection of tuple [id -> density].
        ''' the larger of the result value, the higher density value it is
        ''' </returns>
        Public Shared Function GetDensity(dataset As IEnumerable(Of ClusterEntity), Optional k As Integer = 6) As IEnumerable(Of NamedValue(Of Double))
            Return GetDensity(dataset, Function(x, y) DistanceMethods.EuclideanDistance(x.entityVector, y.entityVector), k)
        End Function

        ''' <summary>
        ''' 密度为到k个最近邻的平均距离的倒数。如果该距离小，则密度高
        ''' </summary>
        ''' <param name="dataset"></param>
        ''' <param name="k"></param>
        ''' <returns>
        ''' a collection of tuple [id -> density].
        ''' the larger of the result value, the higher density value it is
        ''' </returns>
        Public Shared Iterator Function GetDensity(Of T As {Class, INamedValue})(dataset As IEnumerable(Of T), metric As Func(Of T, T, Double), Optional k As Integer = 6) As IEnumerable(Of NamedValue(Of Double))
            Dim raw = dataset.ToArray
            Dim query = From row As T
                        In raw.AsParallel
                        Select QueryDensity(row, raw, metric, k)

            For Each rowQuery As NamedValue(Of Double) In query
                Yield rowQuery
            Next
        End Function

        Private Shared Function QueryDensity(Of T As {Class, INamedValue})(row As T, raw As T(), metric As Func(Of T, T, Double), k As Integer) As NamedValue(Of Double)
            Dim d As Double() = raw _
                .Where(Function(di) Not di Is row) _
                .Select(Function(r)
                            Return metric(r, row)
                        End Function) _
                .OrderBy(Function(di) di) _
                .ToArray
            Dim nearest As Double() = d.Take(k).ToArray
            Dim mean As Double

            If nearest.Length = 0 Then
                mean = 10000
            Else
                mean = nearest.Average
            End If

            Return New NamedValue(Of Double) With {
                .Name = row.Key,
                .Value = 1 / mean,
                .Description = nearest _
                    .Select(Function(di) di.ToString("F2")) _
                    .JoinBy("; ")
            }
        End Function
    End Class
End Namespace
