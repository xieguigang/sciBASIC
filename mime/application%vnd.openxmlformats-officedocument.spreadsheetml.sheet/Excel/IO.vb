Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Public Module IO

    ' /
    '  +------- <_rels>
    '  +------- <docProps>
    '  +------- <xl>
    '  +------- [Content_Types].xml

    Public Function CreateReader(xlsx$) As File
        Dim tmp$ = App.GetAppSysTempFile(".zip", App.PID)

        Call GZip.ImprovedExtractToDirectory(xlsx, tmp, Overwrite.Always)

        Dim contentType As ContentTypes = (tmp & "/[Content_Types].xml").LoadXml(Of ContentTypes)


        Return New File With {
            .ContentTypes = contentType
        }
    End Function
End Module
