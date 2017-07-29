Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileStream.Json

    Public Class net

        Public Property nodes As node()
        Public Property edges As edges()
        ''' <summary>
        ''' 优先加载的样式名称
        ''' </summary>
        ''' <returns></returns>
        Public Property style As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class edges
        Public Property source As Integer
        Public Property target As Integer
        Public Property A As String
        Public Property B As String
        Public Property value As String
        Public Property weight As String
        Public Property id As String
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class node
        Public Property id As Integer
        Public Property name As String
        Public Property degree As Integer
        Public Property type As String
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace