#Region "Microsoft.VisualBasic::a180b46a9f813533d200877a64df6b52, sciBASIC#\Microsoft.VisualBasic.Core\test\wgetTest.vb"

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

    '   Total Lines: 9
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 308.00 B


    ' Module wgetTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Http

Module wgetTest
    Sub Main()
        Dim wget As New wget("http://mona.fiehnlab.ucdavis.edu/rest/downloads/retrieve/9c822c48-67f4-4600-8b81-ef7491008245", "D:\Database\KEGG\organism\KEGG_Organism\ath\test.png")

        Call wget.Run()
    End Sub
End Module
