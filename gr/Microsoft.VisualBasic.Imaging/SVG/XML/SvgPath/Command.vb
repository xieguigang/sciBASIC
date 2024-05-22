#Region "Microsoft.VisualBasic::b248d457d3d9e5a772c2eceb0181f036, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Command.vb"

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

    '   Total Lines: 32
    '    Code Lines: 24 (75.00%)
    ' Comment Lines: 3 (9.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.12 KB


    '     Class Command
    ' 
    '         Properties: isRelative
    ' 
    '         Function: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.PathHelper

    ''' <summary>
    ''' the base command for build a svg path
    ''' </summary>
    Public MustInherit Class Command

        Public Property isRelative As Boolean

        Public Shared Function Parse(text As String) As List(Of String)
            Dim s As String = text.Trim()
            Dim tokens = New List(Of String)()
            Dim startIdx = 0
            For i = 0 To s.Length - 1
                If s(i) = "-"c Then i += 1
                While i < s.Length AndAlso s(i) <> "-"c AndAlso s(i) <> " "c AndAlso s(i) <> ","c
                    i += 1
                End While
                Dim stopIdx = i
                If stopIdx > startIdx Then
                    tokens.Add(s.Substring(startIdx, stopIdx - startIdx))
                End If

                startIdx = If(i < s.Length AndAlso s(i) = "-"c, i, i + 1)
            Next
            Return tokens
        End Function

        Public MustOverride Sub Scale(factor As Double)
        Public MustOverride Sub Translate(deltaX As Double, deltaY As Double)
    End Class
End Namespace
