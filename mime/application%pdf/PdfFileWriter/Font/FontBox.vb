#Region "Microsoft.VisualBasic::eca56d72251321c72369e13e862afbda, mime\application%pdf\PdfFileWriter\Font\FontBox.vb"

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

    '   Total Lines: 37
    '    Code Lines: 13
    ' Comment Lines: 18
    '   Blank Lines: 6
    '     File Size: 781 B


    ' Class FontBox
    ' 
    '     Properties: Bottom, Left, Right, Top
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Font box class
''' </summary>
''' <remarks>
''' FontBox class is part of OUTLINETEXTMETRIC structure
''' </remarks>

Public Class FontBox

    ''' <summary>
    ''' Gets left side.
    ''' </summary>
    Public Property Left As Integer

    ''' <summary>
    ''' Gets top side.
    ''' </summary>
    Public Property Top As Integer

    ''' <summary>
    ''' Gets right side.
    ''' </summary>
    Public Property Right As Integer

    ''' <summary>
    ''' Gets bottom side.
    ''' </summary>
    Public Property Bottom As Integer

    Friend Sub New(DC As FontApi)
        Left = DC.ReadInt32()
        Top = DC.ReadInt32()
        Right = DC.ReadInt32()
        Bottom = DC.ReadInt32()
        Return
    End Sub
End Class
