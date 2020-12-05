#Region "Microsoft.VisualBasic::e07fc09acb483d849b0bd5155e2f15fc, Data_science\Visualization\Visualization\CorrelationNetwork.vb"

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

    ' Module CorrelationNetwork
    ' 
    '     Function: (+2 Overloads) BuildNetwork, HowStrong
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.DataFrame
Imports stdNum = System.Math

''' <summary>
''' 使用这个模块用来生成相关度的网络，相关度网络是``Kmeans``，``Cmeans``或者其他的一些聚类网络可视化的基础
''' </summary>
Public Module CorrelationNetwork

    <Extension>
    Public Function BuildNetwork(data As IEnumerable(Of DataSet), cutoff#) As (net As NetworkTables, matrix As DistanceMatrix)
        Return data.MatrixBuilder(AddressOf Correlations.GetPearson, False).BuildNetwork(cutoff)
    End Function

    ''' <summary>
    ''' 关联网络是没有方向的
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <param name="cutoff"></param>
    ''' <returns></returns>
    <Extension>
    Public Function BuildNetwork(matrix As DistanceMatrix, cutoff#) As (net As NetworkTables, matrix As DistanceMatrix)
        Dim nodes As New Dictionary(Of Node)
        Dim edges As New Dictionary(Of String, NetworkEdge)
        Dim cor As Double

        For Each id As String In matrix.keys
            nodes += New Node With {
                .ID = id
            }
        Next

        For Each id As String In matrix.keys
            For Each partner As String In matrix.keys
                cor = matrix(id, partner)

                If stdNum.Abs(cor) >= cutoff Then
                    Dim uid$ = {partner, id}.OrderBy(Function(s) s).JoinBy(" - ")

                    If Not edges.ContainsKey(uid) Then
                        edges(uid) = New NetworkEdge With {
                            .fromNode = id,
                            .toNode = partner,
                            .value = cor,
                            .interaction = HowStrong(cor)
                        }
                    End If
                End If
            Next
        Next

        Dim net As New NetworkTables(nodes.Values, edges.Values)

        Return (net, matrix)
    End Function

    Private Function HowStrong(c#) As String
        Dim abs = stdNum.Abs(c)

        If abs < 0.4 Then
            Return "Very Weak"
        ElseIf abs < 0.5 Then
            Return "Weak"
        ElseIf abs < 0.7 Then
            Return "Medium"
        ElseIf abs < 0.9 Then
            Return "Strong"
        Else
            Return "Very Strong"
        End If
    End Function
End Module
