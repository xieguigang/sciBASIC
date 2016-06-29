#Region "Microsoft.VisualBasic::6f5e40c9b3247a80a657d16bfb951581, ..\RDF\TestProject\Test1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

Imports Microsoft.VisualBasic.DocumentFormat.RDF
Imports Microsoft.VisualBasic.DocumentFormat.RDF.Serialization

<RDFNamespaceImports("cd", "http://www.recshop.fake/cd#")>
Public Class RDFD

    <RDFElement("cd")> Public Property CDList As CD()

    <RDFDescription(About:="http://www.recshop.fake/cd/Empire Burlesque")>
    <RDFType("cd")>
    Public Class CD : Inherits RDFEntity
        <RDFElement("artist")> Public Property Artist As String
        <RDFElement("country")> Public Property Country As String
        <RDFElement("company")> Public Property Company As String
        <RDFElement("price")> Public Property Price As String
        <RDFElement("year")> Public Property Year As String

        <RDFIgnore> Public Property IgnoredProperty As KeyValuePair(Of Integer, String)
    End Class
End Class

