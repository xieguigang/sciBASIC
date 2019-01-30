Imports System.IO

Namespace ManagedSqlite.Core.Helpers
    Module StreamHelper

        <System.Runtime.CompilerServices.Extension>
        Public Function ReadFully(stream As Stream, length As Integer) As Byte()
            Dim data As Byte() = New Byte(length - 1) {}
            stream.ReadFully(data, 0, data.Length)

            Return data
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function ReadFully(stream As Stream, buffer As Byte(), offset As Integer, length As Integer) As Integer
            Dim totalRead As Integer = 0
            Dim numRead As Integer = stream.Read(buffer, offset, length)
            While numRead > 0
                totalRead += numRead
                If totalRead = length Then
                    Exit While
                End If

                numRead = stream.Read(buffer, offset + totalRead, length - totalRead)
            End While

            Return totalRead
        End Function
    End Module
End Namespace
