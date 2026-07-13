#Region "Microsoft.VisualBasic::65512a59fcfd3e0229f33882a9a38a34, gr\Landscape\ThreeMF\ModelIO.vb"

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


    ' Code Statistics:

    '   Total Lines: 54
    '    Code Lines: 32 (59.26%)
    ' Comment Lines: 15 (27.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (12.96%)
    '     File Size: 1.95 KB


    '     Module ModelIO
    ' 
    '         Function: HasMesh, Load3DModel, NotNull, Open
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.Imaging.Landscape.ThreeMF.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace ThreeMF

    Public Module ModelIO

        ''' <summary>
        ''' Open ``*.3mf`` model file.
        ''' </summary>
        ''' <param name="zip$">``*.3mf``</param>
        ''' <returns></returns>
        Public Function Open(zip$) As Project
            Dim tmp$ = TempFileSystem.GetAppSysTempFile("--" & zip.FileName, sessionID:=App.PID)
            Call UnZip.ImprovedExtractToDirectory(zip, tmp, Overwrite.Always)
            Return Project.FromZipDirectory(tmp)
        End Function

        Public Function Load3DModel(xml$) As XmlModel3D
            Dim doc As New XmlDoc(xml.ReadAllText)
            doc.xmlns.xmlns = Nothing
            doc.xmlns.Clear("m")

            Dim model As XmlModel3D = doc.CreateObject(Of XmlModel3D)(True)
            Return model
        End Function

        ''' <summary>
        ''' 判断 3mf object 是否包含有效的 mesh 数据
        ''' </summary>
        ''' <param name="o"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NotNull(o As XmlModel3D) As Boolean
            Return Not o.resources Is Nothing AndAlso
                   Not o.resources.objects Is Nothing
        End Function

        ''' <summary>
        ''' 判断单个 3mf XML object 是否包含有效的 mesh 数据
        ''' </summary>
        ''' <param name="o"></param>
        ''' <returns></returns>
        <Extension>
        Public Function HasMesh(o As Object3D) As Boolean
            Return Not o Is Nothing AndAlso
                   Not o.mesh Is Nothing AndAlso
                   Not o.mesh.vertices.IsNullOrEmpty
        End Function
    End Module
End Namespace
