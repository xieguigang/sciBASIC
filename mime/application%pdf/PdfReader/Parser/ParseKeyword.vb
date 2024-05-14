#Region "Microsoft.VisualBasic::ee0e8987c9aef7926b4587374b09b9fd, mime\application%pdf\PdfReader\Parser\ParseKeyword.vb"

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

    '   Total Lines: 28
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 638 B


    '     Enum ParseKeyword
    ' 
    '         [False], [True], EndObj, EndStream, Null
    '         Obj, R, StartXRef, Stream, Trailer
    '         XRef
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace PdfReader
    Public Enum ParseKeyword
        <Description("endobj")>
        EndObj
        <Description("endstream")>
        EndStream
        <Description("false")>
        [False]
        <Description("null")>
        Null
        <Description("obj")>
        Obj
        <Description("R")>
        R
        <Description("startxref")>
        StartXRef
        <Description("stream")>
        Stream
        <Description("trailer")>
        Trailer
        <Description("true")>
        [True]
        <Description("xref")>
        XRef
    End Enum
End Namespace
