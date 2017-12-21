#Region "Microsoft.VisualBasic::b40107c81ddc775e65ec933db6eaa70a, ..\sciBASIC#\tutorials\core.test\DefaultValueTest.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module DefaultValueTest

    Sub Main()

        'Dim [new] = [Default](New List(Of String))
        'Dim null As List(Of String) = Nothing

        'Dim x As List(Of String) = null Or [new]

        'Dim notnull As New List(Of String) From {"123"}

        'Dim y = notnull Or [new]

        'println("x:= %s", x.GetJson)
        'println("y:= %s", y.GetJson)

        'Pause()

        Call Draw()  ' using default font
        Call Draw(New Font(FontFace.Cambria, 36, FontStyle.Regular))  ' using user defined font

        Pause()
    End Sub

    Public Function Draw(Optional font As Font = Nothing)
        font = font Or MicrosoftYaHei.Large

        println(font.ToString)
    End Function
End Module
