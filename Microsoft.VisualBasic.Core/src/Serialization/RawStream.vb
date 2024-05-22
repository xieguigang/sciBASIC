#Region "Microsoft.VisualBasic::23af9d66f0dfea244754fabaa4ca1027, Microsoft.VisualBasic.Core\src\Serialization\RawStream.vb"

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

    '   Total Lines: 237
    '    Code Lines: 166 (70.04%)
    ' Comment Lines: 41 (17.30%)
    '    - Xml Docs: 97.56%
    ' 
    '   Blank Lines: 30 (12.66%)
    '     File Size: 10.45 KB


    '     Interface ISerializable
    ' 
    '         Function: Serialize
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class RawStream
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: BytesInternal, empty, GetBytes, (+2 Overloads) GetData, GetRawStream
    '                   readInternal, Serialize
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes

Namespace Serialization

    ''' <summary>
    ''' 支持序列化的对象，则这个对象可以被应用于<see cref="RequestStream"/>数据载体的网络传输操作过程
    ''' </summary>
    Public Interface ISerializable
        ''' <summary>
        ''' Transform this .NET object into a raw stream object for the network data transfer. 
        ''' </summary>
        ''' <returns></returns>
        Function Serialize() As Byte()
    End Interface

    Public Delegate Function ReadObject(Of T)(bytes As Byte(), offset As Integer) As T

    ''' <summary>
    ''' 原始串流的基本模型，这个流对象应该具备有两个基本的方法：
    ''' 1. 从原始的字节流之中反序列化构造出自身的构造函数
    ''' 2. 将自身序列化为字节流的<see cref="ISerializable.Serialize()"/>序列化方法
    ''' </summary>
    <Serializable> Public MustInherit Class RawStream : Implements ISerializable

        ''' <summary>
        ''' You should overrides this constructor to generate a stream object.(必须要有一个这个构造函数来执行反序列化)
        ''' </summary>
        ''' <param name="rawStream"></param>
        Sub New(rawStream As Byte())

        End Sub

        Public Sub New()
        End Sub

        ''' <summary>
        ''' <see cref="ISerializable.Serialize"/>序列化方法
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Serialize() As Byte() Implements ISerializable.Serialize
            Using memory As New MemoryStream()
                Call Serialize(memory)
                Call memory.Flush()

                Return memory.ToArray
            End Using
        End Function

        Public MustOverride Overloads Sub Serialize(buffer As Stream)

        ''' <summary>
        ''' 按照类型的定义进行反序列化操作
        ''' </summary>
        ''' <typeparam name="TRawStream"></typeparam>
        ''' <param name="rawStream"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetRawStream(Of TRawStream As RawStream)(rawStream As Byte()) As TRawStream
            Return Activator.CreateInstance(GetType(TRawStream), {rawStream})
        End Function

        Protected Shared ReadOnly _rawStreamType As Type = GetType(Byte())

        Public Const INT64 As Integer = 8
        ''' <summary>
        ''' Single/Integer
        ''' </summary>
        Public Const INT32 As Integer = 4
        ''' <summary>
        ''' sizeof <see cref="Double"/>
        ''' </summary>
        Public Const DblFloat As Integer = 8
        Public Const ShortInt As Integer = 2
        Public Const SingleFloat As Integer = 4
        Public Const DecimalInt As Integer = 12

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetData(raw As Byte(), code As TypeCode, Optional encoding As Encodings = Encodings.UTF8) As Array
            Using ms As New MemoryStream(raw)
                Return GetData(ms, code, encoding)
            End Using
        End Function

        Private Shared Function empty(code As TypeCode) As Array
            Select Case code
                Case TypeCode.Boolean : Return New Boolean() {}
                Case TypeCode.Byte : Return New Byte() {}
                Case TypeCode.Char : Return New Char() {}
                Case TypeCode.DateTime : Return New Date() {}
                Case TypeCode.DBNull : Return New DBNull() {}
                Case TypeCode.Decimal : Return New Decimal() {}
                Case TypeCode.Double : Return New Double() {}
                Case TypeCode.Empty : Return New Object() {}
                Case TypeCode.SByte : Return New SByte() {}
                Case TypeCode.Single : Return New Single() {}
                Case TypeCode.String : Return New String() {}
                Case TypeCode.UInt16 : Return New UInt16() {}
                Case TypeCode.UInt32 : Return New UInt32() {}
                Case TypeCode.UInt64 : Return New UInt64() {}
                Case Else
                    Return New Object() {}
            End Select
        End Function

        Public Shared Function GetData(raw As Stream, code As TypeCode, Optional encoding As Encodings = Encodings.UTF8) As Array
            Dim type As Type = code.CreatePrimitiveType
            Dim bytes As Byte() = New Byte(raw.Length - 1) {}

            If bytes.Length = 0 Then
                Return empty(code)
            Else
                Call raw.Read(bytes, Scan0, bytes.Length)
            End If

            Select Case code
                Case TypeCode.Boolean
                    Dim flags As Boolean() = New Boolean(bytes.Length - 1) {}

                    For i As Integer = 0 To bytes.Length - 1
                        flags(i) = bytes(i) <> 0
                    Next

                    Return flags
                Case TypeCode.Byte
                    Return bytes
                Case TypeCode.Char
                    Return encoding.CodePage.GetString(bytes).ToArray
                Case TypeCode.Double
                    Return readInternal(bytes, AddressOf BitConverter.ToDouble)
                Case TypeCode.Single
                    Return readInternal(bytes, AddressOf BitConverter.ToSingle)
                Case TypeCode.String
                    Dim str As New List(Of String)
                    Dim size As Integer
                    Dim codepage As Encoding = encoding.CodePage
                    Dim intBuffer As Byte() = New Byte(3) {}

                    Using ms As New MemoryStream(bytes)
                        Do While ms.Position < ms.Length
                            ms.Read(intBuffer, Scan0, 4)
                            size = BitConverter.ToInt32(intBuffer, Scan0)
                            bytes = New Byte(size - 1) {}
                            ms.Read(bytes, Scan0, bytes.Length)
                            str += codepage.GetString(bytes)
                        Loop
                    End Using

                    Return str.ToArray
                Case TypeCode.DateTime
                    Dim timestamps = readInternal(bytes, AddressOf BitConverter.ToDouble)
                    Dim time As DateTime() = timestamps _
                        .Select(AddressOf FromUnixTimeStamp) _
                        .ToArray

                    Return time
                Case TypeCode.Int64
                    Return readInternal(bytes, AddressOf BitConverter.ToInt64)
                Case TypeCode.Int16
                    Return readInternal(bytes, AddressOf BitConverter.ToInt16)
                Case TypeCode.Int32
                    Return readInternal(bytes, AddressOf BitConverter.ToInt32)
                Case Else
                    Throw New NotImplementedException(code.ToString)
            End Select
        End Function

        Private Shared Function readInternal(Of T)(bytes As Byte(), read As ReadObject(Of T)) As T()
            Dim sizeof As Integer = Marshal.SizeOf(GetType(T))
            Dim objs As T() = New T(bytes.Length / sizeof - 1) {}

            For i As Integer = 0 To objs.Length - 1
                objs(i) = read(bytes, i * sizeof)
            Next

            Return objs
        End Function

        ''' <summary>
        ''' this function only works for the primitive data types
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <param name="encoding"></param>
        ''' <returns>
        ''' the empty byte collection will be return if the input vector is nothing
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetBytes(vector As Array, Optional encoding As Encodings = Encodings.UTF8) As Byte()
            If vector Is Nothing OrElse vector.Length = 0 Then
                Return {}
            Else
                Return BytesInternal(vector, encoding) _
                    .IteratesALL _
                    .ToArray
            End If
        End Function

        Private Shared Function BytesInternal(vector As Array, encoding As Encodings) As IEnumerable(Of Byte())
            If TypeOf vector Is Integer() Then
                Return DirectCast(vector, Integer()).Select(Function(s) BitConverter.GetBytes(s))
            ElseIf TypeOf vector Is Long() Then
                Return DirectCast(vector, Long()).Select(Function(s) BitConverter.GetBytes(s))
            ElseIf TypeOf vector Is Double() Then
                Return DirectCast(vector, Double()).Select(Function(s) BitConverter.GetBytes(s))
            ElseIf TypeOf vector Is Single() Then
                Return DirectCast(vector, Single()).Select(Function(s) BitConverter.GetBytes(s))
            ElseIf TypeOf vector Is Boolean() Then
                Return DirectCast(vector, Boolean()).Select(Function(b) {If(b, CByte(1), CByte(0))})
            ElseIf TypeOf vector Is Byte() Then
                Return {DirectCast(vector, Byte())}
            ElseIf TypeOf vector Is DateTime() Then
                Return DirectCast(vector, DateTime()).Select(Function(d) BitConverter.GetBytes(d.UnixTimeStamp))
            ElseIf TypeOf vector Is String() Then
                Dim codepage As Encoding = encoding.CodePage

                Return DirectCast(vector, String()) _
                    .Select(Function(str)
                                Dim bytes As Byte() = If(str Is Nothing, {}, codepage.GetBytes(str))
                                Dim size As Byte() = BitConverter.GetBytes(bytes.Length)

                                Return size.JoinIterates(bytes).ToArray
                            End Function)
            Else
                Throw New NotImplementedException(vector.GetType.FullName)
            End If
        End Function
    End Class
End Namespace
