Option Strict On
Option Explicit On
Option Infer On

Namespace Columns

    ''' <summary>
    ''' 强类型数组的创建辅助函数（正确处理长度为 0 的情况）。
    ''' </summary>
    Public Module ArrayUtil

        Public Function NewArray(Of T)(count As Integer) As T()
            If count <= 0 Then
                Return New T() {}
            End If

            Return New T(count - 1) {}
        End Function

        ''' <summary>
        ''' 按物理类型创建一个空的强类型数组。
        ''' </summary>
        Public Function EmptyValues(type As Format.ParquetType) As Array
            Select Case type
                Case Format.ParquetType.[BOOLEAN] : Return New Boolean() {}
                Case Format.ParquetType.INT32 : Return New Integer() {}
                Case Format.ParquetType.INT64 : Return New Long() {}
                Case Format.ParquetType.FLOAT : Return New Single() {}
                Case Format.ParquetType.[DOUBLE] : Return New Double() {}
                Case Else : Return New Byte()() {}
            End Select
        End Function

    End Module

End Namespace
