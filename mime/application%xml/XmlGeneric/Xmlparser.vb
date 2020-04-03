Imports System.IO

Public Module XmlParser

    Public Function ParseXml(xml As String) As XmlElement
        Dim doc As XDocument = XDocument.Load(New StringReader(xml))
        Dim root As XElement = doc.Root

        Return ParseXml(root)
    End Function

    Private Function ParseXml(root As XElement) As XmlElement
        Dim rootElement As New XmlElement With {
            .name = root.Name.LocalName,
            .[namespace] = root.Name.Namespace.ToString
        }

        If root.HasAttributes Then
            rootElement.attributes = New Dictionary(Of String, String)

            For Each attr In root.Attributes
                rootElement.attributes.Add(attr.Name.ToString, attr.Value)
            Next
        End If

        If root.HasElements Then
            Dim childs As New List(Of XmlElement)

            For Each child In root.Elements
                childs.Add(ParseXml(child))
            Next

            rootElement.elements = childs.ToArray
        Else
            rootElement.text = root.Value
        End If

        Return rootElement
    End Function
End Module