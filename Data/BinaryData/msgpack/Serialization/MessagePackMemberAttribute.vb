#Region "Microsoft.VisualBasic::e851033be81a09ec1549d127cbec9c59, Data\BinaryData\msgpack\Serialization\MessagePackMemberAttribute.vb"

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

    '     Class MessagePackMemberAttribute
    ' 
    '         Properties: Id, NilImplication
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization

    ''' <summary>
    ''' Mimic the full CLI namespace and naming so that this library can be used
    ''' as a drop-in replacement and/or linked file with both frameworks as needed.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class MessagePackMemberAttribute
        Inherits Attribute

        Private ReadOnly idField As Integer

        Public Sub New(id As Integer)
            idField = id
            NilImplication = NilImplication.MemberDefault
        End Sub

        Public ReadOnly Property Id As Integer
            Get
                Return idField
            End Get
        End Property

        Public Property NilImplication As NilImplication
    End Class
End Namespace

