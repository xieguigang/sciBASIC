#Region "Microsoft.VisualBasic::bdc1c09bffdad9d62610d317cea0cdec, sciBASIC#\Data\OCR\Library.vb"

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

    '   Total Lines: 141
    '    Code Lines: 101
    ' Comment Lines: 17
    '   Blank Lines: 23
    '     File Size: 5.24 KB


    ' Class Library
    ' 
    '     Properties: Window
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: BuildLibraryTree, CompareTo, GetOpticals, Match, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language

''' <summary>
''' The character library tree
''' </summary>
Public Class Library

    ReadOnly font As Font

    Public Const Numeric$ = "0123456789.+-"
    Public Const Symbols$ = "/*-+()=^\<>,:;""'{}[]_&%$#@!~`?"
    Public Const enUS$ = Numeric & Symbols & "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"

    ReadOnly tree As BinaryTree(Of OpticalCharacter, Char)
    ReadOnly cutoff#
    ReadOnly compares As Comparison(Of OpticalCharacter)

    Public ReadOnly Property Window As Size

    Sub New(font As Font, Optional characters$ = enUS, Optional cutoff# = 0.98)
        Me.font = font
        Me.cutoff = cutoff
        Me.tree = BuildLibraryTree(font, characters, size:=Me.Window, cutoff:=cutoff)
        Me.compares = CompareTo(cutoff)
    End Sub

    Public Shared Function CompareTo(cutoff As Double) As Comparison(Of OpticalCharacter)
        Return Function(a As OpticalCharacter, b As OpticalCharacter) As Integer
                   Dim score# = a.Compare(b)

                   If score >= cutoff Then
                       Return 0
                   ElseIf score >= cutoff * 0.8 Then
                       Return 1
                   Else
                       Return -1
                   End If
               End Function
    End Function

    ''' <summary>
    ''' 查找失败的时候，返回来的分数值为-1，小于零
    ''' </summary>
    ''' <param name="OpticalCharacter"></param>
    ''' <returns></returns>
    Public Function Match(OpticalCharacter As OpticalCharacter) As (score#, recognized As Char)
        Dim find = tree.Find(OpticalCharacter, compares)

        If find Is Nothing Then
            Return (-1, Nothing)
        Else
            Return (find.Key.Compare(OpticalCharacter), find.Value)
        End If
    End Function

    Public Overrides Function ToString() As String
        Return $"[{font}, {Window}] {tree.PopulateNodes.Values.CharString}"
    End Function

    Public Shared Function BuildLibraryTree(font As Font, characters$, ByRef size As Size, Optional color$ = "black", Optional cutoff# = 0.98) As BinaryTree(Of OpticalCharacter, Char)
        Dim tree As New AVLTree(Of OpticalCharacter, Char)(Library.CompareTo(cutoff), Function(c) c.char)

        For Each optical As Map(Of Image, Char) In GetOpticals(font, characters, color.TranslateColor, size)
            Call tree.Add(
                key:=New OpticalCharacter(optical.Key, [char]:=optical.Maps),
                value:=optical.Maps,
                valueReplace:=False
            )
        Next

        Return tree.root
    End Function

    ''' <summary>
    ''' 因为默认的font大小可能会太大了，所以会需要这个函数来帮助获取平均大小
    ''' </summary>
    ''' <param name="font"></param>
    ''' <param name="characters$"></param>
    ''' <param name="color"></param>
    ''' <returns></returns>
    Private Shared Function GetOpticals(font As Font, characters$, color As Color, ByRef size As Size) As IEnumerable(Of Map(Of Image, Char))
        Dim fontColor As New SolidBrush(color)
        Dim ZERO As New Point(0, 0)
        Dim opticals As New List(Of Map(Of Image, Char))

        size = font.Height.SquareSize

        For Each c As Char In characters
            Dim optical As Image

            Using g As Graphics2D = size.CreateGDIDevice
                g.DrawString(c, font, fontColor, ZERO)
                optical = g.ImageResource.CorpBlank
            End Using

            opticals += New Map(Of Image, Char) With {
                .Key = optical,
                .Maps = c
            }
        Next

        ' 计算出最大的size
        ' 使用平均大小可能会导致比较大的字符被截断从而无法被识别
        Dim resize As New List(Of Map(Of Image, Char))
        Dim pos As Point
        Dim maxSize = opticals _
            .OrderByDescending(Function(op)
                                   Dim opSize = op.Key.Size
                                   Return opSize.Width * opSize.Height
                               End Function) _
            .First _
            .Key _
            .Size

        size = maxSize

        For Each op As Map(Of Image, Char) In opticals
            Using g As Graphics2D = size.CreateGDIDevice
                With op.Key.Size
                    pos = New Point(
                        x:=(size.Width - .Width) / 2,
                        y:=(size.Height - .Height) / 2
                    )
                    g.DrawImageUnscaled(op.Key, pos)
                    resize += New Map(Of Image, Char) With {
                        .Key = g.ImageResource,
                        .Maps = op.Maps
                    }
                End With
            End Using
        Next

        Return resize
    End Function
End Class
