Imports System.Text.RegularExpressions

Namespace Serialization.JSON.Formatter

    ''' <summary>
    ''' Provides JSON formatting functionality.
    ''' </summary>
    Public NotInheritable Class JsonFormatter
        Private Sub New()
        End Sub
        ''' <summary>
        ''' Returns a 'pretty printed' version of the specified JSON string, formatted for human
        ''' consumption.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'pretty printed' version of the specified JSON string.</returns>
        Public Shared Function Format(json As String) As String
            If json Is Nothing Then
                Throw New ArgumentNullException("json should not be null.")
            End If

            Dim context = New JsonFormatterStrategyContext()
            Dim formatter = New JsonFormatterInternal(context)

            Return formatter.Format(json)
        End Function

        ''' <summary>
        ''' Returns a 'minified' version of the specified JSON string, stripped of all 
        ''' non-essential characters.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'minified' version of the specified JSON string.</returns>
        Public Shared Function Minify(json As String) As String
            If json Is Nothing Then
                Throw New ArgumentNullException("json should not be null.")
            End If

            Return Regex.Replace(json, "(""(?:[^""\\]|\\.)*"")|\s+", "$1")
        End Function
    End Class
End Namespace