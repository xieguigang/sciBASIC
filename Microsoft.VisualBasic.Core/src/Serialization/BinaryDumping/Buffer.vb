#Region "Microsoft.VisualBasic::51912bccf8e45edd957f0da4a5f1c13e, Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\Buffer.vb"

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

    '   Total Lines: 102
    '    Code Lines: 72 (70.59%)
    ' Comment Lines: 13 (12.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (16.67%)
    '     File Size: 3.84 KB


    '     Structure Buffer
    ' 
    '         Properties: TotalBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Parse, Serialize, ToString
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

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Serialization.BinaryDumping

    ''' <summary>
    ''' A biffer object with byte length prefix
    ''' </summary>
    ''' <remarks>
    ''' 如果是类似于Memory map file的数据流,由于Memory map file的预分配大小可能会大于实际的数据大小
    ''' 所以会需要使用一个长度的prefix来保证数据可以被正确读取,反序列化
    ''' </remarks>
    Public Structure Buffer : Implements ISerializable

        ''' <summary>
        ''' the length of <see cref="buffer"/> array
        ''' </summary>
        Dim Length As Integer
        Dim buffer As Byte()

        Sub New(buf As Byte())
            Length = buf.Length
            buffer = buf
        End Sub

        Public ReadOnly Property TotalBytes As Integer
            Get
                Return Length + RawStream.INT32
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return StringFormats.Lanudry(bytes:=TotalBytes)
        End Function

        Public Function Serialize() As Byte() Implements ISerializable.Serialize
            Dim buffer As Byte() = New Byte(TotalBytes - 1) {}
            Call Array.ConstrainedCopy(BitConverter.GetBytes(Length), Scan0, buffer, Scan0, RawStream.INT32)
            Call Array.ConstrainedCopy(Me.buffer, Scan0, buffer, RawStream.INT32, Length)
            Return buffer
        End Function

        Public Shared Function Parse(s As Stream) As Buffer
            Dim bytes As Byte() = New Byte(RawStream.INT32 - 1) {}
            Call s.Read(bytes, Scan0, RawStream.INT32)
            Dim nlen As Integer = BitConverter.ToInt32(bytes, Scan0)
            bytes = New Byte(nlen - 1) {}
            Call s.Read(bytes, Scan0, nlen)
            Return New Buffer(bytes)
        End Function

        Public Shared Function Parse(rd As BinaryReader) As Buffer
            Dim nlen As Integer = rd.ReadInt32
            Dim bytes As Byte() = rd.ReadBytes(nlen)
            Return New Buffer(bytes)
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
