#Region "Microsoft.VisualBasic::bf50accd923ab11048ea99ebb369101f, Microsoft.VisualBasic.Core\src\My\JavaScript\String.vb"

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

    '   Total Lines: 70
    '    Code Lines: 58 (82.86%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (17.14%)
    '     File Size: 2.41 KB


    '     Module [String]
    ' 
    '         Function: includes, (+2 Overloads) match, parseInt, (+2 Overloads) substr, (+2 Overloads) test
    ' 
    '         Sub: match
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace My.JavaScript

    Public Module [String]

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function parseInt(s As String, Optional radix% = 10) As Integer
            Return Convert.ToInt32(s, radix)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function substr(str$, start%) As String
            Return str.Substring(start)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function substr(str$, start%, length%) As String
            Return str.Substring(start, length)
        End Function

        <Extension>
        Public Function includes(str$, part$) As Boolean
            If str.StringEmpty Then
                Return False
            ElseIf part.StringEmpty Then
                Return True
            Else
                Return str.Contains(part)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function test(pattern$, target$) As Boolean
            Return r.Match(target, pattern).Success
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function test(r As Regex, target$) As Boolean
            Return r.Match(target).Success
        End Function

        <Extension>
        Public Sub match(text$, pattern$, ByRef a$, ByRef b$, Optional ByRef c$ = Nothing)
            Dim parts = text.Match(pattern)

            a = parts.ElementAtOrNull(0)
            b = parts.ElementAtOrNull(1)
            c = parts.ElementAtOrNull(2)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function match(text$, pattern$) As String()
            Return r.Match(text, pattern).Captures.AsQueryable.Cast(Of String).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function match(text$, pattern As r) As String()
            Return pattern.Match(text).Captures.AsQueryable.Cast(Of String).ToArray
        End Function
    End Module
End Namespace
