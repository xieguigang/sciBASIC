Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Xml.Serialization

Namespace Net.Protocols.Streams.Array

    ''' <summary>
    ''' 对于<see cref="System.Int64"/>, <see cref="System.int32"/>, <see cref="System.Double"/>, <see cref="System.DateTime"/>
    ''' 这些类型的数据来说，进行网络传输的时候使用json会被转换为字符串，数据量比较大，而转换为字节再进行传输，数据流量的消耗会比较小
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>这个是定长的数组序列</remarks>
    Public MustInherit Class ValueArray(Of T) : Inherits ArrayAbstract(Of T)

        Protected ReadOnly _bufWidth As Integer

        Protected Sub New(serialization As Func(Of T, Byte()),
                          deserialization As Func(Of Byte(), T),
                          bufWidth As Integer,
                          rawStream As Byte())

            Call MyBase.New(serialization, deserialization)

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
    End Class
End Namespace