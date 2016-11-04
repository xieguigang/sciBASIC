#Region "Microsoft.VisualBasic::932016faa0e4811b52413256ceacef56, ..\visualbasic_App\Data\DataFrame\StorageProvider\ComponntModels\AttrBridges.vb"

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

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    Public Class Column : Inherits StorageProvider

        ''' <summary>
        ''' The column attribute definition.
        ''' </summary>
        ''' <returns></returns>
        Public Property Define As ColumnAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return Define.Name
            End Get
        End Property

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.Column
            End Get
        End Property

        Private Sub New(attr As ColumnAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty)
            Define = attr
            __toString = AddressOf Scripting.ToString
        End Sub

        ReadOnly __toString As Func(Of Object, String)

        ''' <summary>
        ''' With custom parser from the user code.
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <param name="bindProperty"></param>
        ''' <param name="parser"></param>
        Private Sub New(attr As ColumnAttribute, bindProperty As PropertyInfo, parser As IParser)
            Call MyBase.New(bindProperty, AddressOf parser.TryParse)
            Define = attr
            __toString = AddressOf parser.ToString
        End Sub

        Public Shared Function CreateObject(attr As ColumnAttribute, BindProperty As PropertyInfo) As Column
            Dim parser As IParser = attr.GetParser

            If parser Is Nothing Then
                Return New Column(attr, BindProperty)
            Else
                Return New Column(attr, BindProperty, parser)
            End If
        End Function

        Public Overrides Function ToString([object] As Object) As String
            Return __toString([object])
        End Function
    End Class
End Namespace
