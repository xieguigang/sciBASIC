Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity.NLP

    Public Class Word : Implements Value(Of String).IValueOf

        <XmlAttribute>
        Public Property [Class] As WordClass
        <XmlAttribute>
        Public Property Text As String Implements Value(Of String).IValueOf.Value

        Public Overrides Function ToString() As String
            If [Class] = WordClass.NA Then
                Return Text
            Else
                Return $"{[Class].Description} {Text}"
            End If
        End Function
    End Class
End Namespace