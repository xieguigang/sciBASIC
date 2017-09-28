#Region "Microsoft.VisualBasic::af96fe08325a8c2cc3ebc46d881ee0f7, ..\sciBASIC#\Data_science\DataMining\network\CorrelationNetwork.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Math.Correlations.Correlations

''' <summary>
''' 使用这个模块用来生成相关度的网络，相关度网络是``Kmeans``，``Cmeans``或者其他的一些聚类网络可视化的基础
''' </summary>
Public Module CorrelationNetwork

    <Extension>
    Public Function CorrelationMatrix(data As IEnumerable(Of DataSet), Optional compute As ICorrelation = Nothing) As DataSet()
        Dim vectors As NamedValue(Of Double())() = data _
            .NamedMatrix _
            .Select(Function(x)
                        Return New NamedValue(Of Double()) With {
                            .Name = x.Name,
                            .Value = x.Value.Values.ToArray
                        }
                    End Function) _
            .ToArray
        Dim matrix = vectors _
            .CorrelationMatrix(compute) _
            .AsDataSet _
            .ToArray

        Return matrix
    End Function

    <Extension>
    Public Function BuildNetwork(data As IEnumerable(Of DataSet), cutoff#) As NetworkTables
        Dim matrix As DataSet() = data.CorrelationMatrix.ToArray
        Dim nodes As New Dictionary(Of Node)
        Dim edges As New Dictionary(Of String, NetworkEdge) ' 关联网络是没有方向的

        For Each row As DataSet In matrix
            nodes += New Node With {
                .ID = row.ID
            }
        Next

        For Each row As DataSet In matrix
            Dim ID$ = row.ID

            For Each partner In row.Properties
                If Math.Abs(partner.Value) >= cutoff Then
                    Dim uid$ = {partner.Key, ID}.OrderBy(Function(s) s).JoinBy(" - ")

                    If Not edges.ContainsKey(uid) Then
                        edges(uid) = New NetworkEdge With {
                            .FromNode = ID,
                            .ToNode = partner.Key,
                            .value = partner.Value,
                            .Interaction = HowStrong(partner.Value)
                        }
                    End If
                End If
            Next
        Next

        Return New NetworkTables(nodes.Values, edges.Values)
    End Function

    Private Function HowStrong(c#) As String
        Dim abs = Math.Abs(c)

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

