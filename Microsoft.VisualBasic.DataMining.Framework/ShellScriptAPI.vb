#Region "Microsoft.VisualBasic::f58c8c5ce23ef0b2149f71a995b5fdcd, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.DataMining.Framework\ShellScriptAPI.vb"

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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

<[PackageNamespace]("Tools.DataMining")>
Public Module ShellScriptAPI

    <ExportAPI("Write.Csv.Rule")> Public Function SaveAprioRule(data As IEnumerable(Of AprioriAlgorithm.Entities.Rule), saveTo As String) As Boolean
        Return data.SaveTo(saveTo, False)
    End Function
End Module

