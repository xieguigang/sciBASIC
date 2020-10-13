#Region "Microsoft.VisualBasic::2c615581be58adaa5b5ec634cec6438f, www\Microsoft.VisualBasic.NETProtocol\Protocol\Streams\ArrayBase.vb"

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

    '     Class ValueArray
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Buffer = System.Array

Namespace Protocols.Streams.Array

    ''' <summary>
    ''' 对于<see cref="System.Int64"/>, <see cref="System.int32"/>, <see cref="System.Double"/>, <see cref="System.DateTime"/>
    ''' 这些类型的数据来说，进行网络传输的时候使用json会被转换为字符串，数据量比较大，而转换为字节再进行传输，数据流量的消耗会比较小
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>这个是定长的数组序列</remarks>
    Public MustInherit Class ValueArray(Of T) : Inherits ArrayAbstract(Of T)

        Protected ReadOnly _bufWidth As Integer

        Protected Sub New(serialization As IGetBuffer(Of T),
                          deserialization As IGetObject(Of T),
                          bufWidth As Integer,
                          rawStream As Byte())

            Call MyBase.New(serialization, deserialization)

            _bufWidth = bufWidth

            If Not rawStream.IsNullOrEmpty Then
                Dim valueList As New List(Of T)
                Dim p As i32 = 0
                Dim byts As Byte() = New Byte(_bufWidth - 1) {}

                Do While p < rawStream.Length - 1
                    Call Buffer.ConstrainedCopy(rawStream, p + bufWidth, byts, Scan0, bufWidth)
                    Call valueList.Add(MyBase.deserialization(byts))
                Loop

                Values = valueList.ToArray
            End If
        End Sub

        Public NotOverridable Overrides Function Serialize() As Byte()
            Dim bufferArray As Byte() = New Byte(Values.Length * _bufWidth - 1) {}
            Dim p As i32 = 0

            For Each value As T In Values
                Dim byts As Byte() = serialization(value)
                Call Buffer.ConstrainedCopy(byts, Scan0, bufferArray, p + _bufWidth, _bufWidth)
            Next

            Return bufferArray
        End Function

        Public Overrides Function ToString() As String
            If Values.IsNullOrEmpty Then
                Return GetType(T).FullName
            Else
                Dim valJson$ = Values _
                    .Select(Function(val) Scripting.ToString(val)) _
                    .GetJson
                Return $"{GetType(T).FullName} {valJson}"
            End If
        End Function
    End Class
End Namespace
