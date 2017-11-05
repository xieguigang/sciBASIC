#Region "Microsoft.VisualBasic::d5e966de724b60790aae44b3a1a6e41c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Terminal\ProgressBar\AnimatedBar.vb"

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

Namespace Terminal.ProgressBar

    Public Class AnimatedBar
        Inherits AbstractBar

        Dim animation As List(Of String)
        Dim counter As Integer

        Public Sub New()
            MyBase.New()
            Me.animation = New List(Of String)() From {"/", "-", "\", "|"}
            Me.counter = 0
        End Sub

        ''' <summary>
        ''' prints the character found in the animation according to the current index
        ''' </summary>
        Public Overrides Sub [Step]()
            Console.Write(vbCr)
            Console.Write(Me.animation(Me.counter) & vbBack)
            Me.counter += 1
            If Me.counter = Me.animation.Count Then
                Me.counter = 0
            End If
        End Sub
    End Class
End Namespace
