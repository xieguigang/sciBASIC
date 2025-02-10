#Region "Microsoft.VisualBasic::854000c3c3ffa1dc7892813139da84ea, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BipartiteMatching.vb"

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

    '   Total Lines: 45
    '    Code Lines: 39 (86.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (13.33%)
    '     File Size: 1.49 KB


    '     Class BipartiteMatching
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Dfs, Match
    ' 
    '         Sub: AddEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm

    Public NotInheritable Class BipartiteMatching

        ReadOnly _v As Integer
        ReadOnly _g As List(Of Integer)()

        Public Sub New(v As Integer)
            _v = v
            _g = Enumerable.Repeat(0, v).[Select](Function(__) New List(Of Integer)()).ToArray()
        End Sub

        Public Sub AddEdge(u As Integer, v As Integer)
            _g(u).Add(v)
            _g(v).Add(u)
        End Sub

        Public Function Match() As Integer
            Dim res As Integer = 0
            Dim matches = Enumerable.Repeat(-1, _v).ToArray()
            For v As Integer = 0 To _v - 1
                If matches(v) < 0 Then
                    Dim used = New Boolean(_v - 1) {}
                    If Dfs(v, used, matches) Then
                        res += 1
                    End If
                End If
            Next
            Return res
        End Function

        Private Function Dfs(v As Integer, used As Boolean(), matches As Integer()) As Boolean
            used(v) = True
            For i As Integer = 0 To _g(v).Count - 1
                Dim u = _g(v)(i), w = matches(u)
                If w < 0 OrElse Not used(w) AndAlso Dfs(w, used, matches) Then
                    matches(v) = u
                    matches(u) = v
                    Return True
                End If
            Next
            Return False
        End Function
    End Class
End Namespace
