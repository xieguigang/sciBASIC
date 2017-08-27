#Region "Microsoft.VisualBasic::9daf5273fe3af8bcf515aed95ce7f5e2, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Colors\GDIColors.vb"

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
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language

Namespace Imaging

    ''' <summary>
    ''' Extensions function for the gdi+ color type.
    ''' </summary>
    Public Module GDIColors

        ''' <summary>
        ''' ``<paramref name="x"/> -> <paramref name="y"/>``：返回两个颜色之间的中间的颜色，这个函数是某些插值操作所需要的
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        <Extension> Public Function Middle(x As Color, y As Color) As Color
            Dim r% = (y.R - x.R) / 2 + x.R
            Dim g% = (y.G - x.G) / 2 + x.G
            Dim b% = (y.B - x.B) / 2 + x.B
            Dim a% = (y.A - x.A) / 2 + x.A
            Dim c As Color = Color.FromArgb(a, r, g, b)
            Return c
        End Function

        ''' <summary>
        ''' 调整所输入的这一组颜色的alpha值
        ''' </summary>
        ''' <param name="colors"></param>
        ''' <param name="alphaValue%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Alpha(colors As IEnumerable(Of Color), alphaValue%) As Color()
            Dim out As New List(Of Color)
            For Each c As Color In colors
                With c
                    out += Color.FromArgb(alphaValue, .R, .G, .B)
                End With
            Next
            Return out
        End Function

        <Extension>
        Public Function Average(colors As IEnumerable(Of Color)) As Color
            Dim data As Color() = colors.ToArray
            Dim A% = data.Select(Function(c) CDbl(c.A)).Average
            Dim R% = data.Select(Function(c) CDbl(c.R)).Average
            Dim G% = data.Select(Function(c) CDbl(c.G)).Average
            Dim B% = data.Select(Function(c) CDbl(c.B)).Average

            Return Color.FromArgb(A, R, G, B)
        End Function

        ''' <summary>
        ''' Creates a new light color object for the control from the specified color and
        ''' lightens it by the specified percentage.
        ''' </summary>
        ''' <param name="base">The <see cref="System.Drawing.Color"/> to be lightened.</param>
        ''' <param name="percent!">The percentage to lighten the specified <see cref="System.Drawing.Color"/>.</param>
        ''' <returns>A <see cref="System.Drawing.Color"/> that represents the light color on the control.</returns>
        <Extension>
        Public Function Light(base As Color, percent!) As Color
            Return ControlPaint.Light(base, percent)
        End Function

        ''' <summary>
        ''' Creates a new dark color object for the control from the specified color and
        ''' darkens it by the specified percentage.
        ''' </summary>
        ''' <param name="base">The <see cref="System.Drawing.Color"/> to be darkened.</param>
        ''' <param name="percent!">The percentage to darken the specified <see cref="System.Drawing.Color"/>.</param>
        ''' <returns>A <see cref="System.Drawing.Color"/> that represent the dark color on the control.</returns>
        <Extension>
        Public Function Dark(base As Color, percent!) As Color
            Return ControlPaint.Dark(base, percent)
        End Function

        ''' <summary>
        ''' ``rgb(r,g,b)``
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RGBExpression(c As Color) As String
            With c
                Return $"rgb({ .R},{ .G},{ .B})"
            End With
        End Function

        ''' <summary>
        ''' ``rgb(a,r,g,b)``
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ARGBExpression(c As Color) As String
            With c
                Return $"rgb({ .A},{ .R},{ .G},{ .B})"
            End With
        End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' Reads all of the color property from <see cref="Color"/> and then creates the color dictionary based on the property name.
        ''' </summary>
        ''' <returns></returns>
        Private Function __getDotNetColors() As Dictionary(Of String, Color)
            Dim props As IEnumerable(Of PropertyInfo) =
                GetType(Color).GetProperties(BindingFlags.Public Or BindingFlags.Static)
            Dim getValues = From p As PropertyInfo  ' Gets all of the known name color from the Color object its shared property.
                            In props
                            Where p.PropertyType = GetType(Color)
                            Let ColorValue As Color = DirectCast(p.GetValue(Nothing), Color)
                            Select name = p.Name,
                                ColorValue
            Dim hash As Dictionary(Of String, Color) =
                getValues.ToDictionary(Function(x) x.name.ToLower,
                                       Function(x) x.ColorValue)
            Return hash
        End Function

        ''' <summary>
        ''' Key都是小写的
        ''' </summary>
        ReadOnly __allDotNETPrefixColors As Dictionary(Of String, Color) =
            __getDotNetColors()

        ''' <summary>
        ''' Gets all of the known name color from the Color object its shared property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AllDotNetPrefixColors As Color()
            Get
                Return __allDotNETPrefixColors.Values.Shuffles
            End Get
        End Property

        ''' <summary>
        ''' 经过人工筛选的颜色，不会出现过白或者过黑，过度相似的情况
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ChartColors As Color() = {
            Color.AliceBlue, Color.Aquamarine, Color.BlueViolet, Color.BurlyWood,
            Color.CadetBlue, Color.Chartreuse, Color.Chocolate, Color.Coral,
            Color.CornflowerBlue, Color.Crimson, Color.Cyan, Color.DarkBlue,
            Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkMagenta,
            Color.DarkOliveGreen, Color.DarkOrchid, Color.DarkSeaGreen, Color.DarkSlateBlue,
            Color.DarkSlateGray, Color.DeepPink, Color.DeepSkyBlue, Color.DodgerBlue,
            Color.GreenYellow, Color.ForestGreen, Color.Firebrick, Color.Gold, Color.Indigo,
            Color.LightSeaGreen, Color.LightSkyBlue, Color.LimeGreen, Color.MediumSeaGreen,
            Color.MediumTurquoise, Color.MidnightBlue, Color.Orchid, Color.OrangeRed, Color.Red,
            Color.RoyalBlue, Color.SeaGreen, Color.SpringGreen, Color.SteelBlue, Color.Teal,
            Color.YellowGreen
        }
#End If
        ''' <summary>
        ''' Regex expression for parsing the rgb(a,r,g,b) expression of the color.(解析颜色表达式里面的RGB的正则表达式)
        ''' </summary>
        Public Const rgbExprValues$ = "\d+\s*,\s*\d+\s*,\s*\d+\s*(,\s*\d+)?"
        Public Const rgbExpr$ = "rgb\(" & rgbExprValues & "\)"

        ''' <summary>
        ''' <see cref="Color"/>.Name, rgb(a,r,g,b)
        ''' </summary>
        ''' <param name="str">颜色表达式或者名称</param>
        ''' <returns></returns>
        <ExportAPI("Get.Color")>
        <Extension> Public Function ToColor(str As String, Optional onFailure As Color = Nothing, Optional throwEx As Boolean = True) As Color
#If NET_40 = 0 Then
            If String.IsNullOrEmpty(str) Then
                Return Color.Black
            ElseIf str.TextEquals("transparent") Then
                Return Color.Transparent
            End If

            Dim s As String = Regex.Match(str, rgbExprValues).Value

            If String.IsNullOrEmpty(s) Then ' Color from name/known color
                Dim key As String = str.ToLower

                If __allDotNETPrefixColors.ContainsKey(key) Then
                    Return __allDotNETPrefixColors(key)
                Else
                    Return Color.FromName(str)
                End If
            Else
                Dim tokens As String() = s.Split(","c)

                If tokens.Length = 3 Then  ' rgb
                    Dim R As Integer = CInt(Val(tokens(0)))
                    Dim G As Integer = CInt(Val(tokens(1)))
                    Dim B As Integer = CInt(Val(tokens(2)))

                    Return Color.FromArgb(R, G, B)
                ElseIf tokens.Length = 4 Then ' argb
                    Dim A As Integer = CInt(Val(tokens(0)))
                    Dim R As Integer = CInt(Val(tokens(1)))
                    Dim G As Integer = CInt(Val(tokens(2)))
                    Dim B As Integer = CInt(Val(tokens(3)))

                    Return Color.FromArgb(A, R, G, B)
                Else
                    If Not onFailure.IsEmpty Then
                        Return onFailure
                    Else
                        If throwEx Then
                            Throw New Exception("Unable parsing any color information from expression: " & str)
                        Else
                            Return Nothing
                        End If
                    End If
                End If
            End If
#Else
            Throw New NotSupportedException
#End If
        End Function

        ''' <summary>
        ''' 这个函数会尝试用不同的模式来解析颜色表达式
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <returns></returns>
        <Extension> Public Function TranslateColor(exp$) As Color
            If exp.StringEmpty Then
                Return Color.Black
            End If
            If exp.First = "#"c Then
                ' 2017-2-2
                ' 经过测试与3mf文件之中的材质颜色定义一致，没有问题
                Return HexColor.ConvertToRbg(exp)
            End If
            If Regex.Match(exp, "\d+").Value = exp Then
                Return ColorTranslator.FromOle(CInt(exp))
            End If

            Return exp.ToColor
        End Function

        <Extension>
        Public Function IsColorExpression(expression$) As Boolean
            If expression.MatchPattern(rgbExpr, RegexICSng) Then
                Return True
            ElseIf __allDotNETPrefixColors.ContainsKey(expression.ToLower) Then
                Return True
            ElseIf expression.MatchPattern("\d+") Then
                Return True
            ElseIf expression.MatchPattern("#[a-z0-9]+", RegexICSng) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Determine that the target color value is a empty variable.(判断目标颜色值是否为空值)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function IsNullOrEmpty(Color As Color) As Boolean
            Return Color = Nothing OrElse Color.IsEmpty
        End Function

        ''' <summary>
        ''' 分别比较A,R,G,B这些属性值来判断这样个颜色对象值是否相等
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension> Public Function Equals(a As Color, b As Color) As Boolean
            If a.A = b.A Then
                If a.A = 0 Then
                    ' 只要是alpha值为零，肯定是透明色
                    ' 在这里判定为相同的颜色
                    Return True
                End If
            Else
                Return False '  alpha值不相等，则颜色值肯定不相等
            End If

            If a.B <> b.B Then
                Return False
            End If
            If a.G <> b.G Then
                Return False
            End If
            If a.R <> b.R Then
                Return False
            End If

            Return True
        End Function

        <Extension>
        Public Function EuclideanDistance(a As Color, b As Color) As Double
            Return Math.EuclideanDistance({a.R, a.G, a.B}, {b.R, b.G, b.B})
        End Function
    End Module
End Namespace
