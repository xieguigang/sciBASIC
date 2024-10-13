#Region "Microsoft.VisualBasic::cba66bc5764906a8c76972e977bc71f2, Microsoft.VisualBasic.Core\src\Drawing\Colors\ColorMap.vb"

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

    '   Total Lines: 354
    '    Code Lines: 256 (72.32%)
    ' Comment Lines: 61 (17.23%)
    '    - Xml Docs: 95.08%
    ' 
    '   Blank Lines: 37 (10.45%)
    '     File Size: 13.79 KB


    '     Module ColorMapsExtensions
    ' 
    '         Function: (+2 Overloads) ColorSequence, IsBlackOrWhite
    '         Delegate Function
    ' 
    ' 
    ' 
    '     Class ColorMap
    ' 
    '         Properties: Alpha, ColorMapLength, ColorMaps, MaxDepth
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Autumn, Cool, GetMaps, Gray, Hot
    '                   Jet, Spring, Summer, Winter
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Imaging

    ''' <summary>
    ''' Create Custom Color Maps
    ''' </summary>
    ''' <remarks>http://www.codeproject.com/Articles/18150/Create-Custom-Color-Maps-in-C</remarks>
    ''' 
    <Package("ColorMap", Publisher:="Jack J. H. Xu", Category:=APICategories.UtilityTools)>
    Public Module ColorMapsExtensions

        ''' <summary>
        ''' Is this given color value is near black or white
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsBlackOrWhite(c As Color, Optional offset As Integer = 5) As Boolean
            Dim upperBound As Integer = 255 - offset
            Dim isblack As Boolean = c.R <= offset AndAlso c.G <= offset AndAlso c.B <= offset
            Dim isWhite As Boolean = c.R >= upperBound AndAlso c.G >= upperBound AndAlso c.B >= upperBound

            Return isblack Or isWhite
        End Function

        ''' <summary>
        ''' <paramref name="mapName"/>大小写不敏感
        ''' </summary>
        ''' <param name="mapName"></param>
        ''' <param name="map"><see cref="ColorMap.MaxDepth"/></param>
        ''' <param name="min">value should smaller than <see cref="ColorMap.MaxDepth"/> in parameter <paramref name="map"/></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Color.Sequence")>
        <Extension>
        Public Function ColorSequence(map As ColorMap, Optional mapName$ = ColorMap.PatternJet, Optional min% = 0) As Color()
            Dim cMap%(,) = map.GetMaps(mapName)()
            Dim maxDepth% = map.MaxDepth
            Dim m% = maxDepth * 2
            Dim sequence As New List(Of Color)

            For i As Integer = 0 To maxDepth - 1
                Dim colorIndex% = CInt((i - min) * m \ (maxDepth - min))
                Dim a = cMap(colorIndex, 0)
                Dim r = cMap(colorIndex, 1)
                Dim g = cMap(colorIndex, 2)
                Dim b = cMap(colorIndex, 3)
                Dim cl As Color = Color.FromArgb(a, r, g, b)

                Call sequence.Add(cl)
            Next

            Return sequence.ToArray
        End Function

        ReadOnly __default As ColorMap = New ColorMap(64%)

        <ExportAPI("Color.Sequence")>
        Public Function ColorSequence(Optional min% = 0, Optional name$ = "Jet") As Color()
            SyncLock __default
                Return ColorSequence(__default, name, min)
            End SyncLock
        End Function

        Public Delegate Function MapsFunc() As Integer(,)
    End Module

    Public Class ColorMap

        Public ReadOnly Property ColorMapLength As Integer = 64

        ''' <summary>
        ''' Alpha value in the RBG color function.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Alpha As Integer = 255

        Public ReadOnly Property MaxDepth As Integer
            Get
                Return CInt(ColorMapLength / 2)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(colorLength%)
            ColorMapLength = colorLength
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="colorLength%"></param>
        ''' <param name="alpha%"><see cref="Color.A"/>: color alpha value</param>
        Sub New(colorLength%, alpha%)
            ColorMapLength = colorLength
            Me.Alpha = alpha
        End Sub

        Public ReadOnly Property ColorMaps As New Dictionary(Of String, MapsFunc) From {
 _
            {NameOf(Me.Autumn).ToLower, AddressOf Me.Autumn},
            {NameOf(Me.Cool).ToLower, AddressOf Me.Cool},
            {NameOf(Me.Gray).ToLower, AddressOf Me.Gray},
            {NameOf(Me.Hot).ToLower, AddressOf Me.Hot},
            {NameOf(Me.Jet).ToLower, AddressOf Me.Jet},
            {NameOf(Me.Spring).ToLower, AddressOf Me.Spring},
            {NameOf(Me.Summer).ToLower, AddressOf Me.Summer},
            {NameOf(Me.Winter).ToLower, AddressOf Me.Winter}
        }

        ''' <summary>
        ''' Spring
        ''' </summary>
        Public Const PatternSpring As String = "Spring"
        ''' <summary>
        ''' Summer
        ''' </summary>
        Public Const PatternSummer As String = "Summer"
        ''' <summary>
        ''' Autumn
        ''' </summary>
        Public Const PatternAutumn As String = "Autumn"
        ''' <summary>
        ''' Winter
        ''' </summary>
        Public Const PatternWinter As String = "Winter"
        ''' <summary>
        ''' Jet
        ''' </summary>
        Public Const PatternJet As String = "Jet"
        ''' <summary>
        ''' Gray
        ''' </summary>
        Public Const PatternGray As String = "Gray"
        ''' <summary>
        ''' Hot
        ''' </summary>
        Public Const PatternHot As String = "Hot"
        ''' <summary>
        ''' Cool
        ''' </summary>
        Public Const PatternCool As String = "Cool"

        ''' <summary>
        ''' If failure, then this function will returns <see cref="Jet"/> by default, 
        ''' or nothing if parameter <paramref name="noDefault"/> is set True.
        ''' </summary>
        ''' <param name="name">大小写不敏感</param>
        ''' <returns></returns>
        Public Function GetMaps(name As String, Optional noDefault As Boolean = False) As MapsFunc
            Static key As New Value(Of String)

            SyncLock key
                If Not name Is Nothing AndAlso
                    ColorMaps.ContainsKey(key = name.ToLower) Then

                    Return ColorMaps(key)
                Else
                    If noDefault Then
                        Return Nothing
                    Else
                        Return AddressOf Jet
                    End If
                End If
            End SyncLock
        End Function

#Region "Color Patterns"

        Public Function Spring() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim spring__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                spring__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = 255
                cmap(i, 2) = CInt(Truncate(255 * spring__1(i)))
                cmap(i, 3) = 255 - cmap(i, 1)
            Next
            Return cmap
        End Function

        Public Function Summer() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim summer__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                summer__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = CInt(Truncate(255 * summer__1(i)))
                cmap(i, 2) = CInt(Truncate(255 * 0.5F * (1 + summer__1(i))))
                cmap(i, 3) = CInt(Truncate(255 * 0.4F))
            Next
            Return cmap
        End Function

        Public Function Autumn() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim autumn__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                autumn__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = 255
                cmap(i, 2) = CInt(Truncate(255 * autumn__1(i)))
                cmap(i, 3) = 0
            Next
            Return cmap
        End Function

        Public Function Winter() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim winter__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                winter__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = 0
                cmap(i, 2) = CInt(Truncate(255 * winter__1(i)))
                cmap(i, 3) = CInt(Truncate(255 * (1.0F - 0.5F * winter__1(i))))
            Next
            Return cmap
        End Function

        Public Function Gray() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim gray__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                gray__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = CInt(Truncate(255 * gray__1(i)))
                cmap(i, 2) = CInt(Truncate(255 * gray__1(i)))
                cmap(i, 3) = CInt(Truncate(255 * gray__1(i)))
            Next
            Return cmap
        End Function

        ''' <summary>
        ''' *
        ''' </summary>
        ''' <returns></returns>
        Public Function Jet() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim cMatrix As Single(,) = New Single(ColorMapLength - 1, 2) {}
            Dim n As Integer = CInt(Truncate(Ceiling(ColorMapLength / 4.0F)))
            Dim nMod As Integer = 0
            Dim fArray As Single() = New Single(3 * n - 2) {}
            Dim red As Integer() = New Integer(fArray.Length - 1) {}
            Dim green As Integer() = New Integer(fArray.Length - 1) {}
            Dim blue As Integer() = New Integer(fArray.Length - 1) {}

            If ColorMapLength Mod 4 = 1 Then
                nMod = 1
            End If

            For i As Integer = 0 To fArray.Length - 1
                If i < n Then
                    fArray(i) = CSng(i + 1) / n
                ElseIf i >= n AndAlso i < 2 * n - 1 Then
                    fArray(i) = 1.0F
                ElseIf i >= 2 * n - 1 Then
                    fArray(i) = CSng(3 * n - 1 - i) / n
                End If
                green(i) = CInt(Truncate(Ceiling(n / 2.0F))) - nMod + i
                red(i) = green(i) + n
                blue(i) = green(i) - n
            Next

            Dim nb As Integer = 0
            For i As Integer = 0 To blue.Length - 1
                If blue(i) > 0 Then
                    nb += 1
                End If
            Next

            For i As Integer = 0 To ColorMapLength - 1
                For j As Integer = 0 To red.Length - 1
                    If i = red(j) AndAlso red(j) < ColorMapLength Then
                        cMatrix(i, 0) = fArray(i - red(0))
                    End If
                Next
                For j As Integer = 0 To green.Length - 1
                    If i = green(j) AndAlso green(j) < ColorMapLength Then
                        cMatrix(i, 1) = fArray(i - CInt(green(0)))
                    End If
                Next
                For j As Integer = 0 To blue.Length - 1
                    If i = blue(j) AndAlso blue(j) >= 0 Then
                        cMatrix(i, 2) = fArray(fArray.Length - 1 - nb + i)
                    End If
                Next
            Next

            For i As Integer = 0 To ColorMapLength - 1
                cmap(i, 0) = Alpha
                For j As Integer = 0 To 2
                    cmap(i, j + 1) = CInt(Truncate(cMatrix(i, j) * 255))
                Next
            Next
            Return cmap
        End Function

        Public Function Hot() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim n As Integer = 3 * ColorMapLength \ 8
            Dim red As Single() = New Single(ColorMapLength - 1) {}
            Dim green As Single() = New Single(ColorMapLength - 1) {}
            Dim blue As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                If i < n Then
                    red(i) = 1.0F * (i + 1) / n
                Else
                    red(i) = 1.0F
                End If
                If i < n Then
                    green(i) = 0F
                ElseIf i >= n AndAlso i < 2 * n Then
                    green(i) = 1.0F * (i + 1 - n) / n
                Else
                    green(i) = 1.0F
                End If
                If i < 2 * n Then
                    blue(i) = 0F
                Else
                    blue(i) = 1.0F * (i + 1 - 2 * n) / (ColorMapLength - 2 * n)
                End If
                cmap(i, 0) = Alpha
                cmap(i, 1) = CInt(Truncate(255 * red(i)))
                cmap(i, 2) = CInt(Truncate(255 * green(i)))
                cmap(i, 3) = CInt(Truncate(255 * blue(i)))
            Next
            Return cmap
        End Function

        Public Function Cool() As Integer(,)
            Dim cmap As Integer(,) = New Integer(ColorMapLength - 1, 3) {}
            Dim cool__1 As Single() = New Single(ColorMapLength - 1) {}
            For i As Integer = 0 To ColorMapLength - 1
                cool__1(i) = 1.0F * i / (ColorMapLength - 1)
                cmap(i, 0) = Alpha
                cmap(i, 1) = CInt(Truncate(255 * cool__1(i)))
                cmap(i, 2) = CInt(Truncate(255 * (1 - cool__1(i))))
                cmap(i, 3) = 255
            Next
            Return cmap
        End Function
#End Region
    End Class
End Namespace
