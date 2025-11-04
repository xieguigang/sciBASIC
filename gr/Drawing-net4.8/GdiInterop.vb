#Region "Microsoft.VisualBasic::d37d792367915213e2b5c3c671b27bfb, mzkit\mzblender\GdiInterop.vb"

    ' Author:
    ' 
    '       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
    ' 
    ' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
    ' 
    ' 
    ' MIT License
    ' 
    ' 
    ' Permission is hereby granted, free of charge, to any person obtaining a copy
    ' of this software and associated documentation files (the "Software"), to deal
    ' in the Software without restriction, including without limitation the rights
    ' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ' copies of the Software, and to permit persons to whom the Software is
    ' furnished to do so, subject to the following conditions:
    ' 
    ' The above copyright notice and this permission notice shall be included in all
    ' copies or substantial portions of the Software.
    ' 
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    ' SOFTWARE.



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
            Call image.Save(ms, ImageFormats.Bmp)
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
