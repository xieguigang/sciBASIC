﻿#Region "Microsoft.VisualBasic::2e3bf5f58e5b11db07d1f3d6441a5f85, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Expression.vb"

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

    '   Total Lines: 130
    '    Code Lines: 97 (74.62%)
    ' Comment Lines: 11 (8.46%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 22 (16.92%)
    '     File Size: 4.30 KB


    '     Structure DesignerExpression
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseInternalApi, ToString
    '         Delegate Function
    ' 
    '             Function: alpha, darker, lighter, Modify, reverse
    '                       skip, take
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Drawing2D.Colors

    ''' <summary>
    ''' ```vbnet
    ''' lighter(term, percentage)
    ''' darker(term, percentage)
    ''' alpha(term, percentage) percentage = [0, 1]
    ''' reverse(term)
    ''' skip(term, n)
    ''' take(term, n)
    ''' ```
    ''' </summary>
    Public Structure DesignerExpression

        Dim Term$
        Dim API As NamedValue(Of String)

        Public Const FunctionPattern$ = "[a-z0-9_]+\(.+\)"

        Sub New(exp$)
            If exp.IsPattern(FunctionPattern) Then
                API = ParseInternalApi(exp, Term)
            Else
                Term = exp
            End If
        End Sub

        Private Shared Function ParseInternalApi(exp As String, ByRef term As String) As NamedValue(Of String)
            With exp.GetTagValue("(", trim:=True)
                Dim api$ = .Name
                Dim arg$ = Nothing

                With .Value _
                    .Trim(")"c) _
                    .GetTagValue(",", trim:=True)

                    term = .Name.Trim

                    If term.StringEmpty Then
                        ' 只有一个参数
                        term = .Value
                    Else
                        arg = .Value
                    End If
                End With

                Return New NamedValue(Of String) With {
                    .Name = api,
                    .Value = arg
                }
            End With
        End Function

        Public Overrides Function ToString() As String
            If API.IsEmpty Then
                Return Term
            Else
                With API
                    If .Value.StringEmpty Then
                        Return $"{ .Name} ( {Term} )"
                    Else
                        Return $"{ .Name} ( {Term}, { .Value} )"
                    End If
                End With
            End If
        End Function

        Delegate Function Apply(colors As Color(), value$) As Color()

        Friend Shared ReadOnly actions As New Dictionary(Of String, Apply) From {
            {"lighter", New Apply(AddressOf lighter)},
            {"darker", New Apply(AddressOf darker)},
            {"alpha", New Apply(AddressOf alpha)},
            {"reverse", New Apply(AddressOf reverse)},
            {"rev", New Apply(AddressOf reverse)},
            {"skip", New Apply(AddressOf skip)},
            {"take", New Apply(AddressOf take)}
        }

        Public Function Modify(colors As Color()) As Color()
            If API.IsEmpty Then
                Return colors
            Else
                With API
                    Return actions(.Name.ToLower)(colors, .Value)
                End With
            End If
        End Function

        Private Shared Function lighter(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble

            Return colors _
                .Select(Function(c)
                            Return HSLColor _
                                .GetHSL(c) _
                                .Lighten(percentage, Color.White)
                        End Function) _
                .ToArray
        End Function

        Private Shared Function darker(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble

            Return colors _
                .Select(Function(c) c.Darken(percentage)) _
                .ToArray
        End Function

        Private Shared Function alpha(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble
            Return colors.Alpha(255 * percentage)
        End Function

        Private Shared Function reverse(colors As Color(), value$) As Color()
            Return colors.Reverse.ToArray
        End Function

        Private Shared Function skip(colors As Color(), value$) As Color()
            Return colors.Skip(CInt(Val(value))).ToArray
        End Function

        Private Shared Function take(colors As Color(), value$) As Color()
            Return colors.Take(CInt(Val(value))).ToArray
        End Function
    End Structure
End Namespace
