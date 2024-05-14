#Region "Microsoft.VisualBasic::f12559c3de0b1405a154ba528d74b244, www\Microsoft.VisualBasic.Webservices.Bing\test\DEBUG_MAIN.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 336 B


    ' Module DEBUG_MAIN
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Webservices
Imports Microsoft.VisualBasic.Webservices.Bing

Module DEBUG_MAIN

    Sub Main()

        Dim trans = Translation.GetTranslation("aminoglycoside antibiotic")


        Pause()

        Dim n = Bing.SearchEngineProvider.Search("D-fructuronate: C6H9O7 kegg")
    End Sub
End Module
