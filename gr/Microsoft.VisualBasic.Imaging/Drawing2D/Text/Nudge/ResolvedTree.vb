#Region "Microsoft.VisualBasic::76609941a04065314dd99ae3366dbe9c, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\ResolvedTree.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 484 B


    '     Class ResolvedTree
    ' 
    '         Properties: childrens, parent
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Text.Nudge

    Public Class ResolvedTree

        Public Property parent As CloudOfTextRectangle
        Public Property childrens As ResolvedTree()

        Public Overrides Function ToString() As String
            If childrens.IsNullOrEmpty Then
                Return parent.ToString
            Else
                Return $"{parent.ToString}: {{{childrens.JoinBy("; ")}}}"
            End If
        End Function

    End Class
End Namespace
