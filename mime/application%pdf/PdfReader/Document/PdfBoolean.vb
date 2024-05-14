#Region "Microsoft.VisualBasic::e8994f39b0f00eb9a200a3f9524fb01e, mime\application%pdf\PdfReader\Document\PdfBoolean.vb"

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

    '   Total Lines: 33
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 941 B


    '     Class PdfBoolean
    ' 
    '         Properties: ParseBoolean, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class PdfBoolean
        Inherits PdfObject

        Public Sub New(parent As PdfObject, [boolean] As ParseBoolean)
            MyBase.New(parent, [boolean])
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseBoolean As ParseBoolean
            Get
                Return TryCast(ParseObject, ParseBoolean)
            End Get
        End Property

        Public ReadOnly Property Value As Boolean
            Get
                Return ParseBoolean.Value
            End Get
        End Property

        Public Shared Narrowing Operator CType(b As PdfBoolean) As Boolean
            Return b.Value
        End Operator
    End Class
End Namespace
