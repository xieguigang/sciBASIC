Imports Microsoft.VisualBasic.Text.Xml

Namespace XML._rels

    Public Class rels : Inherits OpenXml.rels
        Implements IXml

        Private Function filePath() As String Implements IXml.filePath
            Return "_rels/.rels"
        End Function

        Private Function toXml() As String Implements IXml.toXml
            Return Me.GetXml
        End Function

        Public Overrides Function ToString() As String
            Return filePath()
        End Function
    End Class
End Namespace