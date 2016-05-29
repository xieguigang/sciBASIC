Namespace d3.color

    Public Module rgb

        Public Function d3_rgb_hex(v As Byte) As String
            Return If(v < "0x10", "0" + Math.Max(CByte(0), v).ToString(16), Math.Min(CByte(255), v).ToString(16))
        End Function
    End Module
End Namespace