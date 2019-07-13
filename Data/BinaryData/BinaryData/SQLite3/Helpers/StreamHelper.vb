#Region "Microsoft.VisualBasic::c0e42c57c523973aaf7d35d39c60a1ea, Data\BinaryData\BinaryData\SQLite3\Helpers\StreamHelper.vb"

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

    '     Module StreamHelper
    ' 
    '         Function: (+2 Overloads) ReadFully
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices

Namespace ManagedSqlite.Core.Helpers

    Module StreamHelper

        <Extension>
        Public Function ReadFully(stream As Stream, length As Integer) As Byte()
            Dim data As Byte() = New Byte(length - 1) {}
            Call stream.ReadFully(data, 0, data.Length)
            Return data
        End Function

        <Extension>
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
