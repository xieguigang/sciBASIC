#Region "Microsoft.VisualBasic::c1373f92f4a1b5e6eb10f162af7c801c, sciBASIC#\Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\BinaryWriter.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 50
'    Code Lines: 37
' Comment Lines: 3
'   Blank Lines: 10
'     File Size: 1.98 KB


'     Module BinaryWriter
' 
'         Function: (+2 Overloads) Serialization, serializeInternal
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Serialization.BinaryDumping

    Public Class NetworkByteOrderBuffer

        Public ReadOnly encode As Func(Of Double(), Byte())
        Public ReadOnly decode As Func(Of Byte(), Double())

        Sub New()
            If BitConverter.IsLittleEndian Then
                ' reverse bytes
                encode = AddressOf networkByteOrderEncoder
                decode = AddressOf networkByteOrderDecoder

                Call Console.WriteLine("system byte order is little endian.")
            Else
                ' no bytes sequence reverse
                encode = AddressOf defaultEncoder
                decode = AddressOf defaultDecoder
            End If
        End Sub

        Private Shared Function defaultDecoder(buffer As Byte()) As Double()
            Dim nums As Double() = New Double(buffer.Length / 8 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                nums(i) = BitConverter.ToDouble(buffer, i * 8)
            Next

            Return nums
        End Function

        Private Shared Function defaultEncoder(nums As Double()) As Byte()
            Dim bytes As New List(Of Byte)

            For Each d As Double In nums
                Call bytes.AddRange(BitConverter.GetBytes(d))
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function networkByteOrderDecoder(buffer As Byte()) As Double()
            Dim nums As Double() = New Double(buffer.Length / 8 - 1) {}
            Dim bytes As Byte() = New Byte(8 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                Call Array.ConstrainedCopy(buffer, i * 8, bytes, Scan0, bytes.Length)
                Call Array.Reverse(bytes)

                nums(i) = BitConverter.ToDouble(bytes, Scan0)
            Next

            Return nums
        End Function

        Private Shared Function networkByteOrderEncoder(nums As Double()) As Byte()
            Dim bytes As New List(Of Byte)
            Dim buffer As Byte()

            For Each d As Double In nums
                buffer = BitConverter.GetBytes(d)

                Call Array.Reverse(buffer)
                Call bytes.AddRange(buffer)
            Next

            Return bytes.ToArray
        End Function
    End Class
End Namespace
