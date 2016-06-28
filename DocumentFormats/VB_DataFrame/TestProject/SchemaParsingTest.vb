#Region "Microsoft.VisualBasic::a4acf635edd34298c1bece8c9b359cc3, ..\VB_DataFrame\TestProject\SchemaParsingTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Public Class SchemaParsingTest

    Public Property a As CommandLine.CommandLine = "1 2 3 4 5"
    Public Property b As String = Guid.NewGuid.ToString
    Public Property c As Double = RandomDouble()
    Public Property d As Integer = RandomDouble()
    Public Property e As Long = Now.ToBinary
    Public Property f As Date
    Public Property g As String() = {"35345", "645646", RandomDouble()}
    Public Property h As Boolean
    <Column("abcc")> Public Property i As GZip.ArchiveAction
    Public Property KV As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(RandomDouble, "1234")

    <Linq.Mapping.Column(Name:="alias.linq")> Public ReadOnly Property LINQAlias As Date = Now

End Class

