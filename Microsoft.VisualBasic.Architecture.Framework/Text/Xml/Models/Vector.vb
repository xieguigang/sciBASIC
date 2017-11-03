Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    Public Class NumericVector

        <XmlAttribute>
        Public Property Vector As Double()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

End Namespace