#Region "Microsoft.VisualBasic::4fbcdb606f58aee6aec4651e51bdab8b, Data_science\DataMining\DataMining\Clustering\KMeans\EvaluationScore.vb"

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

    '   Total Lines: 91
    '    Code Lines: 70
    ' Comment Lines: 8
    '   Blank Lines: 13
    '     File Size: 4.26 KB


    '     Class EvaluationScore
    ' 
    '         Properties: calinskiHarabasz, clusters, davidBouldin, dunn, maximumDiameter
    '                     num_class, silhouette
    ' 
    '         Function: CalinskiHarabaszs, CreateClusters, Evaluate, SilhouetteCoeff, SingleScore
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class EvaluationScore

        Public Property silhouette As Double
        Public Property dunn As Double
        Public Property davidBouldin As Double
        Public Property calinskiHarabasz As Double
        Public Property maximumDiameter As Double
        Public Property clusters As Dictionary(Of String, String())

        Public ReadOnly Property num_class As Integer
            Get
                Return clusters.Count
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[classes:{num_class}, silhouette: {silhouette}, dunn: {dunn}, davidBouldin: {davidBouldin}, calinskiHarabasz: {calinskiHarabasz}, maximumDiameter: {maximumDiameter}]" & clusters.GetJson
        End Function

        Public Shared Function Evaluate(data As IEnumerable(Of ClusterEntity), Optional fast As Boolean = False) As EvaluationScore
            Dim class_groups As Bisecting.Cluster() = CreateClusters(data).ToArray
            Dim dunn = Evaluation.Dunn(class_groups)
            Dim silhouette = Evaluation.Silhouette(class_groups)
            Dim davidBouldin = Evaluation.calcularDavidBouldin(class_groups)
            Dim calinskiHarabasz = Evaluation.CalinskiHarabasz(class_groups)
            Dim maximumDiameter = Evaluation.CalcularMaximumDiameter(class_groups)

            Return New EvaluationScore With {
                .clusters = class_groups _
                    .ToDictionary(Function(c) c.Cluster.ToString,
                                  Function(c)
                                      Return c.Keys.ToArray
                                  End Function),
                .dunn = dunn,
                .silhouette = silhouette,
                .calinskiHarabasz = calinskiHarabasz,
                .davidBouldin = davidBouldin,
                .maximumDiameter = maximumDiameter
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">
        ''' all of the data points inside this given collection should 
        ''' be tagged with the <see cref="ClusterEntity.cluster"/> labels.
        ''' </param>
        ''' <returns></returns>
        Public Shared Iterator Function CreateClusters(data As IEnumerable(Of ClusterEntity)) As IEnumerable(Of Bisecting.Cluster)
            Dim class_groups = data _
                .GroupBy(Function(c) c.cluster) _
                .Select(Function(c) c.ToArray) _
                .ToArray

            For Each group As ClusterEntity() In class_groups
                Yield New Bisecting.Cluster(group.CalculateClusterMean, group) With {
                    .Cluster = group.First
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SilhouetteCoeff(data As ClusterEntity(), traceback As TraceBackIterator) As IEnumerable(Of PointF)
            Return SingleScore(data, traceback, AddressOf Evaluation.Silhouette)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CalinskiHarabaszs(data As ClusterEntity(), traceback As TraceBackIterator) As IEnumerable(Of PointF)
            Return SingleScore(data, traceback, AddressOf Evaluation.CalinskiHarabasz)
        End Function

        Private Shared Iterator Function SingleScore(data As ClusterEntity(), traceback As TraceBackIterator, eval As Func(Of Bisecting.Cluster(), Double)) As IEnumerable(Of PointF)
            For Each i As Integer In Tqdm.Wrap(Enumerable.Range(1, traceback.size - 1).ToArray, useColor:=True)
                traceback.SetTraceback(data, itr:=i)
                Yield New PointF(i, eval(CreateClusters(data).ToArray))
            Next
        End Function

    End Class
End Namespace
