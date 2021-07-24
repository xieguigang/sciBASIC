﻿Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
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

            For Each row As T In raw
                Dim d As Double() = raw _
                    .Where(Function(di) Not di Is row) _
                    .AsParallel _
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

                Yield New NamedValue(Of Double) With {
                    .Name = row.Key,
                    .Value = 1 / mean,
                    .Description = nearest _
                        .Select(Function(di) di.ToString("F2")) _
                        .JoinBy("; ")
                }
            Next
        End Function
    End Class
End Namespace