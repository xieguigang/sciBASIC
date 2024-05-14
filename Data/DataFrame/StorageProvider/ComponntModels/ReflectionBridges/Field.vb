#Region "Microsoft.VisualBasic::6d11cd9ce72980b2195df3b9d7ba765e, Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Field.vb"

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

    '   Total Lines: 63
    '    Code Lines: 40
    ' Comment Lines: 10
    '   Blank Lines: 13
    '     File Size: 2.09 KB


    '     Class Column
    ' 
    '         Properties: define, Name, ProviderId
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CreateObject, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace StorageProvider.ComponentModels

    Public Class Column : Inherits StorageProvider

        ''' <summary>
        ''' The column attribute definition.
        ''' </summary>
        ''' <returns></returns>
        Public Property define As ColumnAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return define.Name
            End Get
        End Property

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.Column
            End Get
        End Property

        Private Sub New(attr As ColumnAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty)

            _define = attr
            _toString = AddressOf Scripting.ToString
        End Sub

        ReadOnly _toString As Func(Of Object, String)

        ''' <summary>
        ''' With custom parser from the user code.
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <param name="bindProperty"></param>
        ''' <param name="parser"></param>
        Private Sub New(attr As ColumnAttribute, bindProperty As PropertyInfo, parser As IParser)
            Call MyBase.New(bindProperty, AddressOf parser.TryParse)

            _define = attr
            _toString = AddressOf parser.ToString
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
            Return _toString([object])
        End Function
    End Class
End Namespace
