#Region "Microsoft.VisualBasic::d0ec7d5f1bd886f6b077a0a036dcded0, mime\application%rdf+xml\DataModel\DCMI.vb"

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

    '   Total Lines: 90
    '    Code Lines: 17
    ' Comment Lines: 72
    '   Blank Lines: 1
    '     File Size: 3.02 KB


    ' Class DCMI
    ' 
    '     Properties: [Date], Contributor, Coverage, Creator, Description
    '                 Format, Identifier, Language, Publisher, Relation
    '                 Rights, Source, Subject, Title, Type
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' RDF 都柏林核心元数据倡议
''' (http://w3school.com.cn/rdf/rdf_dublin.asp)
''' </summary>
''' <remarks>
''' 都柏林核心元数据倡议 (DCMI) 已创建了一些供描述文档的预定义属性。
''' 
''' Dublin 核心
''' RDF 是元数据（关于数据的数据）。RDF 被用于描述信息资源。
''' 都柏林核心是一套供描述文档的预定义属性。
''' 第一份都柏林核心属性是于1995年 在俄亥俄州的都柏林的元数据工作组被定义的，目前由都柏林元数据倡议来维护。
''' </remarks>
Public Class DCMI : Inherits RDFEntity

    ''' <summary>
    ''' 一个负责为资源内容作出贡献的实体(如作者)。
    ''' </summary>
    ''' <returns></returns>
    Public Property Contributor As String
    ''' <summary>
    ''' 资源内容的氛围或作用域
    ''' </summary>
    ''' <returns></returns>
    Public Property Coverage As String
    ''' <summary>
    ''' 一个主要负责创建资源内容的实体。
    ''' </summary>
    ''' <returns></returns>
    Public Property Creator As String
    ''' <summary>
    ''' 物理或数字的资源表现形式。
    ''' </summary>
    ''' <returns></returns>
    Public Property Format As String
    ''' <summary>
    ''' 在资源生命周期中某事件的日期。
    ''' </summary>
    ''' <returns></returns>
    Public Property [Date] As String
    ''' <summary>
    ''' 对资源内容的说明。
    ''' </summary>
    ''' <returns></returns>
    Public Property Description As String
    ''' <summary>
    ''' 一个对在给定上下文中的资源的明确引用
    ''' </summary>
    ''' <returns></returns>
    Public Property Identifier As String
    ''' <summary>
    ''' 资源智力内容所用的语言。
    ''' </summary>
    ''' <returns></returns>
    Public Property Language As String
    ''' <summary>
    ''' 一个负责使得资源内容可用的实体
    ''' </summary>
    ''' <returns></returns>
    Public Property Publisher As String
    ''' <summary>
    ''' 一个对某个相关资源的引用
    ''' </summary>
    ''' <returns></returns>
    Public Property Relation As String
    ''' <summary>
    ''' 有关保留在资源之内和之上的权利的信息
    ''' </summary>
    ''' <returns></returns>
    Public Property Rights As String
    ''' <summary>
    ''' 一个对作为目前资源的来源的资源引用。
    ''' </summary>
    ''' <returns></returns>
    Public Property Source As String
    ''' <summary>
    ''' 一个资源内容的主题
    ''' </summary>
    ''' <returns></returns>
    Public Property Subject As String
    ''' <summary>
    ''' 一个给资源起的名称
    ''' </summary>
    ''' <returns></returns>
    Public Property Title As String
    ''' <summary>
    ''' 资源内容的种类或类型。
    ''' </summary>
    ''' <returns></returns>
    Public Property Type As String
End Class
