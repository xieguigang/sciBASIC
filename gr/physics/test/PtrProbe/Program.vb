Module Program

    Sub Main()
        Dim buf(15) As Integer
        Dim handle = Runtime.InteropServices.GCHandle.Alloc(buf, Runtime.InteropServices.GCHandleType.Pinned)

        Try
            Dim p As Integer* = CType(handle.AddrOfPinnedObject().ToPointer(), Integer*)
            Fill(p, 16)
            Console.WriteLine(buf(3))
        Finally
            handle.Free()
        End Try
    End Sub

    Sub Fill(ByVal p As Integer*, n As Integer)
        For i As Integer = 0 To n - 1
            p(i) = i
        Next
    End Sub

End Module
