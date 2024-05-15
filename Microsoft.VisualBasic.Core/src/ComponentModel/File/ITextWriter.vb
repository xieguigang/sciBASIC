#Region "Microsoft.VisualBasic::c0b636574e2ea60742b329b13e62b244, Microsoft.VisualBasic.Core\src\ComponentModel\File\ITextWriter.vb"

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

    '   Total Lines: 14
    '    Code Lines: 7
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 310 B


    '     Class ITextWriter
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace ComponentModel

    ''' <summary>
    ''' <see cref="TextWriter"/>
    ''' </summary>
    Public MustInherit Class ITextWriter

        Public MustOverride Sub Write(text As String)
        Public MustOverride Sub WriteLine(text As String)

    End Class
End Namespace
