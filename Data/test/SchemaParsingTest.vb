#Region "Microsoft.VisualBasic::fc59474c0914fa67b1ac51e5a72f4219, sciBASIC#\Data\test\SchemaParsingTest.vb"

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

    '   Total Lines: 23
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 959.00 B


    ' Class SchemaParsingTest
    ' 
    '     Properties: a, b, c, d, e
    '                 f, ffssdfsd, g, h, i
    '                 KV, LINQAlias
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports TestProject.RTools

Public Class SchemaParsingTest

    Public Property a As CommandLine.CommandLine = "1 2 3 4 5"
    Public Property b As String = Guid.NewGuid.ToString
    Public Property c As Double = Rnd()
    Public Property d As Integer = Rnd()
    Public Property e As Long = Now.ToBinary
    Public Property f As Date
    Public Property g As String() = {"35345", "645646", Rnd()}
    Public Property h As Boolean
    <Column("abcc")> Public Property i As ArchiveAction
    Public Property KV As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(Rnd, "1234")

    <Linq.Mapping.Column(Name:="alias.linq")> Public ReadOnly Property LINQAlias As Date = Now

    Public Property ffssdfsd As NamedValue(Of DESeq)

End Class
