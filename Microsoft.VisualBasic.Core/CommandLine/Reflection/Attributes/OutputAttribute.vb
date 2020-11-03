Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class OutputAttribute : Inherits Attribute

        Public ReadOnly Property result As Type
        ''' <summary>
        ''' The file extension name, like ``*.csv``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property extension As String

        Sub New(resultType As Type, fileExt$)
            result = resultType
            extension = fileExt
        End Sub

    End Class
End Namespace