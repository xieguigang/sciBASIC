Imports System.Xml.Serialization

Namespace Net.Protocols.Streams.Array

    Public MustInherit Class ArrayAbstract(Of T) : Inherits RawStream

        <XmlAttribute("T")>
        Public Overridable Property Values As T()

        Protected ReadOnly __serialization As Func(Of T, Byte())
        Protected ReadOnly __deserialization As Func(Of Byte(), T)

        ''' <summary>
        ''' 由于这个模块是专门应用于服务器端的数据交换的模块，所以稳定性优先，
        ''' 这里面的函数都是安全的数组访问方法
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Property value(index As Integer) As T
            Get
                Return Values.Get(index)
            End Get
            Set(value As T)
                Call Values.Set(index, value)
            End Set
        End Property

        Sub New(serialize As Func(Of T, Byte()), load As Func(Of Byte(), T))
            __serialization = serialize
            __deserialization = load
        End Sub
    End Class
End Namespace