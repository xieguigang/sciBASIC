#Region "Microsoft.VisualBasic::d2f8a5c17e6b43d9af0c1b8e5d7c2f60, vs_solutions\JavaScript\Language\SourcePosition.vb"

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

    '     Structure SourcePosition
    '
    '         Properties: Column, HasValue, Line
    '
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '
    ' /********************************************************************************/

#End Region

Imports System.Globalization

''' <summary>
''' A 1 based source location stamped on AST nodes by the parser (the lexer already
''' tracks the line/column of every token, see <see cref="Token.Location"/>).
''' <para>
''' This is a small value type so that stamping a node costs nothing: the interpreter
''' copies the position of the statement being executed onto the innermost stack
''' frame, which is what makes a JS stack frame point at the real offending line.
''' </para>
''' </summary>
Public Structure SourcePosition

    ''' <summary>Unknown position (line 0) — nodes the parser did not stamp.</summary>
    Public Shared ReadOnly None As New SourcePosition(0, 0)

    ''' <summary>Source line, 1 based; 0 means unknown.</summary>
    Public ReadOnly Line As Integer

    ''' <summary>Source column, 1 based; 0 means unknown.</summary>
    Public ReadOnly Column As Integer

    Sub New(line As Integer, column As Integer)
        Me.Line = System.Math.Max(0, line)
        Me.Column = System.Math.Max(0, column)
    End Sub

    ''' <summary>True when this position actually points at a source location.</summary>
    Public ReadOnly Property HasValue As Boolean
        Get
            Return Line > 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        If Not HasValue Then Return "<unknown>"

        Return Line.ToString(CultureInfo.InvariantCulture) & ":" & Column.ToString(CultureInfo.InvariantCulture)
    End Function

End Structure
