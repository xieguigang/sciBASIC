#Region "Microsoft.VisualBasic::357402be89a605cfa2049b3d48f37a53, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\ShellScriptAPI.vb"

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

    ' Module ShellScriptAPI
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports AprioriRule = Microsoft.VisualBasic.DataMining.AprioriRules.Entities.Rule

<Package("Tools.DataMining")>
Public Module ShellScriptAPI

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Write.Csv.Rule")> Public Function SaveAprioRule(data As IEnumerable(Of AprioriRule), saveTo$) As Boolean
        Return data.SaveTo(saveTo, False)
    End Function
End Module
