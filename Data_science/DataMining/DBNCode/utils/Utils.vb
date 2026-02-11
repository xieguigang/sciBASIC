#Region "Microsoft.VisualBasic::b209bd5bb7a51c99dd4881cb701a5d86, Data_science\DataMining\DBNCode\utils\Utils.vb"

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

    '   Total Lines: 65
    '    Code Lines: 47 (72.31%)
    ' Comment Lines: 3 (4.62%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (23.08%)
    '     File Size: 2.06 KB


    '     Class Utils
    ' 
    '         Function: adjacencyMatrix, isNumeric, readFile, topologicalSort
    ' 
    '         Sub: dfs, writeToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace utils

    Public Class Utils

        Public Shared Function readFile(pathName As String) As IList(Of String)
            Return pathName.ReadAllLines().AsList()
        End Function

        Public Shared Sub writeToFile(fileName As String, contents As String)
            Call contents.SaveTo(fileName)
        End Sub

        Public Shared Function isNumeric(str As String) As Boolean
            ' TODO: change to non-exception method
            Try
                Double.Parse(str)
            Catch __unusedFormatException1__ As FormatException
                Return False
            End Try
            Return True
        End Function

        Private Shared Sub dfs(graph As IList(Of IList(Of Integer)), used As Boolean(), res As IList(Of Integer), u As Integer)
            used(u) = True
            For Each v In graph(u)
                If Not used(v) Then
                    Utils.dfs(graph, used, res, v)
                End If
            Next
            res.Add(u)
        End Sub

        ' adapted from
        ' https://sites.google.com/site/indy256/algo/topological_sorting
        Public Shared Function topologicalSort(graph As IList(Of IList(Of Integer))) As IList(Of Integer)
            Dim n = graph.Count
            Dim used = New Boolean(n - 1) {}
            Dim res As IList(Of Integer) = New List(Of Integer)()
            For i = 0 To n - 1
                If Not used(i) Then
                    Utils.dfs(graph, used, res, i)
                End If
            Next
            res.Reverse()
            Return res
        End Function

        Public Shared Function adjacencyMatrix(edges As IList(Of Edge), n As Integer) As Boolean()()

            Dim adj = RectangularArray.Matrix(Of Boolean)(n, n)

            For Each e As Edge In edges
                adj(e.Head) = adj(e.Tail)
            Next

            Return adj

        End Function


    End Class

End Namespace

