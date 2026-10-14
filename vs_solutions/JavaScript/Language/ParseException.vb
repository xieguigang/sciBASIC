#Region "Microsoft.VisualBasic::1fb11aae0889fcb1d9ef46773e77c69c, vs_solutions\JavaScript\Language\ParseException.vb"

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

    '   Total Lines: 9
    '    Code Lines: 6 (66.67%)
    ' Comment Lines: 1 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (22.22%)
    '     File Size: 257 B


    ' Class ParseException
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Syntax error raised by the lexer and parser.
''' <para>
''' The <see cref="Exception.Message"/> text is the historical wording (some throw
''' sites already embed the position as <c>at line:col</c>) and must not change;
''' <see cref="Line"/> / <see cref="Column"/> carry the same information in a
''' structured way so a host can render a real stack frame for the failure.
''' </para>
''' </summary>
Public NotInheritable Class ParseException
    Inherits Exception

    ''' <summary>Source line of the offending token, 1 based; 0 when unknown.</summary>
    Public ReadOnly Property Line As Integer

    ''' <summary>Source column of the offending token, 1 based; 0 when unknown.</summary>
    Public ReadOnly Property Column As Integer

    Public Sub New(message As String)
        Me.New(message, 0, 0)
    End Sub

    Public Sub New(message As String, line As Integer, column As Integer)
        MyBase.New(message)
        Me.Line = System.Math.Max(0, line)
        Me.Column = System.Math.Max(0, column)
    End Sub

    ''' <summary>True when the failing source position is known.</summary>
    Public ReadOnly Property HasPosition As Boolean
        Get
            Return Line > 0
        End Get
    End Property

    ''' <summary>
    ''' Same error with the failing position attached; returns this instance when the
    ''' position is already known (or when <paramref name="line"/> is unknown), so a
    ''' rethrow from an outer catch never loses the original error text.
    ''' </summary>
    Public Function WithPosition(line As Integer, column As Integer) As ParseException
        If HasPosition OrElse line <= 0 Then Return Me

        Return New ParseException(Message, line, column)
    End Function

    ''' <summary><see cref="WithPosition(Integer, Integer)"/> for the location of a token.</summary>
    Public Function WithPosition(token As Token) As ParseException
        If token Is Nothing Then Return Me

        Return WithPosition(token.span.line, token.span.start)
    End Function

    ''' <summary>Position of <paramref name="token"/>, or <see cref="SourcePosition.None"/>.</summary>
    Public Shared Function PositionOf(token As Token) As SourcePosition
        If token Is Nothing Then Return SourcePosition.None

        Return New SourcePosition(token.span.line, token.span.start)
    End Function

End Class
