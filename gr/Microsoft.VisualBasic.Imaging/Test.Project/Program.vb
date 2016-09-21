#Region "Microsoft.VisualBasic::930865294243bb0705e07fb8f771b61e, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Test.Project\Program.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text

Module Program

    Sub Main()

        Dim html As String = "<font face=""Microsoft YaHei"" size=""25.5""><strong>text</strong><b> &lt;&lt;&lt; <i><font face=""Ubuntu"" size=""12"">value</font></i></b></font> "

        Dim strings = TextAPI.GetStrings(html)

        Using gdi As GDIPlusDeviceHandle = New Size(500, 200).CreateGDIDevice
            Call strings.DrawStrng(New Point(100, 100), gdi)
            Call gdi.Save("./test_text.png", ImageFormats.Png)
        End Using

        Call New Form1().ShowDialog()

        Dim i As Integer = 1

        'For Each t In TextureResourceLoader.LoadInternalDefaultResource
        '    Call t.Save("x:\" & i & ".png")
        '    i += 1
        'Next


        '    Call TextureResourceLoader.AdjustColor(Microsoft.VisualBasic.Drawing.TextureResourceLoader.LoadInternalDefaultResource.First, Color.SeaGreen).Save("x:\reddddd.bmp")

        Dim vec = New Vectogram(1000, 1000)

        Call vec.AddTextElement("1234", New Font(FontFace.MicrosoftYaHei, 20), System.Drawing.Color.DarkCyan, vec.GDIDevice.Center)

        Call vec.AddCircle(Color.Red, New Point(100, 20), 100)

        Dim res = vec.ToImage

        Call res.Save("x:\ffff.png")

        Call New DrawingScript(vec).ToScript.SaveTo("script.txt")

    End Sub

End Module
