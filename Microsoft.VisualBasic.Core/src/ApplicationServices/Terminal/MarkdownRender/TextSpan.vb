#Region "Microsoft.VisualBasic::e582f274f1c318c3224dcb568715bfb7, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\TextSpan.vb"

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

    '   Total Lines: 61
    '    Code Lines: 40 (65.57%)
    ' Comment Lines: 7 (11.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (22.95%)
    '     File Size: 1.96 KB


    '     Class TextSpan
    ' 
    '         Properties: IsEndByNewLine, style, text
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' console print text with a specific styles
    ''' </summary>
    Public Class TextSpan

        Public Property text As String

        ''' <summary>
        ''' print the pnain text if the style is nothing
        ''' </summary>
        ''' <returns></returns>
        Public Property style As ConsoleFormat
        Public Property IsEndByNewLine As Boolean

        Sub New()
        End Sub

        Sub New(s As String, style As ConsoleFormat)
            Me.text = s
            Me.style = style
        End Sub

        Sub New(s As String, foreground As AnsiColor,
                Optional background As AnsiColor = Nothing,
                Optional bold As Boolean = False,
                Optional underline As Boolean = False,
                Optional inverted As Boolean = False)

            Call Me.New(s, New ConsoleFormat(foreground, background, bold, underline, inverted))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If style Is Nothing Then
                Return text
            End If

            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(style) & text
        End Function

        Public Shared Narrowing Operator CType(span As TextSpan) As String
            If span.style Is Nothing Then
                Return span.text
            End If

            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(span.style) & span.text
        End Operator

        Public Shared Operator &(span As TextSpan, str As String) As String
            Return CType(span, String) & str
        End Operator

        Public Shared Operator &(str As String, span As TextSpan) As String
            Return str & CType(span, String)
        End Operator
    End Class
End Namespace
