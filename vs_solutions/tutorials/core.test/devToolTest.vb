#Region "Microsoft.VisualBasic::8f47ed2a3a056cae65bbc3b18f55c97e, ..\sciBASIC#\vs_solutions\tutorials\core.test\devToolTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Module devToolTest

    Sub Main()
        Call loadTest()
    End Sub

    Sub loadTest()
        Dim assembly = New ProjectSpace(True)

        assembly.ImportFromXmlDocFile("E:\GCModeller\GCModeller\bin\Microsoft.VisualBasic.Framework_v47_dotnet_8da45dcd8060cc9a.xml")

        Pause()
    End Sub
End Module

