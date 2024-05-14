#Region "Microsoft.VisualBasic::c42464b7010a0c31979890d94c4fbae7, www\Microsoft.VisualBasic.NETProtocol\Captcha.vb"

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

    '   Total Lines: 296
    '    Code Lines: 256
    ' Comment Lines: 16
    '   Blank Lines: 24
    '     File Size: 12.40 KB


    ' Class Captcha
    ' 
    '     Properties: CaptchaHeight, CaptchaImage, CaptchaValue, CaptchaWidth, TimeStamp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ToString
    ' 
    '     Sub: __generateCaptcha
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports System.Web
Imports System.Drawing

''' <summary>
''' Generate CAPTCHA graphics that you can use on websites to help authenticate users.
''' CAPTCHA is an abbreviation for Completely Automated Public Turing test to tell Computers and Humans Apart.
''' This method uses images Of words Or numbers that are, In theory, distorted And jumbled enough so that an 
''' optical character recognition program can't read them but a human should be able to do so easily.
''' 
''' (http://www.codeproject.com/Articles/43390/CAPTCHA-Graphic-in-ASP-NET)
''' </summary>
Public Class Captcha

    ''' <summary>
    ''' 返回给客户端的验证码图像
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CaptchaImage As Bitmap
    Public ReadOnly Property CaptchaHeight As Integer
    Public ReadOnly Property CaptchaWidth As Integer
    ''' <summary>
    ''' 保存在服务器的验证码字符串
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CaptchaValue As String
    Public ReadOnly Property TimeStamp As Long = Now.ToBinary

    ReadOnly CaptchaFont As Font
    ReadOnly RandomValue As New Random(Now.Millisecond)

    Protected Shared ReadOnly colors As String() = {
        "#F0F8FF",        'AliceBlue
        "#FAEBD7",        'AntiqueWhite
        "#00FFFF",        'Aqua
        "#7FFFD4",        'Aquamarine
        "#F0FFFF",        'Azure
        "#F5F5DC",        'Beige
        "#FFE4C4",        'Bisque
        "#000000",        'Black
        "#FFEBCD",        'BlanchedAlmond
        "#0000FF",        'Blue
        "#8A2BE2",        'BlueViolet 
        "#A52A2A",        'Brown 
        "#DEB887",        'BurlyWood 
        "#5F9EA0",        'CadetBlue 
        "#7FFF00",        'Chartreuse 
        "#D2691E",        'Chocolate 
        "#FF7F50",        'Coral 
        "#6495ED",        'CornflowerBlue 
        "#FFF8DC",        'Cornsilk 
        "#DC143C",        'Crimson 
        "#00FFFF",        'Cyan 
        "#00008B",        'DarkBlue 
        "#008B8B",        'DarkCyan 
        "#B8860B",        'DarkGoldenrod 
        "#A9A9A9",        'DarkGray 
        "#006400",        'DarkGreen
        "#BDB76B",        'DarkKhaki
        "#8B008B",        'DarkMagenta
        "#556B2F",        'DarkOliveGreen
        "#FF8C00",        'DarkOrange
        "#9932CC",        'DarkOrchid
        "#8B0000",        'DarkRed
        "#E9967A",        'DarkSalmon
        "#8FBC8B",        'DarkSeaGreen
        "#483D8B",        'DarkSlateBlue
        "#2F4F4F",        'DarkSlateGray
        "#00CED1",        'DarkTurquoise
        "#9400D3",        'DarkViolet
        "#FF1493",        'DeepPink
        "#00BFFF",        'DeepSkyBlue
        "#696969",        'DimGray
        "#1E90FF",        'DodgerBlue 
        "#B22222",        'Firebrick
        "#FFFAF0",        'FloralWhite
        "#228B22",        'ForestGreen
        "#FF00FF",        'Fuchsia
        "#DCDCDC",        'Gainsboro 
        "#F8F8FF",        'GhostWhite 
        "#FFD700",        'Gold
        "#DAA520",        'Goldenrod
        "#808080",        'Gray
        "#008000",        'Green
        "#ADFF2F",        'GreenYellow
        "#F0FFF0",        'Honeydew
        "#FF69B4",        'HotPink
        "#CD5C5C",        'IndianRed
        "#4B0082",        'Indigo
        "#FFFFF0",        'Ivory
        "#F0E68C",        'Khaki
        "#E6E6FA",        'Lavender
        "#FFF0F5",        'LavenderBlush
        "#7CFC00",        'LawnGreen
        "#FFFACD",        'LemonChiffon
        "#ADD8E6",        'LightBlue
        "#F08080",        'LightCoral 
        "#E0FFFF",        'LightCyan
        "#FAFAD2",        'LightGoldenrodYellow
        "#D3D3D3",        'LightGray
        "#90EE90",        'LightGreen
        "#FFB6C1",        'LightPink 
        "#FFA07A",        'LightSalmon
        "#20B2AA",        'LightSeaGreen
        "#87CEFA",        'LightSkyBlue 
        "#778899",        'LightSlateGray
        "#B0C4DE",        'LightSteelBlue
        "#FFFFE0",        'LightYellow
        "#00FF00",        'Lime
        "#32CD32",        'LimeGreen
        "#FAF0E6",        'Linen
        "#FF00FF",        'Magenta
        "#800000",        'Maroon
        "#66CDAA",        'MediumAquamarine 
        "#0000CD",        'MediumBlue
        "#BA55D3",        'MediumOrchid
        "#9370DB",        'MediumPurple
        "#3CB371",        'MediumSeaGreen
        "#7B68EE",        'MediumSlateBlue
        "#00FA9A",        'MediumSpringGreen
        "#48D1CC",        'MediumTurquoise
        "#C71585",        'MediumVioletRed
        "#191970",        'MidnightBlue
        "#F5FFFA",        'MintCream
        "#FFE4E1",        'MistyRose
        "#FFE4B5",        'Moccasin
        "#FFDEAD",        'NavajoWhite
        "#000080",        'Navy
        "#FDF5E6",        'OldLace
        "#808000",        'Olive
        "#6B8E23",        'OliveDrab
        "#FFA500",        'Orange
        "#FF4500",        'OrangeRed
        "#DA70D6",        'Orchid
        "#EEE8AA",        'PaleGoldenrod
        "#98FB98",        'PaleGreen
        "#AFEEEE",        'PaleTurquoise
        "#DB7093",        'PaleVioletRed
        "#FFEFD5",        'PapayaWhip
        "#FFDAB9",        'PeachPuff 
        "#CD853F",        'Peru 
        "#FFC0CB",        'Pink
        "#DDA0DD",        'Plum 
        "#B0E0E6",        'PowderBlue 
        "#800080",        'Purple
        "#FF0000",        'Red 
        "#BC8F8F",        'RosyBrown
        "#4169E1",        'RoyalBlue
        "#8B4513",        'SaddleBrown
        "#FA8072",        'Salmon
        "#F4A460",        'SandyBrown
        "#2E8B57",        'SeaGreen 
        "#FFF5EE",        'SeaShell
        "#A0522D",        'Sienna
        "#C0C0C0",        'Silver
        "#87CEEB",        'SkyBlue
        "#6A5ACD",        'SlateBlue
        "#708090",        'SlateGray
        "#FFFAFA",        'Snow
        "#00FF7F",        'SpringGreen
        "#4682B4",        'SteelBlue
        "#D2B48C",        'Tan
        "#008080",        'Teal
        "#D8BFD8",        'Thistle
        "#FF6347",        'Tomato
        "#FFFFFF",        'Transparent
        "#40E0D0",        'Turquoise
        "#EE82EE",        'Violet
        "#F5DEB3",        'Wheat
        "#FFFFFF",        'White
        "#F5F5F5",        'WhiteSmoke
        "#FFFF00",        'Yellow 
        "#9ACD32"        'YellowGreen
    }

    Const alphabets As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"

    Sub New(Optional Width As Integer = 120, Optional Height As Integer = 80, Optional Font As Font = Nothing)
        _CaptchaHeight = Height
        _CaptchaWidth = Width

        If Font Is Nothing Then
            CaptchaFont = New Font("Georgia", 30.0F,
                                   FontStyle.Bold,
                                   GraphicsUnit.Point,
                                   Convert.ToByte(0))
        Else
            CaptchaFont = Font
        End If

        Call __generateCaptcha()
    End Sub

    Private Sub __generateCaptcha()
        If CaptchaFont Is Nothing OrElse _CaptchaHeight = 0 OrElse _CaptchaWidth = 0 Then
            Return
        End If

        Dim HorizontalPosition As Integer
        Dim strValue As String = ""
        Dim captchaDigit As String = ""
        Dim strAlphaDisplay As String = "0100110100"

        captchaDigit = RandomValue.[Next](10000, 1000000).ToString()
        _CaptchaImage = New Bitmap(_CaptchaWidth, _CaptchaHeight)

        Using CaptchaGraphics As Graphics = Graphics.FromImage(CaptchaImage)

            Using BackgroundBrush As New Drawing2D.HatchBrush(   ' 填充背景和绘制栅格
                Drawing2D.HatchStyle.SmallGrid,
                Color.LightGray,
                Color.WhiteSmoke)
                Call CaptchaGraphics.FillRectangle(BackgroundBrush, 0, 0, _CaptchaWidth, _CaptchaHeight)
            End Using

            Dim CharacterSpacing As Integer = (_CaptchaWidth \ captchaDigit.Length) - 1
            Dim MaxVerticalPosition As Integer
            Dim rndbBrush As New Random()
            Dim rndAlphabet As New Random()
            Dim rndAlphaDisplay As New Random()
            Dim xchar As Char = " "c

            For Each CharValue As Char In captchaDigit.ToCharArray()  ' 绘制验证码字符串
                Dim intAlphaDisplay As Integer = rndAlphaDisplay.[Next](0, 9)
                If strAlphaDisplay.ToCharArray(intAlphaDisplay, 1).First() = "1"c Then
                    xchar = Convert.ToChar(alphabets(rndAlphabet.[Next](0, 51)).ToString())
                Else
                    xchar = CharValue
                End If
                strValue = strValue & xchar

                Dim intRand As Integer = rndbBrush.[Next](0, 139)
                Dim brush As Brush = New SolidBrush(ColorTranslator.FromHtml(colors(intRand).ToString()))
                MaxVerticalPosition = _CaptchaHeight -
                    Convert.ToInt32(CaptchaGraphics.MeasureString(xchar.ToString(), CaptchaFont).Height)
                Call CaptchaGraphics.DrawString(xchar.ToString(),
                                                CaptchaFont,
                                                brush,
                                                HorizontalPosition,
                                                RandomValue.[Next](0, MaxVerticalPosition))
                HorizontalPosition += CharacterSpacing + RandomValue.[Next](-1, 1)
                brush.Dispose()
            Next

            Dim RndmValue As New Random()
            Dim start As Integer = RndmValue.[Next](1, 4)

            For counter As Integer = 0 To 24  ' 随机填充颜色斑点
                Call CaptchaGraphics.FillEllipse(Brushes.Red,
                                                 RandomValue.[Next](start, _CaptchaWidth),
                                                 RandomValue.[Next](1, _CaptchaHeight),
                                                 RandomValue.[Next](1, 4),
                                                 RandomValue.[Next](2, 5))
                start = RndmValue.[Next](1, 4)
                Call CaptchaGraphics.FillEllipse(Brushes.Blue,
                                                 RandomValue.[Next](start, _CaptchaWidth),
                                                 RandomValue.[Next](1, _CaptchaHeight),
                                                 RandomValue.[Next](1, 6),
                                                 RandomValue.[Next](2, 7))
                start = RndmValue.[Next](1, 4)
                Call CaptchaGraphics.FillEllipse(Brushes.Green,
                                                 RandomValue.[Next](start, _CaptchaWidth),
                                                 RandomValue.[Next](1, _CaptchaHeight),
                                                 RandomValue.[Next](1, 4),
                                                 RandomValue.[Next](2, 6))
                start = RndmValue.[Next](1, 4)
                Call CaptchaGraphics.FillEllipse(Brushes.Yellow,
                                                 RandomValue.[Next](start, _CaptchaWidth),
                                                 RandomValue.[Next](1, _CaptchaHeight),
                                                 RandomValue.[Next](1, 5),
                                                 RandomValue.[Next](2, 6))
            Next

            Call CaptchaGraphics.Flush()
        End Using

        _CaptchaValue = strValue
        _CaptchaImage = _CaptchaImage
    End Sub

    Public Overrides Function ToString() As String
        Return CaptchaValue
    End Function

    Public Shared Narrowing Operator CType(captcha As Captcha) As String
        Return captcha.CaptchaValue
    End Operator

    Public Shared Narrowing Operator CType(captcha As Captcha) As Image
        Return captcha.CaptchaImage
    End Operator

    Public Shared Narrowing Operator CType(captcha As Captcha) As Bitmap
        Return captcha.CaptchaImage
    End Operator
End Class
