Imports System.Text

Namespace Text.Xml

    ''' <summary>
    ''' https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references
    ''' </summary>
    Public Module XmlEntity

        Public Function EscapingXmlEntity(str As String) As String
            Return New StringBuilder(str) _
                .Replace("&", "&amp;") _
                .Replace("""", "&quot;") _
                .Replace("'", "&apos;") _
                .Replace("<", "&lt;") _
                .Replace(">", "&gt;") _
                .ToString
        End Function

        Public Function UnescapingXmlEntity(str As String) As String
            Return New StringBuilder(str) _
                .Replace("&quot;", """") _
                .Replace("&apos;", "'") _
                .Replace("&lt;", "<") _
                .Replace("&gt;", ">") _
                .Replace("&amp;", "&") _
                .ToString
        End Function
    End Module
End Namespace