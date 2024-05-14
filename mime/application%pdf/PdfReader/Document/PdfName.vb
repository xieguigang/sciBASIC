#Region "Microsoft.VisualBasic::bb621c48e731f4fcbec3b298e3429469, mime\application%pdf\PdfReader\Document\PdfName.vb"

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

    '   Total Lines: 35
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 922 B


    '     Class PdfName
    ' 
    '         Properties: ParseName, StrVal, Value
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

    Public Class PdfName : Inherits PdfObject

        Public ReadOnly Property StrVal As String
            Get
                Return Value
            End Get
        End Property

        Public Sub New(parent As PdfObject, name As ParseName)
            MyBase.New(parent, name)
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseName As ParseName
            Get
                Return TryCast(ParseObject, ParseName)
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return ParseName.Value
            End Get
        End Property
    End Class
End Namespace
