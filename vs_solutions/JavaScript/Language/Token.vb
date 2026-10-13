#Region "Microsoft.VisualBasic::d52fe847eb93f1722f25da4224f7d555, vs_solutions\JavaScript\Language\Token.vb"

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

    '   Total Lines: 30
    '    Code Lines: 21 (70.00%)
    ' Comment Lines: 3 (10.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 1.16 KB


    ' Class Token
    ' 
    '     Properties: LineBreakBefore, Location, Value
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.TokenIcer

''' <summary>A lexical token; <see cref="LineBreakBefore"/> drives the simple ASI rule.</summary>
Public NotInheritable Class Token : Inherits CodeToken(Of TokenType)

    ''' <summary>Parsed value for Number (Double) and String (String) tokens.</summary>
    Public ReadOnly Property Value As Object
    ''' <summary>True when a newline separates this token from the previous one (for ASI).</summary>
    Public ReadOnly Property LineBreakBefore As Boolean

    Public ReadOnly Property Location As String
        Get
            Return $"{span.line}:{span.start}"
        End Get
    End Property

    Public Sub New(type As TokenType, text As String, value As Object,
                   line As Integer, col As Integer, lineBreakBefore As Boolean)
        Me.name = type
        Me.text = text
        Me.Value = value
        Me.span = New CodeSpan With {.line = line, .start = col}
        Me.LineBreakBefore = lineBreakBefore
    End Sub

    Public Overrides Function ToString() As String
        Return $"{name} '{text}' @ {span.ToString}"
    End Function

End Class
