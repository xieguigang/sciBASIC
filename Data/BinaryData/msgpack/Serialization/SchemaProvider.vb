Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Serialization

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' 这个模块是为了处理元素类型定义信息和序列化代码调用模块之间没有实际的引用关系的情况
    ''' 例如模块A没有引用messagepack模块，则没有办法添加<see cref="MessagePackMemberAttribute"/>
    ''' 来完成序列化，则这个时候会需要通过这个模块来提供这样的映射关系
    ''' </remarks>
    Public MustInherit Class SchemaProvider(Of T)

        Shared ReadOnly slotList As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of T)(
            flag:=PropertyAccess.ReadWrite,
            nonIndex:=True,
            primitive:=False,
            binds:=PublicProperty
        )

        ''' <summary>
        ''' provides a schema table for base object for generates 
        ''' a sequence of <see cref="MessagePackMemberAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        Protected Friend MustOverride Iterator Function GetObjectSchema() As IEnumerable(Of (obj As Type, schema As Dictionary(Of String, NilImplication)))

    End Class
End Namespace