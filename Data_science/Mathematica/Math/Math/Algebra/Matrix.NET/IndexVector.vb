#Region "Microsoft.VisualBasic::edaae4c09dab68da43345f436996cc2c, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\IndexVector.vb"

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

    '   Total Lines: 22
    '    Code Lines: 14 (63.64%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 509 B


    '     Class IndexVector
    ' 
    '         Properties: Col, Row, X
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' another form of <see cref="SparseMatrix"/>
    ''' </summary>
    Public Class IndexVector

        Public Property Row As Integer()
        Public Property Col As Integer()
        Public Property X As Double()

        Sub New()
        End Sub

        Sub New(row As Integer(), col As Integer(), x As Double())
            Me.Row = row
            Me.Col = col
            Me.X = x
        End Sub

    End Class
End Namespace
