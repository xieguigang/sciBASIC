#Region "Microsoft.VisualBasic::90e4ab445881618c1e1e862acc3d84d5, mime\application%rdf+xml\RDF.vb"

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

    '   Total Lines: 29
    '    Code Lines: 11
    ' Comment Lines: 14
    '   Blank Lines: 4
    '     File Size: 1015 B


    ' Class RDF
    ' 
    '     Properties: description
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

''' <summary>
''' the xml file serilization model
''' </summary>
''' <remarks>
''' 关于RDF模型对象的使用方法，在这里提供两两个抽象对象用来表示RDF模型
''' 
''' 1. description数据是对象的注释信息存储位置
''' 2. 这个RDF抽象类型为注释信息的存储容器
''' 
''' 因为不同的应用程序会产生不同的注释信息数据，所以需要继承所提供的
''' description对象以及继承当前的RDF对象来生成一个特定的数据读取对象
''' 后进行信息数据的读取操作。
''' </remarks>
''' 
<XmlType("RDF", [Namespace]:=RDFEntity.XmlnsNamespace)>
Public MustInherit Class RDF(Of T As Description)

    <XmlNamespaceDeclarations()>
    Public xmlns As New XmlSerializerNamespaces

    <XmlElement("Description", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property description As T

    Sub New()
        xmlns.Add("rdf", RDFEntity.XmlnsNamespace)
    End Sub
End Class
