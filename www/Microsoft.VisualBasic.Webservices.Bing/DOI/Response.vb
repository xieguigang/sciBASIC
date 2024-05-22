#Region "Microsoft.VisualBasic::ff15ebdde82f1587fed23a6797692799, www\Microsoft.VisualBasic.Webservices.Bing\DOI\Response.vb"

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

    '   Total Lines: 28
    '    Code Lines: 24 (85.71%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (14.29%)
    '     File Size: 796 B


    '     Class Response
    ' 
    '         Properties: handle, message, responseCode, values
    ' 
    '     Class DoiValue
    ' 
    '         Properties: data, index, timestamp, ttl, type
    ' 
    '     Class DataValue
    ' 
    '         Properties: format, value
    ' 
    '     Class HS_ADMIN
    ' 
    '         Properties: handle, index, permissions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DOI

    Public Class Response
        Public Property responseCode As Integer
        Public Property handle As String
        Public Property values As DoiValue()
        Public Property message As String
    End Class

    Public Class DoiValue
        Public Property index As Integer
        Public Property type As String
        Public Property data
        Public Property ttl As Integer
        Public Property timestamp As String
    End Class

    Public Class DataValue
        Public Property format As String
        Public Property value As Object
    End Class

    Public Class HS_ADMIN
        Public Property handle As String
        Public Property index As Integer
        Public Property permissions As String
    End Class
End Namespace
