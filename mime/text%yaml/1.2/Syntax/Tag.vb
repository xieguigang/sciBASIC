#Region "Microsoft.VisualBasic::b3f75c4df164dd554ab694d461760b5a, mime\text%yaml\1.2\Syntax\Tag.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (27.27%)
    '     File Size: 782 B


    '     Class Tag
    ' 
    ' 
    ' 
    '     Class ShorthandTag
    ' 
    '         Function: ToString
    ' 
    '     Class VerbatimTag
    ' 
    '         Function: ToString
    ' 
    '     Class NonSpecificTag
    ' 
    '         Function: ToString
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
