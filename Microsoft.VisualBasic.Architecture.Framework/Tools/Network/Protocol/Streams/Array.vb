Namespace Net.Protocol.Streams.Array

    ''' <summary>
    ''' 对于<see cref="System.Int64"/>, <see cref="System.int32"/>, <see cref="System.Double"/>, <see cref="System.DateTime"/>
    ''' 这些类型的数据来说，进行网络传输的时候使用json会被转换为字符串，数据量比较大，而转换为字节再进行传输，数据流量的消耗会比较小
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class ValueArray(Of T) : Inherits RawStream

        <Xml.Serialization.XmlAttribute("T")>
        Public Property Values As T()

        Protected ReadOnly __serialization As Func(Of T, Byte())
        Protected ReadOnly __deserialization As Func(Of Byte(), T)
        Protected ReadOnly _bufWidth As Integer

        Protected Sub New(serialization As Func(Of T, Byte()),
                          deserialization As Func(Of Byte(), T),
                          bufWidth As Integer,
                          rawStream As Byte())
            __serialization = serialization
            __deserialization = deserialization
            _bufWidth = bufWidth

            If Not rawStream.IsNullOrEmpty Then
                Dim valueList As New List(Of T)
                Dim p As Integer = 0
                Dim byts As Byte() = New Byte(_bufWidth - 1) {}

                Do While p < rawStream.Length - 1
                    Call System.Array.ConstrainedCopy(rawStream, p.Move(bufWidth), byts, Scan0, bufWidth)
                    Call valueList.Add(__deserialization(byts))
                Loop

                Values = valueList.ToArray
            End If
        End Sub

        Public NotOverridable Overrides Function Serialize() As Byte()
            Dim ChunkBuffer As Byte() = New Byte(Values.Length * _bufWidth - 1) {}
            Dim p As Integer = 0
            For Each value As T In Values
                Dim byts As Byte() = __serialization(value)
                Call System.Array.ConstrainedCopy(byts, Scan0, ChunkBuffer, p.Move(_bufWidth), _bufWidth)
            Next
            Return ChunkBuffer
        End Function

        Public Overrides Function ToString() As String
            If Values.IsNullOrEmpty Then
                Return GetType(T).FullName
            Else
                Return $"{GetType(T).FullName}  {"{"}{String.Join("," & vbTab, Values.ToArray(Of String)(Function(val) Scripting.ToString(val)))}{"}"}"
            End If
        End Function

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

    End Class

    Public Class [Long] : Inherits ValueArray(Of Long)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt64, INT64, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt64, INT64, rawStream)
        End Sub

        Sub New(array As IEnumerable(Of Long))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toInt64(byts As Byte()) As Long
            Return BitConverter.ToInt64(byts, Scan0)
        End Function
    End Class

    Public Class [Integer] : Inherits ValueArray(Of Integer)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt32, INT32, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toInt32, INT32, rawStream)
        End Sub

        Sub New(array As IEnumerable(Of Integer))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toInt32(byts As Byte()) As Integer
            Return BitConverter.ToInt32(byts, Scan0)
        End Function
    End Class

    Public Class [Double] : Inherits ValueArray(Of Double)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toFloat, DblFloat, Nothing)
        End Sub

        Sub New(array As IEnumerable(Of Double))
            Call Me.New
            Values = array.ToArray
        End Sub

        Sub New(rawStream As Byte())
            Call MyBase.New(AddressOf BitConverter.GetBytes, AddressOf __toFloat, DblFloat, rawStream)
        End Sub

        Private Shared Function __toFloat(byts As Byte()) As Double
            Return BitConverter.ToDouble(byts, Scan0)
        End Function
    End Class

    Public Class [Boolean] : Inherits ValueArray(Of Boolean)

        Sub New()
            Call MyBase.New(AddressOf BitConverter.GetBytes, Nothing, 1, Nothing)
        End Sub

        Sub New(rawStream As Byte())
            Call Me.New

            If Not rawStream.IsNullOrEmpty Then
                Me.Values = rawStream.ToArray(Function(byt) __toBoolean(byt))
            Else
                Me.Values = New Boolean() {}
            End If
        End Sub

        Sub New(array As IEnumerable(Of Boolean))
            Call Me.New
            Values = array.ToArray
        End Sub

        Private Shared Function __toBoolean(byt As Byte) As Boolean
            If byt = 0 Then
                Return False
            Else
                Return True
            End If
        End Function
    End Class

End Namespace