Imports System.Text

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class MetaAttribute : Inherits Attribute
        Implements Reflection.IAttributeComponent

        ''' <summary>
        ''' The value type of the value slot in the meta attribute dictionary.(被序列化的对象之中的元数据的字典的值的类型)
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property TypeId As System.Type

        Public ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.MetaAttribute
            End Get
        End Property

        ''' <summary>
        ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
        ''' </summary>
        ''' <param name="Type">The value type of the value slot in the meta attribute dictionary.(被序列化的对象之中的元数据的字典的值的类型)</param>
        Sub New(Optional Type As System.Type = Nothing)
            TypeId = If(Type Is Nothing, GetType(String), Type)
        End Sub

        Public Overrides Function ToString() As String
            Return TypeId.FullName
        End Function
    End Class
End Namespace