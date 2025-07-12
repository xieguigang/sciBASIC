#Region "Microsoft.VisualBasic::4c92145c9a1c9d1cd0d3bbf719f7d610, Microsoft.VisualBasic.Core\src\Text\Patterns\CommonTagParser.vb"

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
    '    Code Lines: 112 (67.07%)
    ' Comment Lines: 30 (17.96%)
    '    - Xml Docs: 76.67%
    ' 
    '   Blank Lines: 25 (14.97%)
    '     File Size: 5.79 KB


    '     Class CommonTagParser
    ' 
    '         Properties: IsNullOrEmpty, LabelPrefix, MaxColumnIndex, maxLen, nameMatrix
    '                     Size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetColumn, GetTagNames, RemoveLeadingNumbersAndSymbols, StringMotif, ToString
    ' 
    '         Sub: walkLabels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Text.Patterns

    Public Class CommonTagParser

        Public ReadOnly Property nameMatrix As Char()()
        ''' <summary>
        ''' the max length of the sample names in char numbers
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property maxLen As Integer

        ''' <summary>
        ''' the max length of the labels common parts
        ''' </summary>
        Public ReadOnly Property MaxColumnIndex As Integer

        ''' <summary>
        ''' get the max common prefixed label value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LabelPrefix As String
            Get
                Return nameMatrix(Scan0).Take(MaxColumnIndex).CharString
            End Get
        End Property

        Public ReadOnly Property IsNullOrEmpty As Boolean
            Get
                Return nameMatrix.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' Get number of the input sample names
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return nameMatrix.Length
            End Get
        End Property

        Sub New(allSampleNames As IEnumerable(Of String), Optional maxDepth As Boolean = False)
            nameMatrix = allSampleNames _
                .SafeQuery _
                .Select(Function(name) If(name, "").ToArray) _
                .ToArray
            maxLen% = Aggregate name As Char()
                      In nameMatrix
                      Into Max(name.Length)

            Call walkLabels(maxDepth)
        End Sub

        Public Iterator Function GetTagNames() As IEnumerable(Of String)
            For Each row As Char() In nameMatrix
                Yield row.Skip(MaxColumnIndex).CharString
            Next
        End Function

        Public Iterator Function GetColumn(offset As Integer) As IEnumerable(Of Char)
            For Each row As Char() In nameMatrix
                If offset < row.Length Then
                    Yield row(offset)
                End If
            Next
        End Function

        Private Sub walkLabels(maxDepth As Boolean)
            Dim col As Char()
            Dim nameMatrix = Me.nameMatrix _
                .Where(Function(chrs) chrs.Length > 0) _
                .ToArray

            For i As Integer = 0 To maxLen - 1
                _MaxColumnIndex = i
                col = nameMatrix _
                    .Select(Function(name)
                                Return name.ElementAtOrNull(MaxColumnIndex)
                            End Function) _
                    .ToArray

                '      colIndex
                '      |
                ' iBAQ-AA-1
                ' iBAQ-BB-2
                If maxDepth Then
                    If Not nameMatrix _
                        .GroupBy(Function(cs)
                                     Return cs.Take(MaxColumnIndex + 1).CharString
                                 End Function) _
                        .All(Function(c)
                                 Return c.Count > 1
                             End Function) Then

                        _MaxColumnIndex -= 1
                        Exit For
                    End If
                Else
                    If col.Distinct.Count > 1 Then
                        ' group label at here
                        Exit For
                    End If
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return LabelPrefix
        End Function

        Public Shared Function StringMotif(commonStr As IEnumerable(Of String),
                                           Optional threshold As Double = 0.3,
                                           Optional any As Char = "-") As String
            Dim motif As New List(Of Char)
            Dim common As New CommonTagParser(commonStr)

            If common.IsNullOrEmpty Then
                Return ""
            Else
                ' convert percentage to count number
                threshold = threshold * common.Size
            End If

            Dim max As Integer = common.maxLen

            For i As Integer = 0 To max - 1
                Dim col As Char() = common.GetColumn(i).ToArray

                If col.Length < threshold Then
                    Exit For
                End If

                Dim chars As IGrouping(Of Char, Char)() = col _
                    .GroupBy(Function(a) a) _
                    .Where(Function(c) c.Count > threshold) _
                    .OrderByDescending(Function(a) a.Count) _
                    .ToArray

                If chars.Length = 0 Then
                    Call motif.Add(any)
                Else
                    Call motif.Add(chars.First.Key)
                End If
            Next

            Return motif.CharString
        End Function

        ''' <summary>
        ''' removes leading number and symbols
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns>
        ''' 1-B11 to B11
        ''' B2 remains B2
        ''' </returns>
        Public Shared Function RemoveLeadingNumbersAndSymbols(input As String) As String
            ' 正则表达式：匹配字符串开头（^）的一个或多个（+）数字（\d）或非字母数字字符（\W）
            Dim result As String = Regex.Replace(input, "^[\W\d]+", "")
            Return result
        End Function
    End Class
End Namespace
