Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    Public Class StyleJSON

        Public Property nodes As Dictionary(Of String, Style)
        Public Property edge As Dictionary(Of String, Style)
        Public Property labels As Dictionary(Of String, Style)

    End Class

    Public Structure Style

        Public Property stroke As String
        Public Property fontCSS As String
        Public Property fill As String
        Public Property size As String
        ''' <summary>
        ''' The display label source name
        ''' </summary>
        ''' <returns></returns>
        Public Property label As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace