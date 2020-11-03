Namespace CommandLine.Reflection

    ''' <summary>
    ''' 主要是用于帮助标记命令行命令的更新时间,了解哪些命令可能是已经过时了的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class LastUpdatedAttribute : Inherits Attribute

        Public ReadOnly [Date] As Date

        Sub New([date] As Date)
            Me.Date = [date]
        End Sub

        Sub New([date] As String)
            Me.Date = Date.Parse([date])
        End Sub

        Sub New(yy%, mm%, dd%, H%, M%, S%)
            Me.Date = New Date(yy, mm, dd, H, M, S)
        End Sub

        Public Overrides Function ToString() As String
            Return [Date].ToString
        End Function
    End Class
End Namespace