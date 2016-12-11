#Region "Microsoft.VisualBasic::74feed3fa17b614b3a44d8163c8e4168, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Colors\ColorExtensions.vb"

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

    Public Module ColorExtensions

        ''' <summary>
        ''' ``rgb(r,g,b)``
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RGBExpression(c As Color) As String
            Return $"rgb({c.R},{c.G},{c.B})"
        End Function

        ''' <summary>
        ''' ``rgb(a,r,g,b)``
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ARGBExpression(c As Color) As String
            Return $"rgb({c.A},{c.R},{c.G},{c.B})"
        End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' Reads all of the color property from <see cref="Color"/> and then creates the color dictionary based on the property name.
        ''' </summary>
        ''' <returns></returns>
        Private Function __getColorHash() As Dictionary(Of String, Color)
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
            __getColorHash()

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
        Const RGB_EXPRESSION As String = "\d+,\d+,\d+(,\d+)?"

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

            Dim s As String = Regex.Match(str, RGB_EXPRESSION).Value

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
            If a.A <> b.A Then
                Return False
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
    End Module
End Namespace
