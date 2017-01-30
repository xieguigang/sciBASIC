Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes.Type

Module Program

    Sub Main()

        Dim model = Landscape.IO.Load3DModel("G:\GCModeller\src\runtime\sciBASIC#\gr\3DEngineTest\example\3D\3dmodel.model").GetSurfaces.ToArray

        Dim dddd = "X:\[Content_Types].xml".LoadXml(Of Types)

        Dim types As New Types With {.Defaults = {New ContentTypes.Type With {.ContentType = "123", .Extension = "png"}}}
        Call types.SaveAsXml("x:\dddd.xml")
    End Sub
End Module
