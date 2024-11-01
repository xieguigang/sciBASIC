#Region "Microsoft.VisualBasic::14a55eb4690708e077d529f349e68a12, Microsoft.VisualBasic.Core\src\Text\StringSimilarity\Levenshtein\LevenshteinTree.vb"

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

    '   Total Lines: 119
    '    Code Lines: 85 (71.43%)
    ' Comment Lines: 13 (10.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 21 (17.65%)
    '     File Size: 3.94 KB


    '     Class LevenshteinTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddString, Compares, Query
    ' 
    '     Class LevenshteinTreeIndex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddString, Compares, Query
    '         Structure TextKey
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree

Namespace Text.Levenshtein

    ''' <summary>
    ''' a text string similarity index
    ''' </summary>
    Public Class LevenshteinTree

        ReadOnly tree As AVLClusterTree(Of String)
        ReadOnly ignoreCase As Boolean = True
        ReadOnly cutoff As Double = 0.8
        ReadOnly cost As Double = 0.7
        ReadOnly strlen_diff As Boolean = True
        ReadOnly right As Double = 0.7

        Sub New()
            tree = New AVLClusterTree(Of String)(AddressOf Compares, Function(s) s, ComparisonDirectionPrefers.Right)
        End Sub

        Private Function Compares(a As String, b As String) As Integer
            Dim score As Double = Similarity.LevenshteinEvaluate(
                a, b,
                ignoreCase:=ignoreCase,
                cost:=cost,
                strlen_diff:=strlen_diff)

            If score > cutoff Then
                Return 0
            ElseIf score > right Then
                Return 1
            Else
                Return -1
            End If
        End Function

        Public Function AddString(s As String) As LevenshteinTree
            Call tree.Add(s)
            Return Me
        End Function

        ''' <summary>
        ''' query the indexed string data via string similarity
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function Query(s As String) As IEnumerable(Of String)
            Return tree.Search(s)
        End Function

    End Class

    Public Class LevenshteinTreeIndex

        ReadOnly tree As AVLClusterTree(Of TextKey)
        ReadOnly ignoreCase As Boolean = True
        ReadOnly cutoff As Double = 0.8
        ReadOnly cost As Double = 0.7
        ReadOnly strlen_diff As Boolean = True
        ReadOnly right As Double = 0.7

        Private Structure TextKey

            Dim offset As Integer
            Dim text As String

            Public Overrides Function ToString() As String
                Return $"[&{offset}] {text}"
            End Function

        End Structure

        Sub New(Optional ignoreCase As Boolean = True,
                Optional cutoff As Double = 0.8,
                Optional cost As Double = 0.7,
                Optional strlen_diff As Boolean = True,
                Optional right As Double = 0.7)

            Me.tree = New AVLClusterTree(Of TextKey)(AddressOf Compares, Function(s) s.text, ComparisonDirectionPrefers.Right)
            Me.ignoreCase = ignoreCase
            Me.cutoff = cutoff
            Me.cost = cost
            Me.strlen_diff = strlen_diff
            Me.right = right
        End Sub

        Private Function Compares(a As TextKey, b As TextKey) As Integer
            Dim score As Double = Similarity.LevenshteinEvaluate(
                a.text, b.text,
                ignoreCase:=ignoreCase,
                cost:=cost,
                strlen_diff:=strlen_diff)

            If score > cutoff Then
                Return 0
            ElseIf score > right Then
                Return 1
            Else
                Return -1
            End If
        End Function

        Public Function AddString(s As String, offset As Integer) As LevenshteinTreeIndex
            Call tree.Add(New TextKey With {.text = s, .offset = offset})
            Return Me
        End Function

        ''' <summary>
        ''' query the indexed data via string key similarity
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Iterator Function Query(s As String) As IEnumerable(Of Integer)
            For Each hit As TextKey In tree.Search(New TextKey With {.text = s, .offset = -1})
                Yield hit.offset
            Next
        End Function
    End Class
End Namespace
