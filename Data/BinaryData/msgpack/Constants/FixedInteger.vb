#Region "Microsoft.VisualBasic::0d6988c56b93e787f86005d6969fc9ca, Data\BinaryData\msgpack\Constants\FixedInteger.vb"

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
    '    Code Lines: 8
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 296 B


    '     Class FixedInteger
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Constants

    Public NotInheritable Class FixedInteger
        Public Const POSITIVE_MIN As Byte = &H0
        Public Const POSITIVE_MAX As Byte = &H7F
        Public Const NEGATIVE_MIN As Byte = &HE0
        Public Const NEGATIVE_MAX As Byte = &HFF
    End Class
End Namespace
