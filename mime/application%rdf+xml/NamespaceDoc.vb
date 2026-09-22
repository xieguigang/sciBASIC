#Region "Microsoft.VisualBasic::b4a92cf69baaf59a2d9b0173402ef3b2, mime\application%rdf+xml\NamespaceDoc.vb"

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

    '   Total Lines: 14
    '    Code Lines: 2 (14.29%)
    ' Comment Lines: 12 (85.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 656 B


    ' Module NamespaceDoc
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' RDF/XML serialization models for the ``application/rdf+xml`` MIME type: RDF description, bag
''' and DCMI metadata objects that can be read from and written to RDF/XML documents.
''' </summary>
''' <remarks>
''' 在进行RDF反序列化读取操作的时候似乎存在一个BUG
''' 不可以将元素的命名空间设置为RDF的命名空间，即元素
''' 的命名空间不应该和根元素的命名空间保持一致，否则
''' 无法读取出注释数据
''' 所以这个也是在当前模块之中将元素类型的命名空间设置
''' 为``NA``字符串值的原因
''' </remarks>
Module NamespaceDoc
End Module
