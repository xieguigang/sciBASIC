#Region "Microsoft.VisualBasic::78b2f42de1bb467fca2cc03efdef17fa, mime\application%xml\test\Module1.vb"

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

    '   Total Lines: 25
    '    Code Lines: 16 (64.00%)
    ' Comment Lines: 1 (4.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (32.00%)
    '     File Size: 694 B


    ' Module Module1
    ' 
    '     Sub: genericParsertest2, Main
    ' 
    ' /********************************************************************************/

#End Region

Module Module1

    ''' <summary>
    ''' Entry point of the XSD toolkit self test. The exit code is the number of
    ''' failed DebugAudit assertions (0 means success).
    ''' </summary>
    Public Function Main(args As String()) As Integer
        Return XsdSelfTest.Run()
    End Function
End Module
