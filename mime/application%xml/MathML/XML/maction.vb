#Region "Microsoft.VisualBasic::29afd3833f7c25be169196a4e1c45d8b, mime\application%xml\MathML\XML\maction.vb"

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

    '   Total Lines: 18
    '    Code Lines: 8 (44.44%)
    ' Comment Lines: 7 (38.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (16.67%)
    '     File Size: 633 B


    '     Class maction
    ' 
    '         Properties: [class], actiontype, id, style
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MathML

    ''' <summary>
    ''' The MathML ``&lt;maction>`` element provides a possibility 
    ''' to bind actions to (sub-) expressions. The action itself 
    ''' is specified by the actiontype attribute, which accepts 
    ''' several values. To specify which child elements are addressed 
    ''' by the action, you can make use of the selection attribute.
    ''' </summary>
    Public Class maction

        Public Property actiontype As actiontypes
        Public Property [class] As String
        Public Property id As String
        Public Property style As String

    End Class
End Namespace
