Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.Document

Module AutoContext

    Friend Const AutoContext As String = "graphquery-auto-context"

    <Extension>
    Friend Function IsAutoContext(element As HtmlElement) As Boolean
        Return element.hasAttribute(AutoContext)
    End Function

    Public Function Attribute() As ValueAttribute
        Return New ValueAttribute With {
            .Name = AutoContext,
            .Values = Environment.StackTrace _
                .LineTokens _
                .AsList
        }
    End Function

End Module
