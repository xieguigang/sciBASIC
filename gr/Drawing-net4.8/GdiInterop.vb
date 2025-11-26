#Region "Microsoft.VisualBasic::8575523a37c4c678779ddde90d879fe5, gr\Drawing-net4.8\GdiInterop.vb"

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

    '   Total Lines: 72
    '    Code Lines: 62 (86.11%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (13.89%)
    '     File Size: 2.00 KB


    ' Module GdiInterop
    ' 
    '     Function: (+2 Overloads) CTypeFromGdiImage, (+2 Overloads) CTypeGdiImage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

#If NET48 Then
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
#Else
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Public Module GdiInterop

    <Extension>
    Public Function CTypeGdiImage(image As Image) As System.Drawing.Image
#If NET48 Then
        Return image
#Else
        Using ms As New MemoryStream
            Call image.Save(ms, ImageFormats.Png)
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return System.Drawing.Image.FromStream(ms)
        End Using
#End If
    End Function

    <Extension>
    Public Function CTypeGdiImage(bitmap As Bitmap) As System.Drawing.Bitmap
#If NET48 Then
        Return bitmap
#Else
        Using ms As New MemoryStream
            Call bitmap.Save(ms, ImageFormats.Bmp)
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return System.Drawing.Bitmap.FromStream(ms)
        End Using
#End If
    End Function

    <Extension>
    Public Function CTypeFromGdiImage(image As System.Drawing.Image) As Image
#If NET48 Then
        Return image
#Else
        Using ms As New MemoryStream
            Call image.Save(ms, format:=ImageFormat.Png)
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return DriverLoad.LoadFromStream(ms)
        End Using
#End If
    End Function

    <Extension>
    Public Function CTypeFromGdiImage(bitmap As System.Drawing.Bitmap) As Bitmap
#If NET48 Then
        Return bitmap
#Else
        Using ms As New MemoryStream
            Call bitmap.Save(ms, ImageFormat.Bmp)
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return Microsoft.VisualBasic.Imaging.Bitmap.FromStream(ms)
        End Using
#End If
    End Function
End Module
