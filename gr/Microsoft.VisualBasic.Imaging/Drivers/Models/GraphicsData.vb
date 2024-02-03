﻿#Region "Microsoft.VisualBasic::aff01acc33b6fc81473de30392c35c46, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drivers\Models\GraphicsData.vb"

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

    '   Total Lines: 125
    '    Code Lines: 65
    ' Comment Lines: 45
    '   Blank Lines: 15
    '     File Size: 4.76 KB


    '     Class GraphicsData
    ' 
    '         Properties: content_type, Height, Layout, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Driver

    ''' <summary>
    ''' gdi+ images: <see cref="Drawing.Image"/>, <see cref="Bitmap"/> / SVG image: <see cref="SVGDocument"/>
    ''' </summary>
    Public MustInherit Class GraphicsData
        Implements IDisposable

        ''' <summary>
        ''' The graphics engine driver type indicator, 
        ''' 
        ''' + for <see cref="Drivers.GDI"/> -> <see cref="ImageData"/>(<see cref="Drawing.Image"/>, <see cref="Bitmap"/>)
        ''' + for <see cref="Drivers.SVG"/> -> <see cref="SVGData"/>(<see cref="SVGDocument"/>)
        ''' 
        ''' (驱动程序的类型)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Driver As Drivers

        Public ReadOnly Property content_type As String
            Get
                Select Case Driver
                    Case Drivers.GDI
                        Return "image/png"
                    Case Drivers.PS
                        Return "application/postscript"
                    Case Drivers.SVG
                        Return "text/xml"
                    Case Drivers.WMF
                        Return "application/x-wmf"
                    Case Else
                        Return "application/octet-stream"
                End Select
            End Get
        End Property

        ''' <summary>
        ''' The image size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Layout As GraphicsRegion

        Public ReadOnly Property Width As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _Layout.Size.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _Layout.Size.Height
            End Get
        End Property

        ''' <summary>
        ''' This constructor of base type is only assign the size value
        ''' </summary>
        ''' <param name="img">其实这个参数在基类<see cref="GraphicsData"/>之中是无用的，只是为了统一接口而设置的</param>
        ''' <param name="size"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(img As Object, size As Size, padding As Padding)
            Me.Layout = New GraphicsRegion With {
                .Size = size,
                .Padding = padding
            }
        End Sub

        Public MustOverride Function GetDataURI() As DataURI

        ''' <summary>
        ''' Save the image graphics to file
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        Public MustOverride Function Save(path$) As Boolean
        ''' <summary>
        ''' Save the image graphics to a specific output stream
        ''' </summary>
        ''' <param name="out"></param>
        ''' <returns></returns>
        Public MustOverride Function Save(out As Stream) As Boolean

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub

        ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码以正确实现可释放模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
            Dispose(True)
            ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
