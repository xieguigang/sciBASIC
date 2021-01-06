#Region "Microsoft.VisualBasic::b810442024d29c996e35ab3b00996009, Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\Buffer.vb"

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

    '     Structure Buffer
    ' 
    '         Properties: TotalBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Serialize, ToString
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Module BufferAPI
    ' 
    '         Function: CreateBuffer, GetBuffer
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Serialization.BinaryDumping

    ''' <summary>
    ''' A biffer object with byte length prefix
    ''' </summary>
    ''' <remarks>
    ''' 如果是类似于Memory map file的数据流,由于Memory map file的预分配大小可能会大于实际的数据大小
    ''' 所以会需要使用一个长度的prefix来保证数据可以被正确读取,反序列化
    ''' </remarks>
    Public Structure Buffer : Implements ISerializable

        Dim Length As Long
        Dim buffer As Byte()

        Sub New(buf As Byte())
            Length = buf.Length
            buffer = buf
        End Sub

        Public ReadOnly Property TotalBytes As Long
            Get
                Return Length + RawStream.INT64
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Length} bytes..."
        End Function

        Public Function Serialize() As Byte() Implements ISerializable.Serialize
            Dim buffer As Byte() = New Byte(TotalBytes - 1) {}
            Call Array.ConstrainedCopy(BitConverter.GetBytes(Length), Scan0, buffer, Scan0, RawStream.INT64)
            Call Array.ConstrainedCopy(Me.buffer, Scan0, buffer, RawStream.INT64, Me.buffer.Length)
            Return buffer
        End Function
    End Structure

    Public Delegate Function IGetBuffer(Of T)(x As T) As Byte()
    Public Delegate Function IGetObject(Of T)(buf As Byte()) As T

    ''' <summary>
    ''' 适用于对变长的流的操作
    ''' </summary>
    Public Module BufferAPI

        Public Function CreateBuffer(Of T)(source As IEnumerable(Of T), getBuf As IGetBuffer(Of T)) As Byte()
            Dim array As Buffer() = source.Select(Function(x) New Buffer(getBuf(x))).ToArray
            Dim buffer As Byte() = New Byte(array.Sum(Function(x) x.TotalBytes) - 1L) {}
            Dim i As Integer

            For Each x As Buffer In array
                Call System.Array.ConstrainedCopy(x.Serialize, Scan0, buffer, i, x.TotalBytes)
                i += x.TotalBytes
            Next

            Return buffer
        End Function

        Public Iterator Function GetBuffer(Of T)(raw As Byte(), getObj As IGetObject(Of T)) As IEnumerable(Of T)
            Dim length As Byte() = New Byte(RawStream.INT64 - 1) {}
            Dim l As Long
            Dim i As i32 = 0
            Dim temp As Byte()
            Dim x As T

            Do While True
                Call Array.ConstrainedCopy(raw, i + RawStream.INT64, length, Scan0, RawStream.INT64)
                l = BitConverter.ToInt64(length, Scan0)
                temp = New Byte(l - 1) {}
                Call Array.ConstrainedCopy(raw, i + l, temp, Scan0, l)
                x = getObj(temp)
                Yield x

                If i >= raw.Length - 1 Then
                    Exit Do
                End If
            Loop
        End Function
    End Module
End Namespace
