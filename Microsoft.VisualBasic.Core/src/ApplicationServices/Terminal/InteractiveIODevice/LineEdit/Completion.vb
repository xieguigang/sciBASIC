Namespace ApplicationServices.Terminal.LineEdit

    ''' <summary>
    ''' Completion results returned by the completion handler.
    ''' </summary>
    ''' <remarks>
    ''' You create an instance of this class to return the completion
    ''' results for the text at the specific position.   The prefix parameter
    ''' indicates the common prefix in the results, and the results contain the
    ''' results without the prefix.   For example, when completing "ToString" and "ToDate"
    ''' prefix would be "To" and the completions would be "String" and "Date".
    ''' </remarks>
    Public Class Completion
        ''' <summary>
        ''' Array of results, with the stem removed.
        ''' </summary>
        Public Result As String()

        ''' <summary>
        ''' Shared prefix for the completion results.
        ''' </summary>
        Public Prefix As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Completion"/> class.
        ''' </summary>
        ''' <param name="prefix">Common prefix for all results, an be null.</param>
        ''' <param name="result">Array of possible completions.</param>
        Public Sub New(prefix As String, result As String())
            Me.Prefix = prefix
            Me.Result = result
        End Sub
    End Class
End Namespace