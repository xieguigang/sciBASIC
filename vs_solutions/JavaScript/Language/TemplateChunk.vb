#Region "Microsoft.VisualBasic::9a83c01a0da8af1ff2eecf401145ac2f, vs_solutions\JavaScript\Language\TemplateChunk.vb"

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

    '   Total Lines: 20
    '    Code Lines: 8 (40.00%)
    ' Comment Lines: 9 (45.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (15.00%)
    '     File Size: 721 B


    ' Class TemplateChunk
    ' 
    '     Properties: IsExpression, Text
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' One chunk of a template literal token.
''' <para>
''' <c>IsExpression = False</c>: cooked text (escapes already decoded).
''' <c>IsExpression = True</c>: the RAW source text of a <c>${...}</c> placeholder,
''' which the parser recursively lexes/parses into an <see cref="Expression"/>.
''' </para>
''' <para>Example: <c>`a${x + 1}b`</c> → [text "a", expr "x + 1", text "b"].</para>
''' </summary>
Public NotInheritable Class TemplateChunk

    Public ReadOnly Property IsExpression As Boolean
    Public ReadOnly Property Text As String

    Public Sub New(isExpression As Boolean, text As String)
        Me.IsExpression = isExpression
        Me.Text = text
    End Sub

End Class
