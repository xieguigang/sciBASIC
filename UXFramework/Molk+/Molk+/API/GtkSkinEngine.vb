Imports System.Text

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
        <Serializable> <Xml.Serialization.XmlRoot("metacity_theme")> Public Class MetaCityTheme

            <Xml.Serialization.XmlType("info")> Public Structure InfoF
                <Xml.Serialization.XmlElement> Dim name As String
                <Xml.Serialization.XmlElement> Dim author As String
                <Xml.Serialization.XmlElement> Dim copyright As String
                <Xml.Serialization.XmlElement> Dim [date] As String
                <Xml.Serialization.XmlElement> Dim description As String
            End Structure

            Public Structure KeyValuePair(Of T)
                <Xml.Serialization.XmlAttribute> Dim name As String
                <Xml.Serialization.XmlAttribute> Dim value As T

                Public Overrides Function ToString() As String
                    Return Format(<String>[KeyValuePair name = "{0}" value = "{1}"]</String>, name, value.ToString)
                End Function
            End Structure

            Public Structure FrameGeometryF
                <Xml.Serialization.XmlAttribute("name")> Dim Name As String
                <Xml.Serialization.XmlAttribute("title_scale")> Dim TitleScale As String
                <Xml.Serialization.XmlAttribute("rounded_top_left")>
                Dim RoundedTopLeft As String
                <Xml.Serialization.XmlAttribute("rounded_top_right")>
                Dim RoundedTopRight As String
                <Xml.Serialization.XmlAttribute("rounded_bottom_left")>
                Dim RoundedBottomLeft As String
                <Xml.Serialization.XmlAttribute("rounded_bottom_right")>
                Dim RoundedBottomRight As String

                <Xml.Serialization.XmlElement("distance")> Dim Distance As KeyValuePair(Of String)()

                <Xml.Serialization.XmlType("border")>
                Public Structure BorderF
                    <Xml.Serialization.XmlAttribute> Dim name As String
                    <Xml.Serialization.XmlAttribute> Dim left As Integer
                    <Xml.Serialization.XmlAttribute> Dim right As Integer
                    <Xml.Serialization.XmlAttribute> Dim top As Integer
                    <Xml.Serialization.XmlAttribute> Dim bottom As Integer

                    Public Overrides Function ToString() As String
                        Return Format(<String>[border name="{0}" left="{1}" right="{2}" top="{3}" bottom="{4}"]</String>, name, left, right, top, bottom)
                    End Function
                End Structure

                <Xml.Serialization.XmlElement> Dim border As BorderF()
                Dim AspectRatio As KeyValuePair(Of String)

                Public Overrides Function ToString() As String
                    Return Format(<String>[frame_geometry name="{0}" title_scale="{1}" rounded_top_left="{2}" rounded_top_right="{3}" rounded_bottom_left="{4}" rounded_bottom_right="{5}"]</String>, Name, TitleScale, RoundedTopLeft, RoundedTopRight, RoundedBottomLeft, RoundedBottomRight)
                End Function
            End Structure

            <Xml.Serialization.XmlElement> Public Property info As InfoF
            <Xml.Serialization.XmlElement> Public Property constant As KeyValuePair(Of String)()
            <Xml.Serialization.XmlElement("frame_geometry")> Public Property FrameGeometry As FrameGeometryF()
            <Xml.Serialization.XmlElement("draw_ops")> Public Property DrawOps As DrawOpsF()
            <Xml.Serialization.XmlElement("frame_style")> Public Property FrameStyle As FrameStyleF()
            <Xml.Serialization.XmlElement("frame_style_set")> Public Property FrameStyleSet As FrameStyleSetF()
            <Xml.Serialization.XmlElement> Public Property window As WindowF()

            <Xml.Serialization.XmlIgnore> Friend FileName As String

            Public Overrides Function ToString() As String
                Return FileName
            End Function

            Public Structure DrawOpsF
                <Xml.Serialization.XmlAttribute> Dim name As String

                <Xml.Serialization.XmlElement> Dim title As TitleF()
                <Xml.Serialization.XmlElement> Dim rectangle As RectangleF
                <Xml.Serialization.XmlElement> Dim gradient As GradientF()
                <Xml.Serialization.XmlElement> Dim line As LineF()
                <Xml.Serialization.XmlElement> Dim image As ImageF

                Public Structure TitleF
                    <Xml.Serialization.XmlAttribute> Dim color As String
                    <Xml.Serialization.XmlAttribute> Dim x As String
                    <Xml.Serialization.XmlAttribute> Dim y As String

                    Public Overrides Function ToString() As String
                        Return Format(<String>[title color="{0}" x="{1}" y="{2}"]</String>, color, x, y)
                    End Function
                End Structure

                Public Structure RectangleF
                    <Xml.Serialization.XmlAttribute> Dim color As String
                    <Xml.Serialization.XmlAttribute> Dim filled As String
                    <Xml.Serialization.XmlAttribute> Dim x As String
                    <Xml.Serialization.XmlAttribute> Dim y As String
                    <Xml.Serialization.XmlAttribute> Dim width As String
                    <Xml.Serialization.XmlAttribute> Dim height As String

                    Public Overrides Function ToString() As String
                        Return Format(<String>[rectangle color="{0}" filled="{1}" x="{2}" y="{3}" width="{4}" height="{5}"]</String>, color, filled, x, y, width, height)
                    End Function
                End Structure

                Public Structure GradientF
                    <Xml.Serialization.XmlAttribute> Dim type As String
                    <Xml.Serialization.XmlAttribute> Dim x As String
                    <Xml.Serialization.XmlAttribute> Dim y As String
                    <Xml.Serialization.XmlAttribute> Dim width As String
                    <Xml.Serialization.XmlAttribute> Dim height As String

                    <Xml.Serialization.XmlElement> Dim color As ColorF()

                    Public Structure ColorF
                        <Xml.Serialization.XmlAttribute> Dim value As String

                        Public Overrides Function ToString() As String
                            Return Format(<String>[color value="{0}"]</String>, value)
                        End Function
                    End Structure

                    Public Overrides Function ToString() As String
                        Return Format(<String>[gradient type="{0}" x="{1}" y="{2}" width="{3}" height="{4}"]</String>, type, x, y, width, height)
                    End Function
                End Structure

                Public Structure LineF
                    <Xml.Serialization.XmlAttribute("color")> Dim Color As String
                    <Xml.Serialization.XmlAttribute("x1")> Dim X1 As String
                    <Xml.Serialization.XmlAttribute("x2")> Dim X2 As String
                    <Xml.Serialization.XmlAttribute("y1")> Dim Y1 As String
                    <Xml.Serialization.XmlAttribute("y2")> Dim Y2 As String

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
                    <Xml.Serialization.XmlAttribute("filename")> Dim FileName As String
                    <Xml.Serialization.XmlAttribute("x")> Dim x As String
                    <Xml.Serialization.XmlAttribute("y")> Dim y As String
                    <Xml.Serialization.XmlAttribute("width")> Dim Width As String
                    <Xml.Serialization.XmlAttribute("height")> Dim Height As String

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
                <Xml.Serialization.XmlAttribute("name")> Dim Name As String
                <Xml.Serialization.XmlAttribute("geometry")> Dim Geometry As String

                <Xml.Serialization.XmlElement("piece")> Dim Piece As PieceF()
                <Xml.Serialization.XmlElement("button")> Dim Button As ButtonF()
                <Xml.Serialization.XmlElement("shadow")> Dim Shadow As ShadowF
                <Xml.Serialization.XmlElement("padding")> Dim Padding As PaddingF

                Public Structure PieceF
                    <Xml.Serialization.XmlAttribute("position")> Dim Position As String
                    <Xml.Serialization.XmlAttribute("draw_ops")> Dim DrawOps As String

                    Public Overrides Function ToString() As String
                        Dim sbr As StringBuilder = New StringBuilder(128)

                        sbr.Append(<ToString>[piece position="%s" draw_ops="%t"]</ToString>.Value)
                        sbr.Replace("%s", Position)
                        sbr.Replace("%t", DrawOps)

                        Return sbr.ToString
                    End Function
                End Structure

                Public Structure ButtonF
                    <Xml.Serialization.XmlAttribute("function")> Dim [Function] As String
                    <Xml.Serialization.XmlAttribute("state")> Dim State As String
                    <Xml.Serialization.XmlAttribute("draw_ops")> Dim DrawOps As String

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
                    <Xml.Serialization.XmlAttribute("radius")> Dim Radius As String
                    <Xml.Serialization.XmlAttribute("opacity")> Dim Opacity As String
                    <Xml.Serialization.XmlAttribute("color")> Dim Color As String
                    <Xml.Serialization.XmlAttribute("x_offset")> Dim XOffset As String
                    <Xml.Serialization.XmlAttribute("y_offset")> Dim YOffset As String

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
                    <Xml.Serialization.XmlAttribute("left")> Dim Left As String
                    <Xml.Serialization.XmlAttribute("right")> Dim Right As String
                    <Xml.Serialization.XmlAttribute("bottom")> Dim Bottom As String

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
                <Xml.Serialization.XmlAttribute("name")> Dim Name As String
                <Xml.Serialization.XmlElement("frame")> Dim Frame As FrameF()

                Public Structure FrameF
                    <Xml.Serialization.XmlAttribute("focus")> Dim Focus As String
                    <Xml.Serialization.XmlAttribute("state")> Dim State As String
                    <Xml.Serialization.XmlAttribute("resize")> Dim Resize As String
                    <Xml.Serialization.XmlAttribute("style")> Dim Style As String

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
                <Xml.Serialization.XmlAttribute("type")> Dim Type As String
                <Xml.Serialization.XmlAttribute("style_set")> Dim StyleSet As String

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