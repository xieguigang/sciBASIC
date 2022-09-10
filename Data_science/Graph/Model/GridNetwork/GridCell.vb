#Region "Microsoft.VisualBasic::f9464dde52d52346e034bba90c207fcf, sciBASIC#\Data_science\Graph\Model\GridNetwork\GridCell.vb"

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

    '   Total Lines: 34
    '    Code Lines: 22
    ' Comment Lines: 4
    '   Blank Lines: 8
    '     File Size: 704 B


    ' Class GridCell
    ' 
    '     Properties: data, index
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Public Class GridCell(Of T)

    ''' <summary>
    ''' 二维数组之中的索引 
    ''' </summary>
    ''' <returns></returns>
    Public Property index As Point

    Dim m_data As T

    Public Property data As T
        Get
            Return m_data
        End Get
        Protected Set(value As T)
            m_data = value
        End Set
    End Property

    Sub New()
    End Sub

    Sub New(x As Integer, y As Integer, data As T)
        index = New Point(x, y)
        m_data = data
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{index.X}, {index.Y}] {data.ToString}"
    End Function

End Class
