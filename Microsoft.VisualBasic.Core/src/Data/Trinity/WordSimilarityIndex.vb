#Region "Microsoft.VisualBasic::5339882ed85e020df3e003b9bdb7d4b6, Microsoft.VisualBasic.Core\src\Data\Trinity\WordSimilarityIndex.vb"

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

    '   Total Lines: 134
    '    Code Lines: 96
    ' Comment Lines: 11
    '   Blank Lines: 27
    '     File Size: 4.86 KB


    '     Class WordSimilarityIndex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddTerm, asciiToString, FindMatches
    ' 
    '     Class WordSimilarityIndex
    ' 
    '         Properties: AllValues, Count
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddTerm, FindMatches, HaveKey
    ' 
    '     Class WordSimilarity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareAsciiVector, GetHashCode, IEqualityComparer_Equals
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace Data.Trinity

    ''' <summary>
    ''' 使用模拟比较来建立所给定的字符串的快速模糊查找的索引对象模型
    ''' </summary>
    Public Class WordSimilarityIndex

        ''' <summary>
        ''' 二叉树的主键是字符串的ASCII编码缓存
        ''' </summary>
        ReadOnly bin As AVLTree(Of Integer(), String)

        Sub New(similarity As WordSimilarity)
            bin = New AVLTree(Of Integer(), String)(AddressOf similarity.CompareAsciiVector, AddressOf asciiToString)
        End Sub

        Private Shared Function asciiToString(ascii As Integer()) As String
            Return ascii.Select(AddressOf ChrW).CharString
        End Function

        Public Function AddTerm(term As String) As WordSimilarityIndex
            Call bin.Add(term.Select(AddressOf AscW).ToArray, term, False)
            Return Me
        End Function

        Public Iterator Function FindMatches(term As String) As IEnumerable(Of String)
            Dim node As BinaryTree(Of Integer(), String) = term _
                .Select(AddressOf AscW) _
                .ToArray _
                .DoCall(AddressOf bin.Find)

            ' the given term key have no match items
            ' in current index object
            If node Is Nothing Then
                Return
            End If

            Yield node.Value

            For Each member As String In node.Members
                Yield member
            Next
        End Function

    End Class

    ''' <summary>
    ''' 使用模拟比较来建立所给定的字符串的快速模糊查找的索引对象模型
    ''' </summary>
    Public Class WordSimilarityIndex(Of T)

        ReadOnly bin As WordSimilarityIndex
        ReadOnly table As New Dictionary(Of String, T)

        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return table.Count
            End Get
        End Property

        Public ReadOnly Property AllValues As IEnumerable(Of T)
            Get
                Return table.Values
            End Get
        End Property

        Sub New(Optional similarity As WordSimilarity = Nothing)
            Static defaultThreshold As [Default](Of WordSimilarity) = New WordSimilarity
            bin = New WordSimilarityIndex(similarity Or defaultThreshold)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HaveKey(term As String) As Boolean
            Return table.ContainsKey(term)
        End Function

        Public Function AddTerm(term$, value As T) As WordSimilarityIndex(Of T)
            Call bin.AddTerm(term)
            Call table.Add(term, value)

            Return Me
        End Function

        Public Iterator Function FindMatches(term As String) As IEnumerable(Of T)
            If table.ContainsKey(term) Then
                Yield table(term)
            End If

            For Each key As String In bin.FindMatches(term)
                Yield table(key)
            Next
        End Function
    End Class

    Public Class WordSimilarity : Implements IEqualityComparer(Of Integer())

        ReadOnly equalsThreshold, right As Double

        Sub New(Optional equals As Double = 0.85, Optional right As Double = 0.6)
            Me.equalsThreshold = equals
            Me.right = right
        End Sub

        Public Function CompareAsciiVector(x As Integer(), y As Integer()) As Integer
            With LevenshteinDistance.ComputeDistance(x, y, Function(a, b) a = b, AddressOf ChrW)
                If .IsNothing Then
                    Return -1
                ElseIf .MatchSimilarity >= equalsThreshold Then
                    Return 0
                Else
                    Return 1
                End If
            End With
        End Function

        Public Overloads Function GetHashCode(obj() As Integer) As Integer Implements IEqualityComparer(Of Integer()).GetHashCode
            Return obj.GetHashCode
        End Function

        Private Function IEqualityComparer_Equals(x() As Integer, y() As Integer) As Boolean Implements IEqualityComparer(Of Integer()).Equals
            With LevenshteinDistance.ComputeDistance(x, y, Function(a, b) a = b, Function(c) ChrW(c))
                Return?.MatchSimilarity >= Me.equalsThreshold
            End With
        End Function
    End Class
End Namespace
