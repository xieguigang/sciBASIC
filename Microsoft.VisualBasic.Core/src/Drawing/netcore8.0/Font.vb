#Region "Microsoft.VisualBasic::f6a7fb47d94f6cd5e8873e3ad21ce7e3, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Font.vb"

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

    '   Total Lines: 40
    '    Code Lines: 32 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (20.00%)
    '     File Size: 1.04 KB


    '     Class Font
    ' 
    '         Properties: Height, Name, Size, SizeInPoints, Style
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Clone
    ' 
    '     Enum FontStyle
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public Class Font

        Public ReadOnly Property Name As String
        Public ReadOnly Property Size As Single
        Public ReadOnly Property SizeInPoints As Single
        Public ReadOnly Property Style As FontStyle

        Public ReadOnly Property Height As Single

        Sub New(familyName As String, emSize As Single, Optional style As FontStyle = FontStyle.Regular)
            Me.Name = familyName
            Me.Size = emSize
            Me.Style = style
        End Sub

        Sub New(baseFont As Font, style As FontStyle)
            _Name = baseFont.Name
            _Size = baseFont.Size
            _Style = style
        End Sub

        Public Function Clone() As Object
            Return New Font(Name, Size, Style)
        End Function

    End Class

    <Flags>
    Public Enum FontStyle
        Regular = 0
        Bold = 1
        Italic = 2
        Underline = 4
        Strikeout = 8
    End Enum
#End If
End Namespace
