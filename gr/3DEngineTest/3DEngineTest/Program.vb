Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes.Type

Module Program

    Sub Main()

        Dim dddd = "X:\[Content_Types].xml".LoadXml(Of Types)

        Dim types As New Types With {.Defaults = {New ContentTypes.Type With {.ContentType = "123", .Extension = "png"}}}
        Call types.SaveAsXml("x:\dddd.xml")
    End Sub
End Module
