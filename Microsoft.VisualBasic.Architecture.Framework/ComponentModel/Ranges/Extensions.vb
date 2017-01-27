#Region "Microsoft.VisualBasic::59b62960f9139186393d4cec0ec1b1fb, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Ranges\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting

Namespace ComponentModel.Ranges

    Public Module Extensions

        ''' <summary>
        ''' + ``min -> max``
        ''' + ``[min,max]``
        ''' + ``{min,max}``
        ''' + ``(min,max)``
        ''' + ``min,max``
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <param name="min#"></param>
        ''' <param name="max#"></param>
        <Extension> Public Sub Parser(exp$, ByRef min#, ByRef max#)
            Dim t$()
            Dim raw$ = exp

            If InStr(exp, "->") > 0 Then
                t = Strings.Split(exp, "->")
            Else
                exp = Regex.Match(exp, RegexpFloat & "\s*,\s*" & RegexpFloat).Value

                If String.IsNullOrEmpty(exp) Then
                    exp = $"'{raw}' is not a valid expression format!"
                    Throw New FormatException(exp)
                Else
                    t = exp.Split(","c)
                End If
            End If

            t = t.ToArray(AddressOf Trim)

            min = Casting.ParseNumeric(t(Scan0))
            max = Casting.ParseNumeric(t(1))
        End Sub
    End Module
End Namespace
