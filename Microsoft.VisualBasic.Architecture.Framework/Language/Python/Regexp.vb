#Region "Microsoft.VisualBasic::62a0b903f749a63e29c778493e652b7e, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Python\Regexp.vb"

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

Imports System.Text.RegularExpressions

Namespace Language.Python

    ''' <summary>
    ''' ``re`` module in the python language.
    ''' </summary>
    Public Module Regexp

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pattern">这个会首先被分行然后去除掉python注释</param>
        ''' <param name="input"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function FindAll(pattern As String, input As String, Optional options As RegexOptions = RegexOptions.None) As Array(Of String)
            Dim tokens As String() = pattern.Trim.lTokens _
                .Select(AddressOf __trimComment) _
                .Where(Function(s) Not String.IsNullOrEmpty(s)) _
                .ToArray
            pattern = String.Join("", tokens)
            Return New Array(Of String)(
                Regex.Matches(input, pattern, options).EachValue)
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
