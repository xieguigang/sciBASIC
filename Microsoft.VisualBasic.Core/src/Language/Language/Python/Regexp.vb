#Region "Microsoft.VisualBasic::ab30b487b449be23e5a9d0953d89542f, Microsoft.VisualBasic.Core\src\Language\Language\Python\Regexp.vb"

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

    '   Total Lines: 87
    '    Code Lines: 50
    ' Comment Lines: 21
    '   Blank Lines: 16
    '     File Size: 3.33 KB


    '     Module re
    ' 
    '         Function: __trimComment, FindAll
    '         Structure Match
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Language.Python

    ''' <summary>
    ''' This module provides regular expression matching operations similar to those found in Perl. ``re`` module in the python language.
    ''' </summary>
    Public Module re

        Public Structure Match
            Private __raw As System.Text.RegularExpressions.Match

            Friend Sub New(m As System.Text.RegularExpressions.Match)
                __raw = m
            End Sub

            Default Public ReadOnly Property Value(index%) As String
                Get
                    If index < 0 Then
                        index = __raw.Groups.Count + index%
                    End If

                    Return __raw.Groups.Item(index%).Value
                End Get
            End Property

            Public Shared Narrowing Operator CType(m As Match) As String
                Return m.__raw.Value
            End Operator

            ''' <summary>
            ''' <see cref="System.Text.RegularExpressions.Match.Value"/>
            ''' </summary>
            ''' <returns></returns>
            Public Overrides Function ToString() As String
                Return __raw.Value
            End Function
        End Structure

        ''' <summary>
        ''' Return all non-overlapping matches of pattern in string, as a list of strings. The string is scanned left-to-right, and matches are returned in the order found. 
        ''' If one or more groups are present in the pattern, return a list of groups; this will be a list of tuples if the pattern has more than one group. 
        ''' Empty matches are included in the result unless they touch the beginning of another match.
        ''' </summary>
        ''' <param name="pattern">这个会首先被分行然后去除掉python注释</param>
        ''' <param name="input"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function FindAll(pattern$, input$, Optional options As RegexOptions = RegexOptions.None) As Array(Of Match)
            Dim tokens As String() = pattern.Trim _
                .LineTokens _
                .Select(AddressOf __trimComment) _
                .Where(Function(s) Not String.IsNullOrEmpty(s)) _
                .ToArray
            pattern = String.Join("", tokens)

            Dim ms As MatchCollection =
                Regex.Matches(input, pattern, options)
            Dim mlist As IEnumerable(Of Match) =
                ms.Count _
                .Sequence _
                .Select(Function(i) New Match(ms(i)))

            Return New Array(Of Match)(mlist)
        End Function

        ''' <summary>
        ''' 假设所有的注释都是由#和一个空格开始起始的 ``# ``
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Private Function __trimComment(s As String) As String
            s = s.Trim

            If s.StartsWith("# ") Then Return "" ' 整行都是注释

            Dim i As Integer = s.IndexOf("# ")

            If i > -1 Then
                s = s.Substring(0, i).Trim
            End If

            Return s
        End Function
    End Module
End Namespace
