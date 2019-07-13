#Region "Microsoft.VisualBasic::18caaeaecae432ae59ba5c9891b0b90d, Microsoft.VisualBasic.Core\Serialization\JSON\JsonFormatter.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module JsonFormatter
    ' 
    '         Function: Format, Minify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Serialization.JSON.Formatter.Internals
Imports r = System.Text.RegularExpressions.Regex

Namespace Serialization.JSON.Formatter

    ''' <summary>
    ''' Provides JSON formatting functionality.
    ''' </summary>
    Public Module JsonFormatter

        ''' <summary>
        ''' Returns a 'pretty printed' version of the specified JSON string, formatted for human
        ''' consumption.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'pretty printed' version of the specified JSON string.</returns>
        Public Function Format(json As String) As String
            Dim context As New JsonFormatterStrategyContext()
            Dim formatter As New JsonFormatterInternal(context)

            Return formatter.Format(json Or die("json should not be null."))
        End Function

        ''' <summary>
        ''' Returns a 'minified' version of the specified JSON string, stripped of all 
        ''' non-essential characters.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'minified' version of the specified JSON string.</returns>
        Public Function Minify(json As String) As String
            Return r.Replace(json Or die("json should not be null."), "(""(?:[^""\\]|\\.)*"")|\s+", "$1")
        End Function
    End Module
End Namespace
