#Region "Microsoft.VisualBasic::d6d0428b014ad0a70e63a7da167e9a10, mime\application%pdf\PdfReader\Parser\ParseResolveEventArgs.vb"

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

    '   Total Lines: 11
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 269 B


    '     Class ParseResolveEventArgs
    ' 
    '         Properties: [Object], Gen, Id
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace PdfReader
    Public Class ParseResolveEventArgs
        Inherits EventArgs

        Public Property Id As Integer
        Public Property Gen As Integer
        Public Property [Object] As ParseObjectBase
    End Class
End Namespace
