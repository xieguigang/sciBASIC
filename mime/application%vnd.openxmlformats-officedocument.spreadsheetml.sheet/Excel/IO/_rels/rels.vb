Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Namespace XML._rels

    Public Class rels : Implements IXml

        Public Property document As OpenXml.rels

        Public Property Target(Id As String) As Relationship
            Get
                Return _document(Id)
            End Get
            Set(value As Relationship)
                _document(Id) = value
            End Set
        End Property

        Private Function filePath() As String Implements IXml.filePath
            Return "_rels/.rels"
        End Function

        Private Function toXml() As String Implements IXml.toXml
            Return document.GetXml
        End Function

        Public Shared Function Load(file As String) As rels
            Return New rels With {
                .document = file.LoadXml(Of OpenXml.rels)
            }
        End Function

        Public Overrides Function ToString() As String
            Return filePath()
        End Function
    End Class
End Namespace