#Region "Microsoft.VisualBasic::45cc48e2191edd6ac99252fdb21ba90f, ..\sciBASIC#\mime\text%html\HTML\CSS\CSSLayer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Xml.Serialization

Namespace HTML.CSS

    Public Interface CSSLayer

        ''' <summary>
        ''' Drawing order, if this index value is greater, then it will be draw on the top most.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("z-index")>
        Property zIndex As Integer

    End Interface
End Namespace
