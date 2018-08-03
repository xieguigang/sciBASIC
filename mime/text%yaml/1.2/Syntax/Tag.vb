#Region "Microsoft.VisualBasic::593db87795bffd7fe72a1c6b208d9e4b, mime\text%yaml\Syntax\Tag.vb"

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

'     Class Tag
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class Tag
    End Class

    Public Class ShorthandTag
        Inherits Tag

        Public Chars As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Chars.CharString}"
        End Function
    End Class

    Public Class VerbatimTag
        Inherits Tag

        Public Chars As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Chars.CharString}"
        End Function
    End Class

    Public Class NonSpecificTag
        Inherits Tag

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}>"
        End Function
    End Class
End Namespace
