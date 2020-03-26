Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace SourceMap

    Public Class sourceMap

        Public Property version As Integer
        Public Property file As String
        Public Property sourceRoot As String
        Public Property sources As String()
        Public Property names As String()
        Public Property mappings As String

    End Class

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class mappingLine

        ''' <summary>
        ''' 第一位，表示这个位置在（转换后的代码的）的第几列。
        ''' </summary>
        ''' <returns></returns>
        Public Property targetCol As Integer
        ''' <summary>
        ''' 第二位，表示这个位置属于sources属性中的哪一个文件。
        ''' </summary>
        ''' <returns></returns>
        Public Property fileIndex As Integer
        ''' <summary>
        ''' 第三位，表示这个位置属于转换前代码的第几行。
        ''' </summary>
        ''' <returns></returns>
        Public Property sourceLine As Integer
        ''' <summary>
        ''' 第四位，表示这个位置属于转换前代码的第几列。
        ''' </summary>
        ''' <returns></returns>
        Public Property sourceCol As Integer
        ''' <summary>
        ''' 第五位，表示这个位置属于names属性中的哪一个变量。
        ''' </summary>
        ''' <returns></returns>
        Public Property nameIndex As Integer

        Private ReadOnly Property isEmpty As Boolean
            Get
                Return targetCol = 0 AndAlso fileIndex = 0 AndAlso sourceLine = 0 AndAlso sourceCol = 0 AndAlso nameIndex = 0
            End Get
        End Property

        Public Function GetStackFrame(map As sourceMap) As StackFrame
            If isEmpty Then
                ' return empty info
                Return New StackFrame
            End If

            Return New StackFrame With {
                .File = map.sources(fileIndex),
                .Line = sourceLine,
                .Method = New Method With {
                    .Method = map.names.ElementAtOrDefault(nameIndex, "N/A")
                }
            }
        End Function

        Public Overrides Function ToString() As String
            Return New Integer() {
                targetCol, fileIndex, sourceLine, sourceCol, nameIndex
            } _
                .Select(AddressOf base64VLQ.base64VLQ_encode) _
                .JoinBy("")
        End Function
    End Class
End Namespace