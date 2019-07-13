#Region "Microsoft.VisualBasic::caea15ee86f20fc11ef8717b7b145706, Microsoft.VisualBasic.Core\Net\Protocol\Streams\ArrayAbstract.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class ArrayAbstract
    ' 
    '         Properties: Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace Net.Protocols.Streams.Array

    Public MustInherit Class ArrayAbstract(Of T) : Inherits RawStream

        <XmlAttribute("T")>
        Public Overridable Property Values As T()

        Protected ReadOnly serialization As IGetBuffer(Of T)
        Protected ReadOnly deserialization As IGetObject(Of T)

        ''' <summary>
        ''' 由于这个模块是专门应用于服务器端的数据交换的模块，所以稳定性优先，
        ''' 这里面的函数都是安全的数组访问方法
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Property value(index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Values.ElementAtOrDefault(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                Call Values.Set(index, value)
            End Set
        End Property

        Sub New(serialize As IGetBuffer(Of T), load As IGetObject(Of T))
            serialization = serialize
            deserialization = load
        End Sub
    End Class
End Namespace
