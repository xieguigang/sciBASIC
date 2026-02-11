#Region "Microsoft.VisualBasic::d33645e303b7e40e9ba56b70bff2d6d1, Microsoft.VisualBasic.Core\src\Serialization\JSON\JsonIgnoreAttribute.vb"

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

    '   Total Lines: 10
    '    Code Lines: 6 (60.00%)
    ' Comment Lines: 1 (10.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (30.00%)
    '     File Size: 225 B


    ' Class JsonIgnoreAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON

    ' fix for the missing attribute from .NET core System.Text.Json.Serialization

#If NET48 Then
Public Class JsonIgnoreAttribute : Inherits Attribute
End Class
#End If

End Namespace

