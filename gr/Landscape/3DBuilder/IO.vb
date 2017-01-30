Imports Microsoft.VisualBasic.Text.Xml

Public Module IO

    Public Function Open(zip$) As Project

    End Function

    Public Function Load3DModel(xml$) As XmlModel3D
        Dim doc As New XmlDoc(xml.ReadAllText)
        doc.xmlns.xmlns = Nothing
        doc.xmlns.Clear("m")

        Dim model As XmlModel3D = doc.CreateObject(Of XmlModel3D)(True)
        Return model
    End Function
End Module
