Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel

    ''' <summary>
    ''' Object entity classification class
    ''' </summary>
    Public Class [Class]

        ''' <summary>
        ''' Using for the data visualization.(RGB表达式, html颜色值或者名称)
        ''' </summary>
        ''' <returns></returns>
        Public Property Color As String
        ''' <summary>
        ''' <see cref="Integer"/> encoding for this class
        ''' </summary>
        ''' <returns></returns>
        Public Property int As Integer
        ''' <summary>
        ''' Class Name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace