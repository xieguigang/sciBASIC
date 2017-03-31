#Region "Microsoft.VisualBasic::ed5c63b298fc2b051b5de076932172ae, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\UnixBash\Shell\rm.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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
