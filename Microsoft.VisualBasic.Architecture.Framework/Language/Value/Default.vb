Namespace Language

    Public Delegate Function Assert(Of T)(obj As T) As Boolean

    ''' <summary>
    ''' The default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure DefaultValue(Of T)

        ''' <summary>
        ''' The default value
        ''' </summary>
        Dim Value As T
        ''' <summary>
        ''' asset that if target value is null?
        ''' </summary>
        Dim assert As Assert(Of Object)

        Public Overrides Function ToString() As String
            Return $"default({Value})"
        End Function

        Public Shared Operator Or(obj As T, [default] As DefaultValue(Of T)) As T
            With [default]
                If .assert(obj) Then
                    Return .Value
                Else
                    Return obj
                End If
            End With
        End Operator
    End Structure
End Namespace