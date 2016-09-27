#Region "Microsoft.VisualBasic::63381230b56031b209080bb30005ce02, ..\visualbasic_App\Data\DataFrame\StorageProvider\Reflection\Attributes\Column.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Data.Linq.Mapping

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This is a column in the csv document. 
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property,
                    AllowMultiple:=False,
                    Inherited:=False)>
    Public Class ColumnAttribute : Inherits DataAttribute
        Implements Reflection.IAttributeComponent

        Public Overridable ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.Column
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name"></param>
        Sub New(Name As String)
            Me.Name = Name
            If String.IsNullOrEmpty(Name) Then
                Throw New DataException($"{NameOf(Name)} value can not be null!")
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared ReadOnly Property TypeInfo As System.Type = GetType(ColumnAttribute)

        Public Shared Narrowing Operator CType(attr As ColumnAttribute) As String
            Return attr.Name
        End Operator

        Public Shared Widening Operator CType(sName As String) As ColumnAttribute
            Return New ColumnAttribute(sName)
        End Operator
    End Class
End Namespace
