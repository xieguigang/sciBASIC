#Region "Microsoft.VisualBasic::53bae69fb26cc32521402895edb9b6c3, Microsoft.VisualBasic.Core\Text\Parser\CharEnumerator.vb"

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

    '     Class CharEnumerator
    ' 
    '         Properties: Remaining
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal

Namespace Text.Parser

    Public Class CharEnumerator : Inherits Pointer(Of Char)

        ''' <summary>
        ''' 查看还有多少字符没有被处理完
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Remaining As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Skip(index).CharString
            End Get
        End Property

        Sub New(data As String)
            Call MyBase.New(data)
        End Sub
    End Class
End Namespace
