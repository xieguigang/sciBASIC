Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Components

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
            Select Case cdfDataType
                Case CDFDataTypes.BYTE : Return byteStream
                Case CDFDataTypes.CHAR : Return chars
                Case CDFDataTypes.DOUBLE : Return numerics.JoinBy(",")
                Case CDFDataTypes.FLOAT : Return tiny_num.JoinBy(",")
                Case CDFDataTypes.INT : Return integers.JoinBy(",")
                Case CDFDataTypes.SHORT : Return tiny_int.JoinBy(",")
                Case Else
                    Return "null"
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Byte()) As CDFData
            Return New CDFData With {.byteStream = data.ToBase64String}
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

        Public Shared Widening Operator CType(data As (values As Object(), type As CDFDataTypes)) As CDFData
            Select Case data.type
                Case CDFDataTypes.BYTE
                    Return data.values.As(Of Byte)
                Case CDFDataTypes.CHAR
                    Return data.values.As(Of Char)
                Case CDFDataTypes.DOUBLE
                    Return data.values.As(Of Double)
                Case CDFDataTypes.FLOAT
                    Return data.values.As(Of Single)
                Case CDFDataTypes.INT
                    Return data.values.As(Of Integer)
                Case CDFDataTypes.SHORT
                    Return data.values.As(Of Short)
                Case Else
                    Return New CDFData
            End Select
        End Operator
    End Class
End Namespace