#Region "Microsoft.VisualBasic::ed5c63b298fc2b051b5de076932172ae, Microsoft.VisualBasic.Core\src\Language\Language\UnixBash\Shell\rm.vb"

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

    '   Total Lines: 24
    '    Code Lines: 15
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 585 B


    '     Class FileDelete
    ' 
    '         Operators: -, <=, >=
    ' 
    '     Structure rmOption
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language.UnixBash

    ''' <summary>
    ''' ``rm -rf /*``
    ''' </summary>
    Public Class FileDelete

        Public Shared Operator -(rm As FileDelete, opt As rmOption) As FileDelete
            Return rm
        End Operator

        Public Shared Operator <=(rm As FileDelete, file$) As Long
            Return 0
        End Operator

        Public Shared Operator >=(rm As FileDelete, file$) As Long
            Throw New NotSupportedException
        End Operator
    End Class

    Public Structure rmOption

    End Structure
End Namespace
