#Region "Microsoft.VisualBasic::a12edc1feab42bc9eb2b0a6603c1c142, gr\Landscape\3DBuilder\IO.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



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
