#Region "Microsoft.VisualBasic::662f4ba978dadbe15d91cb4a491bf668, ..\sciBASIC#\gr\Landscape\3DBuilder\IO.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text.Xml

Public Module IO

    ''' <summary>
    ''' Open ``*.3mf`` model file.
    ''' </summary>
    ''' <param name="zip$">``*.3mf``</param>
    ''' <returns></returns>
    Public Function Open(zip$) As Project
        Dim tmp$ = App.GetAppSysTempFile(sessionID:=App.PID)
        Call GZip.ImprovedExtractToDirectory(zip, tmp, Overwrite.Always)
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

