﻿#Region "Microsoft.VisualBasic::73274cc38763d38edcb6bd8ef166d71d, mime\text%html\Render\TextString.vb"

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

    '   Total Lines: 66
    '    Code Lines: 49 (74.24%)
    ' Comment Lines: 6 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (16.67%)
    '     File Size: 1.86 KB


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

#If NET48 Then
Imports Font = System.Drawing.Font
Imports Pen = System.Drawing.Pen
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

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
