#Region "Microsoft.VisualBasic::4327f967e9da14991349520d6800c41e, Data_science\Graph\Analysis\Ullmann.vb"

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

    '   Total Lines: 167
    '    Code Lines: 128 (76.65%)
    ' Comment Lines: 16 (9.58%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 23 (13.77%)
    '     File Size: 6.53 KB


    '     Class Ullmann
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckAdjacencyConstraints, creatM0Matrix, ExplainNodeMapping, FindIsomorphisms, getDegree
    '                   RefineM
    ' 
    '         Sub: Backtrack, (+2 Overloads) checkMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Analysis

    Public Class Ullmann

        ''' <summary>
        ''' The target graph
        ''' </summary>
        ReadOnly T As Integer()()
        ''' <summary>
        ''' the query graph
        ''' </summary>
        ReadOnly Q As Integer()()

        ReadOnly M0 As Integer()()


        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="largeGraphMatrix">target graph, should be a graph with larger vertex count</param>
        ''' <param name="searchGraphMatrix">query graph, should be a graph with smaller vertex count</param>
        Public Sub New(largeGraphMatrix As Integer()(), searchGraphMatrix As Integer()())
            Call checkMatrix(largeGraphMatrix, searchGraphMatrix)

            Me.T = largeGraphMatrix
            Me.Q = searchGraphMatrix

            M0 = creatM0Matrix()
        End Sub

        Private Shared Sub checkMatrix(ByRef largeGraphMatrix As Integer()(), ByRef searchGraphMatrix As Integer()())
            Call checkMatrix(largeGraphMatrix, "target graph")
            Call checkMatrix(searchGraphMatrix, "query graph")
        End Sub

        Private Shared Sub checkMatrix(ByRef x As Integer()(), obj$)
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New NullReferenceException($"the required graph matrix object '{obj}' should not be nothing!")
            End If
            ' check dimension matched
            If x.Length <> x(0).Length Then
                Throw New InvalidDataException($"dimension not matched: the given data object '{obj}' is not a valid matrix data!")
            End If
        End Sub

        Public Shared Iterator Function ExplainNodeMapping(ullmann As IEnumerable(Of Integer()), G As String(), H As String()) As IEnumerable(Of NamedValue(Of String))
            For Each gv As (map As Integer(), gid As String) In ullmann.Zip(join:=G)
                For i As Integer = 0 To H.Length - 1
                    If gv.map(i) > 0 Then
                        Yield New NamedValue(Of String)(gv.gid, H(i))
                    End If
                Next
            Next
        End Function

        Private Shared Function getDegree(vertex As Integer, graph As Integer()()) As Integer
            Return graph(vertex).Sum()
        End Function

        Private Function creatM0Matrix() As Integer()()
            Dim res(Q.Length - 1)() As Integer
            For i As Integer = 0 To Q.Length - 1
                res(i) = New Integer(T.Length - 1) {}
                For j As Integer = 0 To T.Length - 1
                    Dim targetDegree As Integer = getDegree(j, T)
                    Dim queryDegree As Integer = getDegree(i, Q)
                    res(i)(j) = If(targetDegree >= queryDegree, 1, 0)
                Next
            Next
            Return res
        End Function

        ''' <summary>
        ''' get vertex node mapping result, which could be explained via the <see cref="ExplainNodeMapping"/> function.
        ''' </summary>
        ''' <returns></returns>
        Public Function FindIsomorphisms() As IEnumerable(Of Integer())
            Dim results As New List(Of Integer())
            Dim M0 As Integer()() = Me.M0
            Dim currentMatch(Q.Length - 1) As Integer
            Dim used(T.Length - 1) As Boolean
            Call Backtrack(0, currentMatch.Clone(), used.Clone(), M0, results)
            Return results
        End Function

        Private Sub Backtrack(
        depth As Integer,
        currentMatch() As Integer,
        used() As Boolean,
        M As Integer()(),
        ByRef results As List(Of Integer())
    )
            If depth = Q.Length Then
                results.Add(CType(currentMatch.Clone(), Integer()))
                Return
            End If

            For j As Integer = 0 To T.Length - 1
                If M(depth)(j) = 1 AndAlso Not used(j) Then
                    If CheckAdjacencyConstraints(depth, j, currentMatch, depth) Then
                        used(j) = True
                        currentMatch(depth) = j
                        Dim newM As Integer()() = RefineM(M, depth, j, currentMatch)
                        Backtrack(depth + 1, currentMatch.Clone(), used.Clone(), newM, results)
                        used(j) = False
                        currentMatch(depth) = -1
                    End If
                End If
            Next
        End Sub

        Private Function CheckAdjacencyConstraints(
        currentQVertex As Integer,
        currentTVertex As Integer,
        currentMatch() As Integer,
        depth As Integer
    ) As Boolean
            For l As Integer = 0 To depth - 1
                If Q(currentQVertex)(l) = 1 AndAlso T(currentTVertex)(currentMatch(l)) <> 1 Then
                    Return False
                End If
                If Q(l)(currentQVertex) = 1 AndAlso T(currentMatch(l))(currentTVertex) <> 1 Then
                    Return False
                End If
            Next
            Return True
        End Function

        Private Function RefineM(
        oldM As Integer()(),
        currentDepth As Integer,
        selectedTVertex As Integer,
        currentMatch() As Integer
    ) As Integer()()
            Dim newM = oldM.Select(Function(row) row.ToArray()).ToArray()

            For j As Integer = 0 To newM(currentDepth).Length - 1
                newM(currentDepth)(j) = If(j = selectedTVertex, 1, 0)
            Next

            For i As Integer = currentDepth + 1 To Q.Length - 1
                For j As Integer = 0 To T.Length - 1
                    If newM(i)(j) = 1 Then
                        For l As Integer = 0 To currentDepth
                            If Q(i)(l) = 1 AndAlso T(j)(currentMatch(l)) = 0 Then
                                newM(i)(j) = 0
                                Exit For
                            End If
                            If Q(l)(i) = 1 AndAlso T(currentMatch(l))(j) = 0 Then
                                newM(i)(j) = 0
                                Exit For
                            End If
                        Next
                    End If
                Next
            Next

            Return newM
        End Function

    End Class

End Namespace
