#Region "17c7f775bcb901b876c1f172c72a59b2, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GDI+\ColorExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

#If NET_40 = 0 Then

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
                Return __allDotNETPrefixColors.Values.Randomize
            End Get
        End Property
#End If
        ''' <summary>
        ''' 解析颜色表达式里面的RGB的正则表达式
        ''' </summary>
        Const RGB_EXPRESSION As String = "\d+,\d+,\d+"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="str">颜色表达式或者名称</param>
        ''' <returns></returns>
        <ExportAPI("Get.Color")>
        <Extension> Public Function ToColor(str As String) As Color
#If NET_40 = 0 Then
            If String.IsNullOrEmpty(str) Then
                Return Color.Black
            End If

            Dim s As String = Regex.Match(str, RGB_EXPRESSION).Value

            If String.IsNullOrEmpty(s) Then
                Dim key As String = str.ToLower

                If __allDotNETPrefixColors.ContainsKey(key) Then
                    Return __allDotNETPrefixColors(key)
                Else
                    Return Color.FromName(str)
                End If
            Else
                Dim tokens As String() = s.Split(","c)
                Dim R As Integer = CInt(Val(tokens(0)))
                Dim G As Integer = CInt(Val(tokens(1)))
                Dim B As Integer = CInt(Val(tokens(2)))

                Return Color.FromArgb(R, G, B)
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
