Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Linq

Namespace Net.Protocols.Streams.Array

    ''' <summary>
    ''' 里面的元素的长度是不固定的
    ''' </summary>
    Public Class VarArray(Of T) : Inherits ArrayAbstract(Of T)

        Sub New(TSerialize As Func(Of T, Byte()), load As Func(Of Byte(), T))
            Call MyBase.New(TSerialize, load)
        End Sub

        Sub New(raw As Byte(),
                serilize As Func(Of T, Byte()),
                load As Func(Of Byte(), T))
            Call Me.New(serilize, load)

            Dim lb As Byte() = New Byte(INT64 - 1) {}
            Dim buf As Byte()
            Dim i As Pointer = New Pointer
            Dim list As New List(Of T)
            Dim l As Long
            Dim x As T

            Do While raw.Length > i

                Call System.Array.ConstrainedCopy(raw, i << INT64, lb, Scan0, INT64)

                l = BitConverter.ToInt64(lb, Scan0)
                buf = New Byte(l - 1) {}

                Call System.Array.ConstrainedCopy(raw, i << buf.Length, buf, Scan0, buf.Length)

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
            Dim LQuery = (From ind As SeqValue(Of T)
                          In Values.SeqIterator.AsParallel
                          Select ind.Pos, byts = __serialization(ind.obj)
                          Order By Pos Ascending)

            For Each x In LQuery
                Dim byts As Byte() = x.byts
                Dim l As Long = byts.Length
                Dim lb As Byte() = BitConverter.GetBytes(l)

                Call list.AddRange(lb)
                Call list.AddRange(byts)
            Next

            Return list.ToArray
        End Function
    End Class
End Namespace