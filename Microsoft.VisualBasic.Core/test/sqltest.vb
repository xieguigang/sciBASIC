#Region "Microsoft.VisualBasic::1f4861b2277154ede918a88180ab0989, Microsoft.VisualBasic.Core\test\sqltest.vb"

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

    ' Module sqltest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module sqltest

    Sub Main()
        Dim names = "C:\Users\Administrator\Desktop\list.txt".ReadAllLines.Select(AddressOf Strings.LCase).ToArray
        Dim queryMeta = $"SELECT * FROM biodeepDB.metadb where lower(`name`) in ({names.Select(Function(a) $"""{a}""").JoinBy(", ")});"


        Call queryMeta.SaveTo("C:\Users\Administrator\Desktop\list_meta.sql")
    End Sub
End Module

