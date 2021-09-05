#Region "Microsoft.VisualBasic::8f3dc7f119721ca3fd7f06a83738b4bf, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanAlgorithm.vb"

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

'     Class DbscanAlgorithm
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: ComputeClusterDBSCAN, RegionQuery
' 
'         Sub: ExpandCluster
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace DBSCAN

    ''' <summary>
    ''' DBSCAN algorithm class, Density-based spatial clustering of applications with noise (DBSCAN) 
    ''' </summary>
    ''' <typeparam name="T">Takes dataset item row (features, preferences, vector) type</typeparam>
    ''' <remarks>
    ''' ###### DBSCAN Clustering Algorithm C# Implementation
    ''' 
    ''' > https://github.com/yusufuzun/dbscan
    ''' </remarks>
    Public Class DbscanAlgorithm(Of T)

        Friend ReadOnly _metricFunc As Func(Of T, T, Double)
        Friend ReadOnly _full As Boolean

        ''' <summary>
        ''' Takes metric function to compute distances between dataset items T
        ''' </summary>
        ''' <param name="metricFunc"></param>
        ''' <param name="full">
        ''' A logical option for indicates that evaluate all neighbor points 
        ''' or not for test and create cluster members
        ''' </param>
        Public Sub New(metricFunc As Func(Of T, T, Double), Optional full As Boolean = True)
            _metricFunc = metricFunc
            _full = full
        End Sub

        ''' <summary>
        ''' Performs the DBSCAN clustering algorithm.
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="epsilon">Desired region ball radius</param>
        ''' <param name="minPts">Minimum number of points to be in a region</param>
        ''' <param name="densityCut">
        ''' value in range of ``[0,1]``. a percentage location for create threshold of the density values
        ''' </param>
        ''' <returns>sets of clusters, renew the parameter</returns>
        ''' 
        Public Function ComputeClusterDBSCAN(allPoints As T(),
                                             epsilon As Double,
                                             minPts As Integer,
                                             Optional ByRef is_seed As Integer() = Nothing,
                                             Optional filterNoise As Boolean = True,
                                             Optional densityCut As Double = -1) As NamedCollection(Of T)()

            Dim allPointsDbscan As DbscanPoint(Of T)() = allPoints _
                .Select(Function(x, i)
                            Return New DbscanPoint(Of T)(x) With {
                                .ID = (i + 1).ToHexString
                            }
                        End Function) _
                .ToArray()
            Dim session As DbscanSession(Of T) = New DbscanSession(Of T)(
                dbscan:=Me,
                allPoints:=allPointsDbscan,
                epsilon:=epsilon,
                minPts:=minPts
            ).LoadDensityVector(densityCut)

            is_seed = IteratesAllPoints(session)

            With allPointsDbscan _
                .Where(Function(x)
                           If Not filterNoise Then
                               Return True
                           Else
                               Return x.ClusterId > 0
                           End If
                       End Function) _
                .GroupBy(Function(x)
                             Return x.ClusterId
                         End Function)

                Return .Select(Function(x)
                                   Return New NamedCollection(Of T) With {
                                       .name = x.Key,
                                       .value = x _
                                           .Select(Function(y) y.ClusterPoint) _
                                           .ToArray()
                                   }
                               End Function) _
                       .ToArray
            End With
        End Function

        Private Function IteratesAllPoints(session As DbscanSession(Of T)) As Integer()
            Dim clusterId As Integer = 0
            Dim seeds As New List(Of Integer)
            Dim j As i32 = 0
            Dim size As Integer = session.allPoints.Length
            Dim d As Integer = size / 20

            For i As Integer = 0 To size - 1
                Dim p As DbscanPoint(Of T) = session.allPoints(i)

                If ++j = d Then
                    j = 0
                    Call Console.WriteLine($" [{i}/{size}] query {p.ID}...{CInt(100 * i / size)}%")
                End If

                If p.IsVisited AndAlso Not (p.ClusterId = ClusterIDs.Unclassified OrElse p.ClusterId = ClusterIDs.Noise) Then
                    Continue For
                Else
                    p.IsVisited = True
                End If

                If session.isNoise(p.ID) Then
                    p.ClusterId = ClusterIDs.Noise
                    Continue For
                End If

                Dim neighborPts As DbscanPoint(Of T)() = session.RegionQuery(p.ClusterPoint)

                If neighborPts.Length < session.minPts Then
                    p.ClusterId = ClusterIDs.Noise
                Else
                    clusterId += 1
                    ' point to be in a cluster
                    p.ClusterId = clusterId

                    Call seeds.Add(i)
                    Call session.ExpandClusterParallel(neighborPts, clusterId, stackDepth:=0)
                End If
            Next

            Return seeds
        End Function
    End Class
End Namespace
