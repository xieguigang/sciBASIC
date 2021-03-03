Imports System.Xml.Serialization

Namespace MathML

    Public Class Apply : Inherits symbols

        Public Property divide As mathOperator
        Public Property times As mathOperator
        Public Property plus As mathOperator
        Public Property power As mathOperator

        Public Property cn As constant

        <XmlElement("apply")>
        Public Property apply As Apply()

        Public ReadOnly Property [operator] As String
            Get
                If Not divide Is Nothing Then
                    Return "/"
                ElseIf Not times Is Nothing Then
                    Return "*"
                ElseIf Not plus Is Nothing Then
                    Return "+"
                ElseIf Not power Is Nothing Then
                    Return "^"
                Else
                    Return "-"
                End If
            End Get
        End Property
    End Class

End Namespace