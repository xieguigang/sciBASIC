Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel



Namespace Serialization

    ''' <summary>
    ''' 数据类型转换方法的句柄对象
    ''' </summary>
    ''' <param name="data">源之中的数据，由于源是一个TEXT格式的数据文件，故而这里的数据类型为字符串，通过本句柄对象可以将字符串数据映射为其他的复杂数据类型</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Delegate Function __StringTypeCaster(data As String) As Object
    Public Delegate Function __LDMStringTypeCastHandler(data As Object) As String

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
        ''' <typeparam name="T_Entity">数据模型</typeparam>
        ''' <typeparam name="T_Mapping">源</typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadMapping(Of T_Entity As Class, T_Mapping As Class)(source As T_Mapping) As T_Entity
            Dim Mappings = GetNodeMapping(Of T_Entity, T_Mapping)(source)
            Dim DataModel As T_Entity = Activator.CreateInstance(Of T_Entity)()

            For Each Node In Mappings '读取数据
                Dim value As Object = Node.Source.GetValue(source)
                Dim str As String = DataFramework.__toStringInternal(value)
                value = Node.SourceToMappingCasting(str)
                Call Node.Mapping.SetValue(DataModel, value)
            Next

            Return DataModel
        End Function

        Public Function WriteMapping(Of T_Entity As Class, T_Mapping As Class)(Model As T_Entity, ByRef WriteToSource As T_Mapping) As T_Mapping
            Dim Mappings = GetNodeMapping(Of T_Entity, T_Mapping)(Nothing)

            For Each Node In Mappings '写数据
                If Node.MappingToSourceCasting Is Nothing Then
                    Continue For
                End If

                Dim value As Object = Node.Mapping.GetValue(Model)
                Dim str As String = Node.MappingToSourceCasting(value)
                Call Node.Source.SetValue(WriteToSource, str)
            Next

            Return WriteToSource
        End Function

        ''' <summary>
        ''' 从数据模型将值类型数据映射回源，以将配置数据写入文件
        ''' </summary>
        ''' <typeparam name="T_Entity">数据模型</typeparam>
        ''' <typeparam name="T_Mapping">源</typeparam>
        ''' <param name="Model"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function WriteMapping(Of T_Entity As Class, T_Mapping As Class)(Model As T_Entity) As T_Mapping
            Dim Source As T_Mapping = Activator.CreateInstance(Of T_Mapping)()
            Return WriteMapping(Of T_Entity, T_Mapping)(Model, WriteToSource:=Source)
        End Function

        Private Function __knowsIsIgnored(p As PropertyInfo) As Boolean
            Dim c_attrs As Object() = p.GetCustomAttributes(attributeType:=GetType(MappingsIgnored), inherit:=False)
            Return Not c_attrs.IsNullOrEmpty
        End Function

        ''' <summary>
        ''' 获取从源映射至数据模型的映射过程
        ''' </summary>
        ''' <typeparam name="T_Entity">数据模型</typeparam>
        ''' <typeparam name="T_Mapping">源</typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNodeMapping(Of T_Entity As Class, T_Mapping As Class)(obj_source As Object) As NodeMapping()
            Dim LQuery As PropertyInfo() = (From p In GetType(T_Mapping).GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                                            Where Not __knowsIsIgnored(p) AndAlso
                                           DataFramework.PrimitiveFromString.ContainsKey(p.PropertyType)
                                            Select p).ToArray  '获取所有的数据源之中的映射
            Dim T_EntityType As Type = GetType(T_Entity)
            Dim CustomMappings As MethodInfo() = (From entry In GetType(T_Mapping).GetMethods()
                                                  Where entry.ReturnType <> GetType(System.Void) AndAlso
                                                  entry.GetParameters.Length = 1
                                                  Select entry).ToArray
            Dim Mappings = (From p As PropertyInfo
                        In T_EntityType.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                            Let p_Collection = (From prop As PropertyInfo In LQuery
                                                Where String.Equals(prop.Name, p.Name, StringComparison.OrdinalIgnoreCase)
                                                Select prop).FirstOrDefault
                            Where Not p_Collection Is Nothing AndAlso
                            Not __knowsIsIgnored(p)
                            Select source = p_Collection,
                            MappingToModel = p).ToArray          '获取数据模型之中的同名的映射属性
            Dim Mappings_LQuery = (From mapping In Mappings
                                   Let sourceMapping = __getReads_MappingHandle(mapping.source, mapping.MappingToModel, CustomMappings, obj_source)
                                   Let model2Mapping = __getWrite_MappingHandle(mapping.source, mapping.MappingToModel, CustomMappings)
                                   Select mapping.source, mapping.MappingToModel, sourceMapping, model2Mapping).ToArray    '获取具体的映射过程
            Return (From map In Mappings_LQuery
                    Where Not (map.sourceMapping Is Nothing)
                    Let nodeMap As NodeMapping = New NodeMapping With {
                    .Source = map.source,
                    .Mapping = map.MappingToModel,
                    .SourceToMappingCasting = map.sourceMapping,
                    .MappingToSourceCasting = map.model2Mapping
                }
                    Select nodeMap).ToArray      '返回映射句柄，为了简化程序设计，数据模型至源文件的映射可以不必定义。但是当需要使用本模块进行配置文件的写操作的时候，映射至源文件的方法则非常有必要要进行定义了
        End Function

        Private Function __getWrite_MappingHandle(source As PropertyInfo, Model As PropertyInfo, Methods As MethodInfo()) As __LDMStringTypeCastHandler
            If DataFramework.ToStrings.ContainsKey(Model.PropertyType) Then
                Return DataFramework.ToStrings(Model.PropertyType)
            Else
                Dim Method = __getCustomMapping(p_Type:=Model.PropertyType, ReturnedType:=source.PropertyType, Methods:=Methods)
                Return Function(obj As Object) DirectCast(Method.Invoke(Nothing, {obj}), String)
            End If
        End Function

        Private Function __getReads_MappingHandle(source As PropertyInfo, Model As PropertyInfo, Methods As MethodInfo(), obj_source As Object) As __StringTypeCaster
            If DataFramework.PrimitiveFromString.ContainsKey(Model.PropertyType) Then
                Return DataFramework.PrimitiveFromString(Model.PropertyType)
            Else
                Dim Method = __getCustomMapping(p_Type:=source.PropertyType, ReturnedType:=Model.PropertyType, Methods:=Methods)
                Return Function(s As String) Method.Invoke(obj_source, {s})
            End If
        End Function

        Private Function __getCustomMapping(p_Type As Type, ReturnedType As Type, Methods As MethodInfo()) As MethodInfo
            Dim LQuery = (From entryPoint As MethodInfo In Methods
                          Where entryPoint.GetParameters.First.ParameterType = p_Type AndAlso
                          entryPoint.ReturnType = ReturnedType
                          Select entryPoint).FirstOrDefault
            Return LQuery
        End Function
    End Module
#End If
End Namespace

