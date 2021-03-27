#Region "Microsoft.VisualBasic::3d125c37a2d81976cafb2e6bb4cd022a, Data\BinaryData\BinaryData\XDR\Attributes\FixAttribute.vb"

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

    '     Class FixAttribute
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class FixAttribute
        Inherits Attribute

        Private _Length As UInteger

        Public Property Length As UInteger
            Get
                Return _Length
            End Get
            Private Set(value As UInteger)
                _Length = value
            End Set
        End Property

        Public Sub New(length As UInteger)
            If length = 0 Then Throw New ArgumentException("length must be greater than zero")
            Me.Length = length
        End Sub
    End Class
End Namespace

