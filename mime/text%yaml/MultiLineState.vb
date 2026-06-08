#Region "Microsoft.VisualBasic::943360ca824558160481f827d4610bea, mime\text%yaml\MultiLineState.vb"

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

    '   Total Lines: 21
    '    Code Lines: 10 (47.62%)
    ' Comment Lines: 11 (52.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 1.16 KB


    ' Class MultiLineState
    ' 
    '     Properties: BaseIndent, ChompMinus, ChompPlus, IndentIndicator, Key
    '                 Lines, StartLineNumber, Style
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Tracks the state for multi-line string collection during pre-processing.
''' </summary>
Friend Class MultiLineState
    ''' <summary>The type of multi-line string: | (literal) or > (folded).</summary>
    Public Property Style As Char
    ''' <summary>Whether the block scalar has a '+' chomping indicator.</summary>
    Public Property ChompPlus As Boolean
    ''' <summary>Whether the block scalar has a '-' chomping indicator.</summary>
    Public Property ChompMinus As Boolean
    ''' <summary>The indentation indicator for the block scalar (0 if not specified).</summary>
    Public Property IndentIndicator As Integer
    ''' <summary>The key that this multi-line string is associated with.</summary>
    Public Property Key As String
    ''' <summary>The collected lines of the multi-line string.</summary>
    Public Property Lines As New List(Of String)
    ''' <summary>The base indentation level of the first content line.</summary>
    Public Property BaseIndent As Integer = -1
    ''' <summary>The line number where the block scalar started.</summary>
    Public Property StartLineNumber As Integer
End Class
