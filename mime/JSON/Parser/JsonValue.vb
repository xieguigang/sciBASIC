Namespace Parser

    ''' <summary>
    ''' Primitive value.
    ''' (请注意，假若是字符串的话，值是未经过处理的原始字符串，可能会含有转义字符，
    ''' 则这个时候还需要使用<see cref="GetStripString"/>得到最终的字符串)
    ''' </summary>
    Public Class JsonValue : Inherits JsonElement

        Public Overloads Property Value As Object

        Public Sub New()
        End Sub

        Public Sub New(obj As Object)
            Value = obj
        End Sub

        ''' <summary>
        ''' 处理转义等特殊字符串
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStripString() As String
            Dim s$ = Scripting _
                .ToString(Value, "null") _
                .GetString
            s = JsonParser.StripString(s)
            Return s
        End Function

        Public Overrides Function BuildJsonString() As String
            Return Scripting.ToString(Value, "null")
        End Function

        Public Overrides Function ToString() As String
            Return GetStripString()
        End Function
    End Class
End Namespace