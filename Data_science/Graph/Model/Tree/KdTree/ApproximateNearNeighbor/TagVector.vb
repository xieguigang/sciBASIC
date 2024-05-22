#Region "Microsoft.VisualBasic::94ccfe8e48b24247ba5cd83568544009, Data_science\Graph\Model\Tree\KdTree\ApproximateNearNeighbor\TagVector.vb"

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

    '   Total Lines: 47
    '    Code Lines: 24 (51.06%)
    ' Comment Lines: 15 (31.91%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.38 KB


    '     Class TagVector
    ' 
    '         Properties: index, size, tag, vector
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math

Namespace KdTree.ApproximateNearNeighbor

    ''' <summary>
    ''' a matrix row data is a vector
    ''' </summary>
    Public Class TagVector : Implements INamedValue, IVector

        ''' <summary>
        ''' the row index inside the original matrix rows
        ''' </summary>
        ''' <returns></returns>
        Public Property index As Integer
        ''' <summary>
        ''' the vector row data
        ''' </summary>
        ''' <returns></returns>
        Public Property vector As Double() Implements IVector.Data
        ''' <summary>
        ''' maybe is the unique reference id tag
        ''' </summary>
        ''' <returns></returns>
        Public Property tag As String Implements INamedValue.Key

        Public ReadOnly Property size As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(index As Integer, tag As String, vector As Double())
            _index = index
            _tag = tag
            _vector = vector
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] {vector.Take(6).JoinBy(", ")}..."
        End Function

    End Class
End Namespace
