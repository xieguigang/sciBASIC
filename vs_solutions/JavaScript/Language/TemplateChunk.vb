#Region "Microsoft.VisualBasic::0549110cb708b1f7ef436c37c2eca4fa, vs_solutions\JavaScript\Language\TemplateChunk.vb"

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
    '    Code Lines: 8 (38.10%)
    ' Comment Lines: 9 (42.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (19.05%)
    '     File Size: 725 B


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

