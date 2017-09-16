Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Public Module IO

    ' /
    '  +------- <_rels>
    '           + .rels
    '  +------- <docProps>
    '  +------- <xl>
    '  +------- [Content_Types].xml

    Public Function CreateReader(xlsx$) As File
        Dim ROOT$ = App.GetAppSysTempFile(".zip", App.PID)

        Call GZip.ImprovedExtractToDirectory(xlsx, ROOT, Overwrite.Always)

        Dim contentType As ContentTypes = (ROOT & "/[Content_Types].xml").LoadXml(Of ContentTypes)
        Dim rels As New _rels(ROOT)
        Dim docProps As New docProps(ROOT)
        Dim xl As New xl(ROOT)
        Dim file As New File With {
            .ContentTypes = contentType,
            ._rels = rels,
            .docProps = docProps,
            .xl = xl,
            .FilePath = xlsx
        }

        Return file
    End Function

    <Extension>
    Public Function Save(xlsx As File, path$) As Boolean

    End Function
End Module
