#Region "Microsoft.VisualBasic::6f93eeec3aac712957ec93b2f9ffe6df, Data\BinaryData\BinaryData\XDR\Attributes\VarAttribute.vb"

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

    '     Class VarAttribute
    ' 
    '         Properties: MaxLength
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class VarAttribute
        Inherits Attribute

        Private _MaxLength As UInteger

        Public Property MaxLength As UInteger
            Get
                Return _MaxLength
            End Get
            Private Set(value As UInteger)
                _MaxLength = value
            End Set
        End Property

        Public Sub New()
            MaxLength = UInteger.MaxValue
        End Sub

        Public Sub New(maxLength As UInteger)
            If maxLength = 0 Then Throw New ArgumentException("length must be greater than zero")
            Me.MaxLength = maxLength
        End Sub
    End Class
End Namespace

