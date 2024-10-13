#Region "Microsoft.VisualBasic::a0fb6acef97a564d7d4ca6e54cd660aa, Microsoft.VisualBasic.Core\src\Drawing\GDI+\FontFace.vb"

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

    '   Total Lines: 148
    '    Code Lines: 89 (60.14%)
    ' Comment Lines: 37 (25.00%)
    '    - Xml Docs: 91.89%
    ' 
    '   Blank Lines: 22 (14.86%)
    '     File Size: 6.16 KB


    '     Class FontFace
    ' 
    '         Properties: InstalledFontFamilies
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetFontName, IsInstalled, MeasureString, PointSizeScale, (+2 Overloads) SVGPointSize
    ' 
    '     Module DefaultFontValues
    ' 
    ' 
    '         Class MicrosoftYaHei
    ' 
    '             Properties: Bold, Large, Normal
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver

#If NET48 Then
Imports DefaultFont = Microsoft.VisualBasic.Language.Default.Default(Of System.Drawing.Font)
#Else
Imports DefaultFont = Microsoft.VisualBasic.Language.Default.Default(Of Microsoft.VisualBasic.Imaging.Font)
#End If

Namespace Imaging

    ''' <summary>
    ''' Font names collection
    ''' </summary>
    Public NotInheritable Class FontFace

        ''' <summary>
        ''' 微软雅黑字体的名称
        ''' </summary>
        Public Const MicrosoftYaHei As String = "Microsoft YaHei"
        Public Const MicrosoftYaHeiUI As String = "Microsoft YaHei UI"
        Public Const Ubuntu As String = "Ubuntu"
        Public Const SegoeUI As String = "Segoe UI"
        Public Const Arial As String = "Arial"
        Public Const BookmanOldStyle As String = "Bookman Old Style"
        Public Const Calibri As String = "Calibri"
        Public Const Cambria As String = "Cambria"
        Public Const CambriaMath As String = "Cambria Math"
        Public Const Consolas As String = "Consolas"
        Public Const CourierNew As String = "Courier New"
        Public Const NSimSun As String = "NSimSun"
        Public Const SimSun As String = "SimSun"
        Public Const Verdana As String = "Verdana"
        Public Const Tahoma As String = "Tahoma"
        Public Const TimesNewRoman As String = "Times New Roman"

        Public Shared ReadOnly Property InstalledFontFamilies As IReadOnlyCollection(Of String)

        Shared ReadOnly fontFamilies As Dictionary(Of String, String)

        Shared Sub New()
#If NET48 Then
            Dim fontFamilies() As FontFamily
            Dim installedFontCollection As New InstalledFontCollection()

            ' Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families
            InstalledFontFamilies = fontFamilies.Select(Function(f) f.Name).ToArray
            FontFace.fontFamilies = New Dictionary(Of String, String)

            For Each family$ In InstalledFontFamilies
                FontFace.fontFamilies(LCase(family)) = family
            Next
#End If
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>
        ''' fix for dpi bugs on unix mono platform when create a font object.
        ''' 
        ''' https://github.com/dotnet/runtime/issues/28361
        ''' </summary>
        ''' <param name="pointSize"></param>
        ''' <param name="dpiResolution"></param>
        ''' <returns></returns>
        Public Shared Function PointSizeScale(pointSize As Single, dpiResolution As Single) As Single
            If Environment.OSVersion.Platform <> PlatformID.Win32NT Then
                ' fix for running on unix mono/dotnet core 
                Return If(App.IsMicrosoftPlatform, pointSize, pointSize * dpiResolution / 96)
            Else
                Return pointSize
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SVGPointSize(pointSize As Single, dpiResolution As Single) As Single
            Return pointSize * dpiResolution / 96
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SVGPointSize(size As SizeF, dpi As Single) As SizeF
            Return New SizeF(SVGPointSize(size.Width, dpi), SVGPointSize(size.Height, dpi))
        End Function

        ''' <summary>
        ''' 检查当前的操作系统之中是否安装有指定名称的字体
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsInstalled(name As String) As Boolean
            Return fontFamilies.ContainsKey(name) OrElse fontFamilies.ContainsKey(LCase(name))
        End Function

        ''' <summary>
        ''' 由于字体名称的大小写敏感，所以假若是html css之类的渲染的话，由于可能会是小写的字体名称会导致无法
        ''' 正确的加载所需要的字体，所以可以使用这个函数来消除这种由于大小写敏感而带来的bug
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="default">默认使用Windows10的默认字体</param>
        ''' <returns></returns>
        Public Shared Function GetFontName(name$, Optional default$ = FontFace.SegoeUI) As String
            If fontFamilies.ContainsKey(name) Then
                Return fontFamilies(name)
            Else
                name = LCase(name)

                If fontFamilies.ContainsKey(name) Then
                    Return fontFamilies(name)
                Else
                    Return [default]
                End If
            End If
        End Function

        ''' <summary>
        ''' A common shared method for measure text drawing size in gdi+ environment.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="font"></param>
        ''' <returns></returns>
        Public Shared Function MeasureString(s As String, font As Font) As SizeF
            Static g As IGraphics = DriverLoad.CreateGraphicsDevice(New Size(12, 12), driver:=Drivers.GDI)
            Return g.MeasureString(s, font)
        End Function

    End Class

    ''' <summary>
    ''' Default font values
    ''' </summary>
    Public Module DefaultFontValues

        Public NotInheritable Class MicrosoftYaHei

            Public Shared ReadOnly Property Normal As DefaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Regular)
            Public Shared ReadOnly Property Large As DefaultFont = New Font(FontFace.MicrosoftYaHei, 30, FontStyle.Regular)
            Public Shared ReadOnly Property Bold As DefaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)

            Private Sub New()
            End Sub
        End Class
    End Module
End Namespace
