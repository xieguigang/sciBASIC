#Region "Microsoft.VisualBasic::27bb47af904856479df933bd2fa06a6c, Data\BinaryData\DataStorage\netCDF\Components\CDFData.vb"

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
'         Properties: byteStream, cdfDataType, chars, integers, Length
'                     numerics, tiny_int, tiny_num
' 
'         Function: GetBuffer, GetInt64Buffer, ToString
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
                    Case Else
                        Return 0
                End Select
            End Get
        End Property

        Public ReadOnly Property cdfDataType As CDFDataTypes
            Get
                If Not byteStream.StringEmpty Then
                    Return CDFDataTypes.BYTE
                ElseIf Not chars.StringEmpty Then
                    Return CDFDataTypes.CHAR
                ElseIf Not tiny_int.IsNullOrEmpty Then
                    Return CDFDataTypes.SHORT
                ElseIf Not integers.IsNullOrEmpty Then
                    Return CDFDataTypes.INT
                ElseIf Not tiny_num.IsNullOrEmpty Then
                    Return CDFDataTypes.FLOAT
                ElseIf Not numerics.IsNullOrEmpty Then
                    Return CDFDataTypes.DOUBLE
                Else
                    ' null
                    Return CDFDataTypes.undefined
                End If
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
                Case Else
                    Throw New NotImplementedException(cdfDataType.Description)
            End Select
        End Function

        ''' <summary>
        ''' 将<see cref="byteStream"/>转换为字节流之后分割然后转换为<see cref="Long"/>之后返回
        ''' </summary>
        ''' <returns></returns>
        Public Function GetInt64Buffer() As Long()
            Dim bytes As Byte() = Convert.FromBase64String(byteStream)
            Dim int64s&() = bytes _
                .Split(8) _
                .Select(Function(chunk)
                            Return BitConverter.ToInt64(chunk, Scan0)
                        End Function) _
                .ToArray

            Return int64s
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Byte()) As CDFData
            Return New CDFData With {.byteStream = data.ToBase64String}
        End Operator

        ''' <summary>
        ''' 在进行转换的时候也需要以相同的方式进行逆变换
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Long()) As CDFData
            Return New CDFData With {
                .byteStream = data _
                    .SelectMany(Function(i) BitConverter.GetBytes(i)) _
                    .ToBase64String
            }
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

        Public Shared Widening Operator CType(data As (values As Object(), type As CDFDataTypes)) As CDFData
            Select Case data.type
                Case CDFDataTypes.BYTE
                    Return data.values.As(Of Byte).ToArray
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
                Case Else
                    Return New CDFData
            End Select
        End Operator
    End Class
End Namespace
