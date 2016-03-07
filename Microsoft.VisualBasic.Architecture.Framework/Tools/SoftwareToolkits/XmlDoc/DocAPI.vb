Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SoftwareToolkits.XmlDoc

    <PackageNamespace("Assembly.Doc.API")>
    Public Module DocAPI

        <ExportAPI("Load")>
        Public Function Load(path As String) As Doc
            Return path.LoadXml(Of Doc)(preprocess:=AddressOf __trim)
        End Function

        Private Function __trim(doc As String) As String
            Return doc
        End Function
    End Module
End Namespace