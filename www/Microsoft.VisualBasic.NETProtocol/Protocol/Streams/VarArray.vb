#Region "Microsoft.VisualBasic::2f96d1b8fe455bcc689650341a8cbf4c, www\Microsoft.VisualBasic.NETProtocol\Protocol\Streams\VarArray.vb"

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

    '   Total Lines: 73
    '    Code Lines: 49
    ' Comment Lines: 8
    '   Blank Lines: 16
    '     File Size: 2.47 KB


    '     Class VarArray
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Buffer = System.Array

Namespace Protocols.Streams.Array

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
        Public Overrides Sub Serialize(buffer As Stream)
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

                Call buffer.Write(lb, Scan0, lb.Length)
                Call buffer.Write(byts, Scan0, byts.Length)

                Erase byts
                Erase lb
            Next

            Call buffer.Flush()
        End Sub
    End Class
End Namespace
