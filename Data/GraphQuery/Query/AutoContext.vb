#Region "Microsoft.VisualBasic::afcaede2db5e3503a398605c71e96b2c, Data\GraphQuery\Query\AutoContext.vb"

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

    '   Total Lines: 22
    '    Code Lines: 17 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 691 B


    ' Module AutoContext
    ' 
    '     Function: Attribute, IsAutoContext
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.Document

Module AutoContext

    Friend Const AutoContext As String = "graphquery-auto-context"

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function IsAutoContext(element As HtmlElement) As Boolean
        Return element.hasAttribute(AutoContext)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Attribute() As ValueAttribute
        Return New ValueAttribute With {
            .Name = AutoContext,
            .Values = New List(Of String) From {"in-memory-source"}
        }
    End Function

End Module
