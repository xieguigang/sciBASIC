#Region "Microsoft.VisualBasic::e10d7871a6e6dc82ac6743fc2b2a43c9, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\GtkSkinEngine.vb"

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

Imports System.Text
Imports System.Xml.Serialization

Namespace API.Gtk

    ''' <summary>
    ''' Molk+ theme skin engine which can using the gtk theme file on Linux.(Molk+版本的Gtk窗口装饰器的皮肤引擎)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SkinEngine : Implements ISkinEngine

        Public Interface ISkinEngine
            Sub LoadDefault()
        End Interface

        Public Sub LoadDefault() Implements ISkinEngine.LoadDefault

        End Sub

        ''' <summary>
        ''' 对颜色进行中和替换
        ''' </summary>
        ''' <param name="bitmap"></param>
        ''' <param name="ExceptColor">本颜色的像素点将不会被中和掉，默认为白色</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ColorNeutralization(bitmap As Bitmap, MergeColor As Color, Optional ExceptColor As Color = Nothing) As Bitmap
            If ExceptColor = Nothing OrElse ExceptColor.IsEmpty Then
                ExceptColor = Color.White
            End If

            bitmap = DirectCast(bitmap.Clone, Bitmap)

            For i As Integer = 0 To bitmap.Width - 1
                For j As Integer = 0 To bitmap.Height - 1
                    Dim Color = bitmap.GetPixel(i, j)
                    If Color.R = ExceptColor.R AndAlso Color.G = ExceptColor.G AndAlso Color.B = ExceptColor.B Then
                        Continue For
                    End If
                    Color = System.Drawing.Color.FromArgb((MergeColor.R + Color.R) / 2, (MergeColor.G + Color.G) / 2, (MergeColor.B + Color.B) / 2)
                    Call bitmap.SetPixel(i, j, Color)
                Next
            Next

            Return bitmap
        End Function
    End Class

    Namespace File

        ''' <summary>
        ''' 皮肤引擎的皮肤数据存储结构
        ''' </summary>
        ''' <remarks></remarks>
        <Serializable> <XmlRoot("metacity_theme")> Public Class MetaCityTheme

            <XmlType("info")> Public Structure InfoF
                <XmlElement> Dim name As String
                <XmlElement> Dim author As String
                <XmlElement> Dim copyright As String
                <XmlElement> Dim [date] As String
                <XmlElement> Dim description As String
            End Structure

            Public Structure KeyValuePair(Of T)
                <XmlAttribute> Dim name As String
                <XmlAttribute> Dim value As T

                Public Overrides Function ToString() As String
                    Return Format(<String>[KeyValuePair name = "{0}" value = "{1}"]</String>, name, value.ToString)
                End Function
            End Structure

            Public Structure FrameGeometryF
                <XmlAttribute("name")> Dim Name As String
                <XmlAttribute("title_scale")> Dim TitleScale As String
                <XmlAttribute("rounded_top_left")>
                Dim RoundedTopLeft As String
                <XmlAttribute("rounded_top_right")>
                Dim RoundedTopRight As String
                <XmlAttribute("rounded_bottom_left")>
                Dim RoundedBottomLeft As String
                <XmlAttribute("rounded_bottom_right")>
                Dim RoundedBottomRight As String

                <XmlElement("distance")> Dim Distance As KeyValuePair(Of String)()

                <XmlType("border")>
                Public Structure BorderF
                    <XmlAttribute> Dim name As String
                    <XmlAttribute> Dim left As Integer
                    <XmlAttribute> Dim right As Integer
                    <XmlAttribute> Dim top As Integer
                    <XmlAttribute> Dim bottom As Integer

                    Public Overrides Function ToString() As String
                        Return Format(<String>[border name="{0}" left="{1}" right="{2}" top="{3}" bottom="{4}"]</String>, name, left, right, top, bottom)
                    End Function
                End Structure

                <XmlElement> Dim border As BorderF()
                Dim AspectRatio As KeyValuePair(Of String)

                Public Overrides Function ToString() As String
                    Return Format(<String>[frame_geometry name="{0}" title_scale="{1}" rounded_top_left="{2}" rounded_top_right="{3}" rounded_bottom_left="{4}" rounded_bottom_right="{5}"]</String>, Name, TitleScale, RoundedTopLeft, RoundedTopRight, RoundedBottomLeft, RoundedBottomRight)
                End Function
            End Structure

            <XmlElement> Public Property info As InfoF
            <XmlElement> Public Property constant As KeyValuePair(Of String)()
            <XmlElement("frame_geometry")> Public Property FrameGeometry As FrameGeometryF()
            <XmlElement("draw_ops")> Public Property DrawOps As DrawOpsF()
            <XmlElement("frame_style")> Public Property FrameStyle As FrameStyleF()
            <XmlElement("frame_style_set")> Public Property FrameStyleSet As FrameStyleSetF()
            <XmlElement> Public Property window As WindowF()

            <XmlIgnore> Friend FileName As String

            Public Overrides Function ToString() As String
                Return FileName
            End Function

            Public Structure DrawOpsF
                <XmlAttribute> Dim name As String

                <XmlElement> Dim title As TitleF()
                <XmlElement> Dim rectangle As RectangleF
                <XmlElement> Dim gradient As GradientF()
                <XmlElement> Dim line As LineF()
                <XmlElement> Dim image As ImageF

                Public Structure TitleF
                    <XmlAttribute> Dim color As String
                    <XmlAttribute> Dim x As String
                    <XmlAttribute> Dim y As String

                    Public Overrides Function ToString() As String
                        Return Format(<String>[title color="{0}" x="{1}" y="{2}"]</String>, color, x, y)
                    End Function
                End Structure

                Public Structure RectangleF
                    <XmlAttribute> Dim color As String
                    <XmlAttribute> Dim filled As String
                    <XmlAttribute> Dim x As String
                    <XmlAttribute> Dim y As String
                    <XmlAttribute> Dim width As String
                    <XmlAttribute> Dim height As String

                    Public Overrides Function ToString() As String
                        Return Format(<String>[rectangle color="{0}" filled="{1}" x="{2}" y="{3}" width="{4}" height="{5}"]</String>, color, filled, x, y, width, height)
                    End Function
                End Structure

                Public Structure GradientF
                    <XmlAttribute> Dim type As String
                    <XmlAttribute> Dim x As String
                    <XmlAttribute> Dim y As String
                    <XmlAttribute> Dim width As String
                    <XmlAttribute> Dim height As String

                    <XmlElement> Dim color As ColorF()

                    Public Structure ColorF
                        <XmlAttribute> Dim value As String

                        Public Overrides Function ToString() As String
                            Return Format(<String>[color value="{0}"]</String>, value)
                        End Function
                    End Structure

                    Public Overrides Function ToString() As String
                        Return Format(<String>[gradient type="{0}" x="{1}" y="{2}" width="{3}" height="{4}"]</String>, type, x, y, width, height)
                    End Function
                End Structure

                Public Structure LineF
                    <XmlAttribute("color")> Dim Color As String
                    <XmlAttribute("x1")> Dim X1 As String
                    <XmlAttribute("x2")> Dim X2 As String
                    <XmlAttribute("y1")> Dim Y1 As String
                    <XmlAttribute("y2")> Dim Y2 As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[line color="%s" x1="%x1" x2="%x2" y1="%y1" y2="%y2"]</ToString>.Value)
                        sbr.Replace("%s", Color)
                        sbr.Replace("%x1", X1)
                        sbr.Replace("%x2", X2)
                        sbr.Replace("%y1", Y1)
                        sbr.Replace("%y2", Y2)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Structure ImageF
                    <XmlAttribute("filename")> Dim FileName As String
                    <XmlAttribute("x")> Dim x As String
                    <XmlAttribute("y")> Dim y As String
                    <XmlAttribute("width")> Dim Width As String
                    <XmlAttribute("height")> Dim Height As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[image filename="%s" x="%x" y="%y" width="%w" height="%h"]</ToString>.Value)
                        sbr.Replace("%s", FileName)
                        sbr.Replace("%x", x)
                        sbr.Replace("%y", y)
                        sbr.Replace("%w", Width)
                        sbr.Replace("%h", Height)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Overrides Function ToString() As String
                    Return <DrawOps>draw_ops name="%s"</DrawOps>.Value.Replace("%s", name)
                End Function
            End Structure

            Public Structure FrameStyleF
                <XmlAttribute("name")> Dim Name As String
                <XmlAttribute("geometry")> Dim Geometry As String

                <XmlElement("piece")> Dim Piece As PieceF()
                <XmlElement("button")> Dim Button As ButtonF()
                <XmlElement("shadow")> Dim Shadow As ShadowF
                <XmlElement("padding")> Dim Padding As PaddingF

                Public Structure PieceF
                    <XmlAttribute("position")> Dim Position As String
                    <XmlAttribute("draw_ops")> Dim DrawOps As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[piece position="%s" draw_ops="%t"]</ToString>.Value)
                        sbr.Replace("%s", Position)
                        sbr.Replace("%t", DrawOps)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Structure ButtonF
                    <XmlAttribute("function")> Dim [Function] As String
                    <XmlAttribute("state")> Dim State As String
                    <XmlAttribute("draw_ops")> Dim DrawOps As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[button function="%s" state="%t" draw_ops="%d"]</ToString>.Value)
                        sbr.Replace("%s", [Function])
                        sbr.Replace("%t", State)
                        sbr.Replace("%d", DrawOps)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Structure ShadowF
                    <XmlAttribute("radius")> Dim Radius As String
                    <XmlAttribute("opacity")> Dim Opacity As String
                    <XmlAttribute("color")> Dim Color As String
                    <XmlAttribute("x_offset")> Dim XOffset As String
                    <XmlAttribute("y_offset")> Dim YOffset As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[shadow radius="%r" opacity="%o" color="%c" x_offset="%x" y_offset="%y"]</ToString>.Value)
                        sbr.Replace("%r", Radius)
                        sbr.Replace("%o", Opacity)
                        sbr.Replace("%c", Color)
                        sbr.Replace("%x", XOffset)
                        sbr.Replace("%y", YOffset)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Structure PaddingF
                    <XmlAttribute("left")> Dim Left As String
                    <XmlAttribute("right")> Dim Right As String
                    <XmlAttribute("bottom")> Dim Bottom As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[padding left="%l" right="%r" bottom="%b"]</ToString>.Value)
                        sbr.Replace("%l", Left)
                        sbr.Replace("%r", Right)
                        sbr.Replace("%b", Bottom)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Overrides Function ToString() As String
                    Dim sbr As StringBuilder = New StringBuilder(128)

                    sbr.Append(<ToString>[frame_style name="%s" geometry="%t"]</ToString>.Value)
                    sbr.Replace("%s", Name)
                    sbr.Replace("%t", Geometry)

                    Return sbr.ToString
                End Function
            End Structure

            Public Structure FrameStyleSetF
                <XmlAttribute("name")> Dim Name As String
                <XmlElement("frame")> Dim Frame As FrameF()

                Public Structure FrameF
                    <XmlAttribute("focus")> Dim Focus As String
                    <XmlAttribute("state")> Dim State As String
                    <XmlAttribute("resize")> Dim Resize As String
                    <XmlAttribute("style")> Dim Style As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[frame focus="%f" state="%t" resize="%r" style="%s"]</ToString>.Value)
                        sbr.Replace("%s", Style)
                        sbr.Replace("%f", Focus)
                        sbr.Replace("%t", State)
                        sbr.Replace("%r", Resize)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Overrides Function ToString() As String
                    Return <ToString>frame_style_set name="%s"</ToString>.Value.Replace("%s", Name)
                End Function
            End Structure

            Public Structure WindowF
                <XmlAttribute("type")> Dim Type As String
                <XmlAttribute("style_set")> Dim StyleSet As String

                Public Overrides Function ToString() As String
                    Dim sbr As StringBuilder = New StringBuilder(128)

                    sbr.Append(<ToString>[window type="%t" style_set="%s"]</ToString>.Value)
                    sbr.Replace("%s", StyleSet)
                    sbr.Replace("%t", Type)

                    Return sbr.ToString
                End Function
            End Structure

            Public Sub Save(spath As String)
                Using fs As New IO.FileStream(spath, IO.FileMode.Create)
                    Call (New Xml.Serialization.XmlSerializer(GetType(MetaCityTheme))).Serialize(fs, Me)
                End Using
            End Sub

            Public Shared Function Load(spath As String) As MetaCityTheme
                Using fs As New IO.FileStream(spath, IO.FileMode.Open)
                    Dim newObj = DirectCast(New Xml.Serialization.XmlSerializer(GetType(MetaCityTheme)).Deserialize(fs), MetaCityTheme)
                    newObj.FileName = spath
                    Return newObj
                End Using
            End Function

            Shared Widening Operator CType(spath As String) As MetaCityTheme
                Using fs As New IO.FileStream(spath, IO.FileMode.Open)
                    Dim newObj = DirectCast(New Xml.Serialization.XmlSerializer(GetType(MetaCityTheme)).Deserialize(fs), MetaCityTheme)
                    newObj.FileName = spath
                    Return newObj
                End Using
            End Operator
        End Class
    End Namespace
End Namespace
