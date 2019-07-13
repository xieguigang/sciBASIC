#Region "Microsoft.VisualBasic::294d3d421637bd5c010bdb8742d4cec3, Microsoft.VisualBasic.Core\Net\Protocol\Streams\VarArray.vb"

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

    '     Class VarArray
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Buffer = System.Array

Namespace Net.Protocols.Streams.Array

    ''' <summary>
    ''' The bytes length of the element in thee source sequence is not fixed.
    ''' (序列里面的元素的长度是不固定的)
    ''' </summary>
    Public Class VarArray(Of T) : Inherits ArrayAbstract(Of T)

        Sub New(TSerialize As IGetBuffer(Of T), load As IGetObject(Of T))
            Call MyBase.New(TSerialize, load)
        End Sub

        Sub New(raw As Byte(), serilize As IGetBuffer(Of T), load As IGetObject(Of T))
            Call Me.New(serilize, load)

            Dim lb As Byte() = New Byte(INT64 - 1) {}
            Dim buf As Byte()
            Dim i As New Pointer
            Dim list As New List(Of T)
            Dim l As Long
            Dim x As T

            Do While raw.Length > i

                Call Buffer.ConstrainedCopy(raw, i << INT64, lb, Scan0, INT64)

                l = BitConverter.ToInt64(lb, Scan0)
                buf = New Byte(l - 1) {}

                Call Buffer.ConstrainedCopy(raw, i << buf.Length, buf, Scan0, buf.Length)

                x = load(buf)
                list += x
            Loop

            Values = list.ToArray
        End Sub

        ''' <summary>
        ''' Long + T + Long + T
        ''' 其中Long是一个8字节长度的数组，用来指示T的长度
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Serialize() As Byte()
            Dim list As New List(Of Byte)
            Dim LQuery = From index As SeqValue(Of T)
                         In Values.SeqIterator.AsParallel
                         Select buf = New SeqValue(Of Byte()) With {
                             .i = index.i,
                             .value = serialization(index.value)
                         }
                         Order By buf.i Ascending

            For Each x As SeqValue(Of Byte()) In LQuery
                Dim byts As Byte() = x.value
                Dim l As Long = byts.Length
                Dim lb As Byte() = BitConverter.GetBytes(l)

                Call list.AddRange(lb)
                Call list.AddRange(byts)
            Next

            Return list.ToArray
        End Function
    End Class
End Namespace
