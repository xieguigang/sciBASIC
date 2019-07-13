#Region "Microsoft.VisualBasic::b7ceedf73869affc6ffd0357bff5e945, Microsoft.VisualBasic.Core\Net\Protocol\Streams\ArrayBase.vb"

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

    '     Class ValueArray
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Buffer = System.Array

Namespace Net.Protocols.Streams.Array

    ''' <summary>
    ''' 对于<see cref="System.Int64"/>, <see cref="System.int32"/>, <see cref="System.Double"/>, <see cref="System.DateTime"/>
    ''' 这些类型的数据来说，进行网络传输的时候使用json会被转换为字符串，数据量比较大，而转换为字节再进行传输，数据流量的消耗会比较小
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>这个是定长的数组序列</remarks>
    Public MustInherit Class ValueArray(Of T) : Inherits ArrayAbstract(Of T)

        Protected ReadOnly _bufWidth As Integer

        Protected Sub New(serialization As IGetBuffer(Of T),
                          deserialization As IGetObject(Of T),
                          bufWidth As Integer,
                          rawStream As Byte())

            Call MyBase.New(serialization, deserialization)

            _bufWidth = bufWidth

            If Not rawStream.IsNullOrEmpty Then
                Dim valueList As New List(Of T)
                Dim p As VBInteger = 0
                Dim byts As Byte() = New Byte(_bufWidth - 1) {}

                Do While p < rawStream.Length - 1
                    Call Buffer.ConstrainedCopy(rawStream, p + bufWidth, byts, Scan0, bufWidth)
                    Call valueList.Add(MyBase.deserialization(byts))
                Loop

                Values = valueList.ToArray
            End If
        End Sub

        Public NotOverridable Overrides Function Serialize() As Byte()
            Dim bufferArray As Byte() = New Byte(Values.Length * _bufWidth - 1) {}
            Dim p As VBInteger = 0

            For Each value As T In Values
                Dim byts As Byte() = serialization(value)
                Call Buffer.ConstrainedCopy(byts, Scan0, bufferArray, p + _bufWidth, _bufWidth)
            Next

            Return bufferArray
        End Function

        Public Overrides Function ToString() As String
            If Values.IsNullOrEmpty Then
                Return GetType(T).FullName
            Else
                Dim valJson$ = Values _
                    .Select(Function(val) Scripting.ToString(val)) _
                    .GetJson
                Return $"{GetType(T).FullName} {valJson}"
            End If
        End Function
    End Class
End Namespace
