#Region "Microsoft.VisualBasic::12a8fc4eed84175fb7d0c68f1489c5b2, sciBASIC#\mime\text%markdown\Markups\HtmlWriter.vb"

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
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 951 B


    ' Class HtmlWriter
    ' 
    '     Function: div, img, p
    ' 
    ' /********************************************************************************/

#End Region

Public Class HtmlWriter

    Public Shared Function img(src As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <img src=<%= src %> id=<%= id %> class=<%= [class] %> style=<%= style %>/>
    End Function

    Public Shared Function p(content As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <p id=<%= id %> class=<%= [class] %> style=<%= style %>>
                   <%= content %>
               </p>
    End Function

    Public Shared Function div(content As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <div id=<%= id %> class=<%= [class] %> style=<%= style %>>
                   <%= content %>
               </div>
    End Function
End Class
