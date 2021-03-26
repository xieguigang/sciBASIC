#Region "Microsoft.VisualBasic::b7e0fff7fd2c090cf52c8261144118ae, Data\BinaryData\DataStorage\netCDF\Components\CDFData.vb"

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

    '     Class CDFData
    ' 
    '         Properties: byteStream, cdfDataType, chars, flags, genericValue
    '                     integers, Length, longs, numerics, tiny_int
    '                     tiny_num
    ' 
    '         Function: GetBuffer, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace netCDF.Components

    ''' <summary>
    '''  存储在CDF文件之中的数据的统一接口模块
    ''' </summary>
    Public Class CDFData

        ''' <summary>
        ''' byte集合，base64字符串编码
        ''' </summary>
        ''' <returns></returns>
        Public Property byteStream As String
        ''' <summary>
        ''' char集合
        ''' </summary>
        ''' <returns></returns>
        Public Property chars As String
        Public Property tiny_int As Short()
        Public Property integers As Integer()
        Public Property tiny_num As Single()
        Public Property numerics As Double()
        Public Property longs As Long()
        Public Property flags As Boolean()

        Public ReadOnly Property Length As Integer
            Get
                Select Case cdfDataType
                    Case CDFDataTypes.BYTE
                        Return Convert.FromBase64String(byteStream).Length
                    Case CDFDataTypes.CHAR
                        Return chars.Length
                    Case CDFDataTypes.DOUBLE
                        Return numerics.Length
                    Case CDFDataTypes.FLOAT
                        Return tiny_num.Length
                    Case CDFDataTypes.INT
                        Return integers.Length
                    Case CDFDataTypes.SHORT
                        Return tiny_int.Length
                    Case CDFDataTypes.LONG
                        Return longs.Length
                    Case CDFDataTypes.BOOLEAN
                        Return flags.Length
                    Case Else
                        Return 0
                End Select
            End Get
        End Property

        Public ReadOnly Property cdfDataType As CDFDataTypes
            Get
                If Not byteStream Is Nothing Then
                    Return CDFDataTypes.BYTE
                ElseIf Not chars Is Nothing Then
                    Return CDFDataTypes.CHAR
                ElseIf Not tiny_int Is Nothing Then
                    Return CDFDataTypes.SHORT
                ElseIf Not integers Is Nothing Then
                    Return CDFDataTypes.INT
                ElseIf Not tiny_num Is Nothing Then
                    Return CDFDataTypes.FLOAT
                ElseIf Not numerics Is Nothing Then
                    Return CDFDataTypes.DOUBLE
                ElseIf Not longs Is Nothing Then
                    Return CDFDataTypes.LONG
                ElseIf Not flags Is Nothing Then
                    Return CDFDataTypes.BOOLEAN
                Else
                    ' null
                    Return CDFDataTypes.undefined
                End If
            End Get
        End Property

        Public ReadOnly Property genericValue As Object
            Get
                Select Case cdfDataType
                    Case CDFDataTypes.BYTE
                        Return byteStream.Base64RawBytes
                    Case CDFDataTypes.CHAR
                        Return chars
                    Case CDFDataTypes.DOUBLE
                        Return numerics
                    Case CDFDataTypes.FLOAT
                        Return tiny_num
                    Case CDFDataTypes.INT
                        Return integers
                    Case CDFDataTypes.LONG
                        Return longs
                    Case CDFDataTypes.SHORT
                        Return tiny_int
                    Case CDFDataTypes.BOOLEAN
                        Return flags
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim stringify$

            Select Case cdfDataType
                Case CDFDataTypes.BYTE : stringify = byteStream
                Case CDFDataTypes.CHAR : stringify = chars
                Case CDFDataTypes.DOUBLE : stringify = numerics.JoinBy(",")
                Case CDFDataTypes.FLOAT : stringify = tiny_num.JoinBy(",")
                Case CDFDataTypes.INT : stringify = integers.JoinBy(",")
                Case CDFDataTypes.SHORT : stringify = tiny_int.JoinBy(",")
                Case CDFDataTypes.LONG : stringify = longs.JoinBy(",")
                Case CDFDataTypes.BOOLEAN : stringify = flags.Select(Function(b) If(b, 1, 0)).JoinBy(",")
                Case Else
                    Return "null"
            End Select

            If (stringify.Length > 50) Then
                stringify = stringify.Substring(0, 50)
            End If
            If (cdfDataType <> CDFDataTypes.undefined) Then
                stringify &= $" (length: ${Me.Length})"
            End If

            Return $"[{cdfDataType}] {stringify}"
        End Function

        Public Function GetBuffer(encoding As Encoding) As Byte()
            Select Case cdfDataType
                Case CDFDataTypes.BYTE : Return Convert.FromBase64String(byteStream)
                Case CDFDataTypes.BOOLEAN : Return flags.Select(Function(b) CByte(If(b, 1, 0))).ToArray
                Case CDFDataTypes.CHAR : Return encoding.GetBytes(chars)
                Case CDFDataTypes.DOUBLE
                    Return numerics _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .Select(Function(b) DirectCast(b, IEnumerable(Of Byte)).Reverse) _
                        .IteratesALL _
                        .ToArray
                Case CDFDataTypes.FLOAT
                    Return tiny_num _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .Select(Function(b) DirectCast(b, IEnumerable(Of Byte)).Reverse) _
                        .IteratesALL _
                        .ToArray
                Case CDFDataTypes.INT
                    Return integers _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .Select(Function(b) DirectCast(b, IEnumerable(Of Byte)).Reverse) _
                        .IteratesALL _
                        .ToArray
                Case CDFDataTypes.SHORT
                    Return tiny_int _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .Select(Function(b) DirectCast(b, IEnumerable(Of Byte)).Reverse) _
                        .IteratesALL _
                        .ToArray
                Case CDFDataTypes.LONG
                    Return longs _
                        .Select(AddressOf BitConverter.GetBytes) _
                        .Select(Function(b) DirectCast(b, IEnumerable(Of Byte)).Reverse) _
                        .IteratesALL _
                        .ToArray
                Case Else
                    Throw New NotImplementedException(cdfDataType.Description)
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Byte()) As CDFData
            Return New CDFData With {.byteStream = data.ToBase64String}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Boolean()) As CDFData
            Return New CDFData With {.flags = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Char()) As CDFData
            Return New CDFData With {.chars = New String(data)}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Integer()) As CDFData
            Return New CDFData With {.integers = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Short()) As CDFData
            Return New CDFData With {.tiny_int = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Single()) As CDFData
            Return New CDFData With {.tiny_num = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Double()) As CDFData
            Return New CDFData With {.numerics = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Vector(Of Double)) As CDFData
            Return New CDFData With {.numerics = data.Array}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Long()) As CDFData
            Return New CDFData With {.longs = data.ToArray}
        End Operator

        Public Shared Widening Operator CType(data As (values As Object(), type As CDFDataTypes)) As CDFData
            Select Case data.type
                Case CDFDataTypes.BYTE
                    If data.values.All(Function(obj) TypeOf obj Is Byte()) Then
                        Return data.values _
                            .Select(Function(obj)
                                        Return DirectCast(obj, Byte())(Scan0)
                                    End Function) _
                            .ToArray
                    Else
                        Return data.values.As(Of Byte).ToArray
                    End If
                Case CDFDataTypes.BOOLEAN
                    Return data.values.As(Of Boolean).ToArray
                Case CDFDataTypes.CHAR
                    Return data.values.As(Of Char).ToArray
                Case CDFDataTypes.DOUBLE
                    Return data.values.As(Of Double).ToArray
                Case CDFDataTypes.FLOAT
                    Return data.values.As(Of Single).ToArray
                Case CDFDataTypes.INT
                    Return data.values.As(Of Integer).ToArray
                Case CDFDataTypes.SHORT
                    Return data.values.As(Of Short).ToArray
                Case CDFDataTypes.LONG
                    Return data.values.As(Of Long).ToArray
                Case Else
                    Return New CDFData
            End Select
        End Operator
    End Class
End Namespace
