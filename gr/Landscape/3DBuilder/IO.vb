#Region "Microsoft.VisualBasic::a12edc1feab42bc9eb2b0a6603c1c142, gr\Landscape\3DBuilder\IO.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module IO
    ' 
    '         Function: Load3DModel, NotNull, Open
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape.Vendor_3mf.XML
Imports Microsoft.VisualBasic.Text.Xml

Namespace Vendor_3mf

    Public Module IO

        ''' <summary>
        ''' Open ``*.3mf`` model file.
        ''' </summary>
        ''' <param name="zip$">``*.3mf``</param>
        ''' <returns></returns>
        Public Function Open(zip$) As Project
            Dim tmp$ = App.GetAppSysTempFile("--" & zip.FileName, sessionID:=App.PID)
            Call unzip.ImprovedExtractToDirectory(zip, tmp, Overwrite.Always)
            Return Project.FromZipDirectory(tmp)
        End Function

        Public Function Load3DModel(xml$) As XmlModel3D
            Dim doc As New XmlDoc(xml.ReadAllText)
            doc.xmlns.xmlns = Nothing
            doc.xmlns.Clear("m")

            Dim model As XmlModel3D = doc.CreateObject(Of XmlModel3D)(True)
            Return model
        End Function

        <Extension> Public Function NotNull(o As [object]) As Boolean
            Return Not o.mesh Is Nothing AndAlso
            Not o.mesh.vertices.IsNullOrEmpty
        End Function
    End Module
End Namespace
