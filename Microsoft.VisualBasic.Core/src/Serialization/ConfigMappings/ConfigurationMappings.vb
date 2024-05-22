#Region "Microsoft.VisualBasic::5ecd2229c267c6f208a59d6c70edc280, Microsoft.VisualBasic.Core\src\Serialization\ConfigMappings\ConfigurationMappings.vb"

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

    '   Total Lines: 168
    '    Code Lines: 122 (72.62%)
    ' Comment Lines: 27 (16.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (11.31%)
    '     File Size: 8.59 KB


    '     Module ConfigurationMappings
    ' 
    '         Function: __getCustomMapping, __getReads_MappingHandle, __getWrite_MappingHandle, __knowsIsIgnored, GetNodeMapping
    '                   LoadMapping, (+2 Overloads) WriteMapping
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Serialization

#If NET_40 = 0 Then

    ''' <summary>
    ''' 最基本的思想是将属性值按照同名属性名称在A和B两个对象类型之间进行映射，即A与B两个对象之间必须要具备相同的属性名称，才可以产生映射，请注意在本对象之中仅能够映射最基本的值类型的数据类型
    ''' 对于一些自定义的映射操作，请在目标数据模型之中定义自定义的映射函数，要求为函数只有一个参数，参数类型和返回值类型分别为映射的两个节点的数据类型，程序会使用反射自动查找
    ''' </summary>
    ''' <remarks></remarks>
    Public Module ConfigurationMappings

        ''' <summary>
        ''' 从源江基本的值类型映射到数据模型，以将配置数据读取出来并进行加载
        ''' </summary>
        ''' <typeparam name="T">数据模型</typeparam>
        ''' <typeparam name="TMaps">源</typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadMapping(Of T As Class, TMaps As Class)(source As TMaps) As T
            Dim Mappings = GetNodeMapping(Of T, TMaps)(source)
            Dim DataModel As T = Activator.CreateInstance(Of T)()

            For Each Node In Mappings '读取数据
                Dim value As Object = Node.Source.GetValue(source)
                Dim str As String = CStrSafe(value, "")
                value = Node.SourceToMappingCasting(str)
                Call Node.Mapping.SetValue(DataModel, value)
            Next

            Return DataModel
        End Function

        Public Function WriteMapping(Of T As Class, TMaps As Class)(model As T, ByRef WriteToSource As TMaps) As TMaps
            Dim Mappings = GetNodeMapping(Of T, TMaps)(Nothing)

            For Each Node In Mappings '写数据
                If Node.MappingToSourceCasting Is Nothing Then
                    Continue For
                End If

                Dim value As Object = Node.Mapping.GetValue(model)
                Dim str As String = Node.MappingToSourceCasting(value)
                Call Node.Source.SetValue(WriteToSource, str)
            Next

            Return WriteToSource
        End Function

        ''' <summary>
        ''' 从数据模型将值类型数据映射回源，以将配置数据写入文件
        ''' </summary>
        ''' <typeparam name="T">数据模型</typeparam>
        ''' <typeparam name="TMaps">源</typeparam>
        ''' <param name="Model"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function WriteMapping(Of T As Class, TMaps As Class)(Model As T) As TMaps
            Dim Source As TMaps = Activator.CreateInstance(Of TMaps)()
            Return WriteMapping(Of T, TMaps)(Model, WriteToSource:=Source)
        End Function

        Private Function __knowsIsIgnored(p As PropertyInfo) As Boolean
            Dim c_attrs As Object() = p.GetCustomAttributes(attributeType:=GetType(MappingsIgnored), inherit:=False)
            Return Not c_attrs.IsNullOrEmpty
        End Function

        ''' <summary>
        ''' 获取从源映射至数据模型的映射过程
        ''' </summary>
        ''' <typeparam name="T">数据模型</typeparam>
        ''' <typeparam name="TMaps">源</typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNodeMapping(Of T As Class, TMaps As Class)(obj_source As Object) As NodeMapping()
            Dim LQuery As PropertyInfo() = LinqAPI.Exec(Of PropertyInfo) <=
                From p As PropertyInfo
                In GetType(TMaps).GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                Where Not __knowsIsIgnored(p) AndAlso
                    DataFramework.StringParsers.ContainsKey(p.PropertyType)
                Select p '获取所有的数据源之中的映射
            Dim T_EntityType As Type = GetType(T)
            Dim CustomMappings As MethodInfo() = LinqAPI.Exec(Of MethodInfo) <=
                From entry As MethodInfo
                In GetType(TMaps).GetMethods()
                Where entry.ReturnType <> GetType(System.Void) AndAlso
                    entry.GetParameters.Length = 1
                Select entry

            Dim Mappings = From p As PropertyInfo
                           In T_EntityType.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                           Let array As PropertyInfo =
                               LQuery.Where(Function(prop) String.Equals(prop.Name, p.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault
                           Where Not array Is Nothing AndAlso
                               Not __knowsIsIgnored(p)
                           Select source = array,
                               MappingToModel = p         ' 获取数据模型之中的同名的映射属性
            Dim out = From mapping
                      In Mappings
                      Let sourceMapping = __getReads_MappingHandle(mapping.source, mapping.MappingToModel, CustomMappings, obj_source)
                      Let model2Mapping = __getWrite_MappingHandle(mapping.source, mapping.MappingToModel, CustomMappings)
                      Select mapping.source,
                          mapping.MappingToModel,
                          sourceMapping,
                          model2Mapping    ' 获取具体的映射过程

            Return LinqAPI.Exec(Of NodeMapping) <=
 _
                From map
                In out
                Where Not (map.sourceMapping Is Nothing)
                Let nodeMap As NodeMapping = New NodeMapping With {
                    .Source = map.source,
                    .Mapping = map.MappingToModel,
                    .SourceToMappingCasting = map.sourceMapping,
                    .MappingToSourceCasting = map.model2Mapping
                }
                Select nodeMap     ' 返回映射句柄，为了简化程序设计，数据模型至源文件的映射可以不必定义。但是当需要使用本模块进行配置文件的写操作的时候，映射至源文件的方法则非常有必要要进行定义了
        End Function

        Private Function __getWrite_MappingHandle(source As PropertyInfo, Model As PropertyInfo, Methods As MethodInfo()) As IStringBuilder
            If DataFramework.StringBuilders.ContainsKey(Model.PropertyType) Then
                Return DataFramework.StringBuilders(Model.PropertyType)
            Else
                Dim Method As MethodInfo = __getCustomMapping(
                    p_Type:=Model.PropertyType,
                    ReturnedType:=source.PropertyType,
                    Methods:=Methods)
                Return Function(obj As Object) DirectCast(Method.Invoke(Nothing, {obj}), String)
            End If
        End Function

        Private Function __getReads_MappingHandle(source As PropertyInfo, Model As PropertyInfo, Methods As MethodInfo(), obj_source As Object) As IStringParser
            If DataFramework.StringParsers.ContainsKey(Model.PropertyType) Then
                Return DataFramework.StringParsers(Model.PropertyType)
            Else
                Dim method As MethodInfo = __getCustomMapping(
                    p_Type:=source.PropertyType,
                    ReturnedType:=Model.PropertyType,
                    Methods:=Methods)
#If DEBUG Then
                If method Is Nothing Then
                    Call $"{source.Name} --> {Model.Name} is incomplete!".Warning
                End If
#End If
                Return Function(s$)
                           Return method.Invoke(obj_source, {s})
                       End Function
            End If
        End Function

        Private Function __getCustomMapping(p_Type As Type, ReturnedType As Type, Methods As MethodInfo()) As MethodInfo
            Dim LQuery As MethodInfo = LinqAPI.DefaultFirst(Of MethodInfo) <=
                From entryPoint As MethodInfo
                In Methods
                Where entryPoint.GetParameters.First.ParameterType = p_Type AndAlso
                    entryPoint.ReturnType = ReturnedType
                Select entryPoint

            Return LQuery
        End Function
    End Module
#End If
End Namespace
