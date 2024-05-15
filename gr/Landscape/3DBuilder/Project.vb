#Region "Microsoft.VisualBasic::c62d482cdccddd8af53a7e42e791c69d, gr\Landscape\3DBuilder\Project.vb"

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
    '    Code Lines: 33
    ' Comment Lines: 13
    '   Blank Lines: 8
    '     File Size: 1.80 KB


    '     Class Project
    ' 
    '         Properties: model, Thumbnail
    ' 
    '         Function: FromZipDirectory, GetMatrix, GetSurfaces
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Vendor_3mf.XML
Imports Microsoft.VisualBasic.Linq

Namespace Vendor_3mf

    Public Class Project

        ''' <summary>
        ''' ``*.3mf/Metadata/thumbnail.png``
        ''' </summary>
        ''' <returns></returns>
        Public Property Thumbnail As Image
        ''' <summary>
        ''' ``*.3mf/3D/3dmodel.model``
        ''' </summary>
        ''' <returns></returns>
        Public Property model As XmlModel3D

        Public Shared Function FromZipDirectory(dir$) As Project
            Return New Project With {
                .Thumbnail = $"{dir}/Metadata/thumbnail.png".LoadImage,
                .model = IO.Load3DModel(dir & "/3D/3dmodel.model")
            }
        End Function

        ''' <summary>
        ''' Get all of the 3D surface model data in this 3mf project.
        ''' </summary>
        ''' <param name="centraOffset"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(Optional centraOffset As Boolean = False) As Surface()
            If model Is Nothing Then
                Return {}
            Else
                Dim out As Surface() = model.GetSurfaces.ToArray

                If centraOffset Then
                    With out.Centra
                        out = .Offsets(out).ToArray
                    End With
                End If

                Return out
            End If
        End Function

        Public Function GetMatrix(Optional centraOffset As Boolean = False) As Matrix
            Return New Matrix(GetSurfaces(centraOffset))
        End Function
    End Class
End Namespace
