﻿#Region "Microsoft.VisualBasic::93ee43aa3455dae54daa53c9142543a5, G:/GCModeller/src/runtime/sciBASIC#/mime/application%rdf+xml//test/mathMLloaderTest.vb"

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

    '   Total Lines: 10
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 243 B


    ' Module mathMLloaderTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.rdf_xml

Module mathMLloaderTest

    Sub Main()
        Dim simple = "E:\GCModeller\src\runtime\sciBASIC#\mime\etc\kinetics1.xml".LoadXml(Of Math)

        Pause()
    End Sub
End Module
