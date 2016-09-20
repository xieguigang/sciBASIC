#Region "Microsoft.VisualBasic::c2acbf11bdd0155bff2dede994182f84, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing2D\VectorObject.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Public MustInherit Class VectorObject

    Dim __rectangle As Rectangle

    Public Property RECT As Rectangle
        Get
            Return __rectangle
        End Get
        Protected Set(value As Rectangle)
            __rectangle = value
        End Set
    End Property

    Sub New(locat As Point, size As Size)
        RECT = New Rectangle(locat, size)
    End Sub

    Sub New(rect As Rectangle)
        Me.RECT = rect
    End Sub

    Public Overridable Sub Draw(gdi As GDIPlusDeviceHandle)
        Call Draw(gdi, RECT)
    End Sub

    Public MustOverride Sub Draw(gdi As GDIPlusDeviceHandle, loci As Rectangle)

    Public Overrides Function ToString() As String
        Return RECT.GetJson
    End Function
End Class
