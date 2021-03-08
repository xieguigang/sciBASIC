Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Serialization

    Public Interface ISchemaProvider
        Function GetMembers() As IEnumerable(Of BindProperty(Of MessagePackMemberAttribute))
    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' 这个模块是为了处理元素类型定义信息和序列化代码调用模块之间没有实际的引用关系的情况
    ''' 例如模块A没有引用messagepack模块，则没有办法添加<see cref="MessagePackMemberAttribute"/>
    ''' 来完成序列化，则这个时候会需要通过这个模块来提供这样的映射关系
    ''' </remarks>
    Public MustInherit Class SchemaProvider(Of T) : Implements ISchemaProvider

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
        Protected Friend MustOverride Function GetObjectSchema() As Dictionary(Of String, NilImplication)

        Public Iterator Function GetMembers() As IEnumerable(Of BindProperty(Of MessagePackMemberAttribute)) Implements ISchemaProvider.GetMembers
            For Each item In GetObjectSchema()
                If Not slotList.ContainsKey(item.Key) Then
                    Throw New NotImplementedException($"invalid member name: {item.Key}!")
                End If

                Dim attr As New MessagePackMemberAttribute(item.Value)
                Dim bind As New BindProperty(Of MessagePackMemberAttribute)(attr, slotList(item.Key))

                Yield bind
            Next
        End Function

    End Class
End Namespace