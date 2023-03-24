Namespace Turtle

    ''' <summary>
    ''' object properties
    ''' </summary>
    Public Class Triple

        Public Property subject As String
        Public Property relations As Relation()

        Public Overrides Function ToString() As String
            Return $"<{subject}> {relations.JoinBy(" ; ")}."
        End Function

    End Class

    ''' <summary>
    ''' property data
    ''' </summary>
    Public Class Relation

        ''' <summary>
        ''' the property name
        ''' </summary>
        ''' <returns></returns>
        Public Property predicate As String
        ''' <summary>
        ''' the property value
        ''' </summary>
        ''' <returns></returns>
        Public Property objs As String()

        Public Overrides Function ToString() As String
            Return $"<{predicate}> <{objs.JoinBy(" , ")}>"
        End Function
    End Class
End Namespace