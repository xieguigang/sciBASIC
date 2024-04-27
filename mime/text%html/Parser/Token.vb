﻿#Region "Microsoft.VisualBasic::67614dfe2043ccb6c5674882e6fe9518, G:/GCModeller/src/runtime/sciBASIC#/mime/text%html//Parser/Token.vb"

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

    '   Total Lines: 15
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 403 B


    '     Class Token
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    Public Class Token : Inherits CodeToken(Of HtmlTokens)

        Sub New(type As HtmlTokens, text As String)
            Call MyBase.New(type, text)
        End Sub

        Sub New(type As HtmlTokens, chars As Char())
            Call MyBase.New(type, New String(chars))
        End Sub
    End Class
End Namespace
