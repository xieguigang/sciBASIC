#Region "Microsoft.VisualBasic::76dd099d396264ff7e659d98c490e857, mime\text%html\Render\TextString.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 6 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.43 KB


    '     Class TextString
    ' 
    '         Properties: color, font, text, weight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetWeightedFont, ToString
    '         Enum WeightStyles
    ' 
    '             [sub], [sup]
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Render

    Public Class TextString

        Public Property font As Font
        Public Property text As String
        Public Property weight As WeightStyles
        Public Property color As String

        Sub New()
        End Sub

        Sub New(copy As TextString)
            font = copy.font.Clone
            text = copy.text
            weight = copy.weight
            color = copy.color
        End Sub

        Public Function GetWeightedFont() As Font
            Select Case weight
                Case WeightStyles.sub, WeightStyles.sup
                    Return New Font(font.Name, font.Size / 2)
                Case Else
                    Return font
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return text
        End Function

        Public Enum WeightStyles As Integer
            normal = 0
            ''' <summary>
            ''' 上标
            ''' </summary>
            [sup]
            ''' <summary>
            ''' 下标
            ''' </summary>
            [sub]
        End Enum

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(text As TextString) As String
            Return text.text
        End Operator
    End Class

End Namespace
