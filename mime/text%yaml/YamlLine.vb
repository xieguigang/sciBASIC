#Region "Microsoft.VisualBasic::5e45af2d35371327e7d03fca97539d7f, mime\text%yaml\YamlLine.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 6 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (11.54%)
    '     File Size: 904 B


    ' Class YamlLine
    ' 
    '     Properties: Content, Indent, IsListItem, LineNumber
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Represents a single pre-processed line of YAML text with its indentation level.
''' </summary>
Friend Class YamlLine

    ''' <summary>Indentation level (number of leading spaces).</summary>
    Public Property Indent As Integer
    ''' <summary>Content of the line (with leading whitespace stripped).</summary>
    Public Property Content As String
    ''' <summary>Original line number in the source text (for error reporting).</summary>
    Public Property LineNumber As Integer

    Public ReadOnly Property IsListItem As Boolean
        Get
            If Content Is Nothing Then
                Return False
            Else
                Return Content.TrimStart.StartsWith("-")
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{LineNumber}] {Content}"
    End Function
End Class
