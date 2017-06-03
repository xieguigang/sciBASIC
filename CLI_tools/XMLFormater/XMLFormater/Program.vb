#Region "Microsoft.VisualBasic::84b071287dd4f322299c14017a84146e, ..\sciBASIC#\CLI_tools\XMLFormater\XMLFormater\Program.vb"

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

Imports System.Xml
Imports Microsoft.VisualBasic.CommandLine

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine, executeFile:=AddressOf FormatXmlFile)
    End Function

    Private Function FormatXmlFile(file$, args As CommandLine) As Integer
        Dim xmlDoc As New XmlDocument
        Dim out$ = args.GetValue("/out", file.TrimSuffix & ".FormatEdited.XML")
        Call xmlDoc.Load(file)
        Call xmlDoc.Save(out)
        Return 0
    End Function
End Module

