#Region "Microsoft.VisualBasic::7607e531f0daa2eb23768dffd39c30a6, mime\application%json\Parser\JsonValue.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class JsonValue
    ' 
    '         Properties: value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: BuildJsonString, GetStripString, Literal, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Parser

    ''' <summary>
    ''' Primitive value.
    ''' (请注意，假若是字符串的话，值是未经过处理的原始字符串，可能会含有转义字符，
    ''' 则这个时候还需要使用<see cref="GetStripString"/>得到最终的字符串)
    ''' </summary>
    Public Class JsonValue : Inherits JsonElement

        Public Overloads Property value As Object

        Public Sub New()
        End Sub

        Public Sub New(obj As Object)
            value = obj
        End Sub

        Public Function Literal(typeOfT As Type) As Object
            Select Case typeOfT
                Case GetType(String)
                    Return GetStripString()
                Case GetType(Date)
                    Return Casting.CastDate(GetStripString)
                Case Else
                    Return Scripting.CTypeDynamic(value, typeOfT)
            End Select
        End Function

        ''' <summary>
        ''' 处理转义等特殊字符串
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStripString() As String
            Dim s$ = Scripting _
                .ToString(value, "null") _
                .GetString
            s = JsonParser.StripString(s)
            Return s
        End Function

        Public Overrides Function BuildJsonString() As String
            Return Scripting.ToString(value, "null")
        End Function

        Public Overrides Function ToString() As String
            Return GetStripString()
        End Function
    End Class
End Namespace
