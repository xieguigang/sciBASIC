#Region "Microsoft.VisualBasic::c2a3a9e19f456d476893db6513570097, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\LCS.vb"

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

    '   Total Lines: 75
    '    Code Lines: 51
    ' Comment Lines: 9
    '   Blank Lines: 15
    '     File Size: 2.60 KB


    '     Module LongestCommonSubsequenceExtension
    ' 
    '         Function: charEquals, MaxLengthSubString, MaxSet
    ' 
    '         Sub: doLCSInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ComponentModel.Algorithm.DynamicProgramming

    ''' <summary>
    ''' Longest Common Subsequence
    ''' </summary>
    Public Module LongestCommonSubsequenceExtension

        ''' <summary>
        ''' 比较两个字符串之间的最长的子串
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function MaxLengthSubString(a As String, b As String) As String
            Return MaxSet(a.ToArray, b.ToArray, AddressOf charEquals)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function charEquals(a As Char, b As Char) As Boolean
            Return a = b
        End Function

        Public Function MaxSet(Of T)(a As T(), b As T(), equals As IEquals(Of T)) As T()
            Dim m As Integer = a.Length
            Dim n As Integer = b.Length
            Dim len()() As Integer = RectangularArray.Matrix(Of Integer)(m + 1, n + 1)
            Dim p()() As Char = RectangularArray.Matrix(Of Char)(m + 1, n + 1)

            For i As Integer = 1 To m
                For j As Integer = 1 To n

                    If equals(a(i - 1), b(j - 1)) Then
                        len(i)(j) = len(i - 1)(j - 1) + 1
                        p(i)(j) = "-"c
                    ElseIf len(i - 1)(j) >= len(i)(j - 1) Then
                        len(i)(j) = len(i - 1)(j)
                        p(i)(j) = "<"c
                    Else
                        len(i)(j) = len(i)(j - 1)
                        p(i)(j) = ">"c
                    End If
                Next j
            Next i

            Dim lst As New List(Of T)

            doLCSInternal(p, a, a.Length, b.Length, lst)

            Call lst.Reverse()

            Return lst.ToArray
        End Function

        Private Sub doLCSInternal(Of T)(p()() As Char, a() As T, i As Integer, j As Integer, ByRef lst As List(Of T))
            If i = 0 OrElse j = 0 Then
                Return
            End If

            If p(i)(j) = "-"c Then
                doLCSInternal(p, a, i - 1, j - 1, lst)
                lst += a(i - 1)

            ElseIf p(i)(j) = "<"c Then
                doLCSInternal(p, a, i - 1, j, lst)

            ElseIf p(i)(j) = ">"c Then
                doLCSInternal(p, a, i, j - 1, lst)
            End If
        End Sub
    End Module
End Namespace
