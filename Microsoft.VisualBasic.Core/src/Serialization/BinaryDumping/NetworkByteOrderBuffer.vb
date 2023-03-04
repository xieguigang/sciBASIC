#Region "Microsoft.VisualBasic::4268f7e4958e1e091b5d6d7499c73f70, sciBASIC#\Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\NetworkByteOrderBuffer.vb"

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

    '   Total Lines: 72
    '    Code Lines: 51
    ' Comment Lines: 2
    '   Blank Lines: 19
    '     File Size: 2.39 KB


    '     Class NetworkByteOrderBuffer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: defaultDecoder, defaultEncoder, networkByteOrderDecoder, networkByteOrderEncoder
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

                Call VBDebugger.EchoLine("system byte order is little endian.")
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
