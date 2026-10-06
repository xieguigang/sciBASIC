
''' <summary>单个工程的处理结果</summary>
Public Class ProjectResult

    Public Property FilePath As String
    Public Property Changes As VersionChange()
    Public Property RemovedConditions As Integer
    Public Property Warnings As Integer
    Public Property OutputPath As OutputPathResult
    Public Property [Error] As String
    Public Property Skipped As Boolean

    ''' <summary>输出路径修正是否产生了改动</summary>
    Public ReadOnly Property OutputPathChanged As Boolean
        Get
            Return OutputPath IsNot Nothing AndAlso OutputPath.Changed
        End Get
    End Property

    Public ReadOnly Property Changed As Boolean
        Get
            If Changes Is Nothing Then
                Return False
            End If

            For Each change In Changes
                If change.Changed Then
                    Return True
                End If
            Next

            Return False
        End Get
    End Property
End Class