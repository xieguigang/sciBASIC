#Region "Microsoft.VisualBasic::6d7e61d58408ced9cd1a94f5515eeb60, Data\BinaryData\BinaryData\XDR\Attributes\OrderAttribute.vb"

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

    '     Class OrderAttribute
    ' 
    '         Properties: Order
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class OrderAttribute
        Inherits Attribute

        Private _Order As UInteger

        Public Property Order As UInteger
            Get
                Return _Order
            End Get
            Private Set(value As UInteger)
                _Order = value
            End Set
        End Property

        Public Sub New(order As UInteger)
            Me.Order = order
        End Sub
    End Class
End Namespace

