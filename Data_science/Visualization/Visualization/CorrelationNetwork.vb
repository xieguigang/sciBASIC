#Region "Microsoft.VisualBasic::0e95d9a5015c4d93d66074350c8cf5e6, Data_science\Visualization\Visualization\CorrelationNetwork.vb"

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

    '   Total Lines: 121
    '    Code Lines: 91 (75.21%)
    ' Comment Lines: 11 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (15.70%)
    '     File Size: 4.37 KB


    ' Module CorrelationNetwork
    ' 
    '     Function: (+3 Overloads) BuildNetwork, HowStrong
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.DataFrame
Imports std = System.Math

''' <summary>
''' 使用这个模块用来生成相关度的网络，相关度网络是``Kmeans``，``Cmeans``或者其他的一些聚类网络可视化的基础
''' </summary>
Public Module CorrelationNetwork

    <Extension>
    Public Function BuildNetwork(data As IEnumerable(Of DataSet), cutoff#, Optional pvalue As Double = 1) As (net As NetworkGraph, matrix As CorrelationMatrix)
        Return data.Correlation(False).BuildNetwork(cutoff, pvalue)
    End Function

    <Extension>
    Public Function BuildNetwork(matrix As DistanceMatrix, cutoff As Double) As NetworkGraph
        If matrix.is_dist Then
            Throw New InvalidConstraintException("the given input matrix should be a similarity matrix!")
        End If

        Dim g As New NetworkGraph
        Dim cor As Double
        Dim nodeData As NodeData
        Dim linkdata As EdgeData

        For Each id As String In matrix.keys
            nodeData = New NodeData With {.origID = id, .label = id}
            g.CreateNode(id, nodeData)
        Next

        Dim uid As String

        For Each id As String In matrix.keys
            For Each partner As String In matrix.keys.Where(Function(b) b <> id)
                cor = matrix(id, partner)

                If std.Abs(cor) > cutoff Then
                    uid$ = {partner, id}.OrderBy(Function(s) s).JoinBy(" - ")
                    linkdata = New EdgeData With {
                        .label = uid,
                        .length = cor,
                        .Properties = New Dictionary(Of String, String) From {
                            {NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, HowStrong(cor)}
                        }
                    }

                    g.CreateEdge(id, partner, cor, linkdata)
                End If
            Next
        Next

        Return g
    End Function

    ''' <summary>
    ''' 关联网络是没有方向的
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <param name="cutoff">
    ''' 相关度阈值的绝对值
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function BuildNetwork(matrix As CorrelationMatrix, cutoff#, Optional pvalue As Double = 1) As (net As NetworkGraph, matrix As CorrelationMatrix)
        Dim g As New NetworkGraph
        Dim cor As Double
        Dim nodeData As NodeData
        Dim linkdata As EdgeData

        For Each id As String In matrix.keys
            nodeData = New NodeData With {.origID = id, .label = id}
            g.CreateNode(id, nodeData)
        Next

        Dim uid As String
        Dim prob As Double

        For Each id As String In matrix.keys
            For Each partner As String In matrix.keys.Where(Function(b) b <> id)
                cor = matrix(id, partner)
                prob = matrix.pvalue(id, partner)

                If std.Abs(cor) > cutoff AndAlso prob < pvalue Then
                    uid$ = {partner, id}.OrderBy(Function(s) s).JoinBy(" - ")
                    linkdata = New EdgeData With {
                        .label = uid,
                        .length = cor,
                        .Properties = New Dictionary(Of String, String) From {
                            {NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, HowStrong(cor)},
                            {"pvalue", prob}
                        }
                    }

                    g.CreateEdge(id, partner, cor, linkdata)
                End If
            Next
        Next

        Return (g, matrix)
    End Function

    Private Function HowStrong(c#) As String
        Dim abs = std.Abs(c)

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
