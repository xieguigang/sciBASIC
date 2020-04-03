#Region "Microsoft.VisualBasic::df1c802c309b51ab1710f2ea920d4dc5, mime\application%rdf+xml\MathML\maction.vb"

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

' Class maction
' 
'     Properties: [class], actiontype, id, style
' 
' Enum actiontypes
' 
'     statusline, toggle, tooltip
' 
'  
' 
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

    Public Enum actiontypes
        statusline
        toggle
        ''' <summary>
        ''' When the pointer moves over the expression, a tooltip box with a message is displayed near the expression.
        ''' The syntax Is: ``&lt;maction actiontype = "tooltip"> expression message &lt;/maction>``.
        ''' </summary>
        tooltip
    End Enum
End Namespace