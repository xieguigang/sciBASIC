#Region "Microsoft.VisualBasic::56c64f0e80158ea6bb7cdadef2bded5c, Data_science\DataMining\DataMining\Clustering\Density.vb"

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

    '   Total Lines: 117
    '    Code Lines: 70 (59.83%)
    ' Comment Lines: 29 (24.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (15.38%)
    '     File Size: 4.97 KB


    '     Class Density
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: dist, (+3 Overloads) GetDensity
    '         Class DensityDelegate
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: IQueryDensity_Raw, QueryDensity
    ' 
    ' 
    ' 
    '     Interface IQueryDensity
    ' 
    '         Function: QueryDensity, Raw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations

Namespace Clustering

    ''' <summary>
    ''' evaluate point density
    ''' </summary>
    Public NotInheritable Class Density

        Private Sub New()
        End Sub

        Private Class DensityDelegate(Of T As {Class, INamedValue}) : Implements IQueryDensity(Of T)

            Friend ReadOnly raw As T()
            Friend ReadOnly metric As Func(Of T, T, Double)

            Sub New(raw As IEnumerable(Of T), metric As Func(Of T, T, Double))
                Me.raw = raw.ToArray
                Me.metric = metric
            End Sub

            Public Function QueryDensity(row As T, k As Integer) As NamedValue(Of Double) Implements IQueryDensity(Of T).QueryDensity
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

            Private Function IQueryDensity_Raw() As IEnumerable(Of T) Implements IQueryDensity(Of T).Raw
                Return raw
            End Function
        End Class

        ''' <summary>
        ''' 密度为到k个最近邻的平均距离的倒数。如果该距离小，则密度高
        ''' </summary>
        ''' <param name="dataset"></param>
        ''' <param name="k"></param>
        ''' <returns>
        ''' a collection of tuple [id -> density].
        ''' the larger of the result value, the higher density value it is
        ''' </returns>
        Public Shared Function GetDensity(dataset As IEnumerable(Of ClusterEntity), Optional k As Integer = 6, Optional query As IQueryDensity(Of ClusterEntity) = Nothing) As IEnumerable(Of NamedValue(Of Double))
            Return GetDensity(If(query, New DensityDelegate(Of ClusterEntity)(dataset, AddressOf dist)), k)
        End Function

        Private Shared Function dist(x As ClusterEntity, y As ClusterEntity) As Double
            Return DistanceMethods.EuclideanDistance(x.entityVector, y.entityVector)
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
        Public Shared Function GetDensity(Of T As {Class, INamedValue})(dataset As IEnumerable(Of T), metric As Func(Of T, T, Double), Optional k As Integer = 6) As IEnumerable(Of NamedValue(Of Double))
            Return GetDensity(New DensityDelegate(Of T)(dataset, metric), k)
        End Function

        ''' <summary>
        ''' 密度为到k个最近邻的平均距离的倒数。如果该距离小，则密度高
        ''' </summary>
        ''' <param name="k"></param>
        ''' <returns>
        ''' a collection of tuple [id -> density].
        ''' the larger of the result value, the higher density value it is
        ''' </returns>
        Public Shared Iterator Function GetDensity(Of T As {Class, INamedValue})(handler As IQueryDensity(Of T), Optional k As Integer = 6) As IEnumerable(Of NamedValue(Of Double))
            Dim query = From row As SeqValue(Of T)
                        In handler _
                            .Raw _
                            .SeqIterator _
                            .AsParallel
                        Select row.i, dist = handler.QueryDensity(row.value, k)
                        Order By i Ascending

            For Each rowQuery In query
                Yield rowQuery.dist
            Next
        End Function
    End Class

    Public Interface IQueryDensity(Of T As {Class, INamedValue})

        Function Raw() As IEnumerable(Of T)
        Function QueryDensity(row As T, k As Integer) As NamedValue(Of Double)

    End Interface
End Namespace
