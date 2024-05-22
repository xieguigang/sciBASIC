#Region "Microsoft.VisualBasic::2ef534f6c0add3b4bcbd5d3322ac7c59, mime\application%pdf\PdfReader\Document\PdfObjectReference.vb"

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

    '   Total Lines: 31
    '    Code Lines: 26 (83.87%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (16.13%)
    '     File Size: 898 B


    '     Class PdfObjectReference
    ' 
    '         Properties: Gen, Id, ParseObjectReference
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class PdfObjectReference
        Inherits PdfObject

        Public Sub New(parent As PdfObject, reference As ParseObjectReference)
            MyBase.New(parent, reference)
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseObjectReference As ParseObjectReference
            Get
                Return TryCast(ParseObject, ParseObjectReference)
            End Get
        End Property

        Public ReadOnly Property Id As Integer
            Get
                Return ParseObjectReference.Id
            End Get
        End Property

        Public ReadOnly Property Gen As Integer
            Get
                Return ParseObjectReference.Gen
            End Get
        End Property
    End Class
End Namespace
