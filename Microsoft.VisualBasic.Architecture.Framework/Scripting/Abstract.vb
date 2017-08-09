#Region "Microsoft.VisualBasic::fef72ddaec105f464cd59a449ca92e7d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Scripting\Abstract.vb"

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

Namespace Scripting

    Public Module Abstract

        ''' <summary>
        ''' Gets the variable value from runtime.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <returns></returns>
        Public Delegate Function GetValue(var$) As Object
        ''' <summary>
        ''' How to make a function calls.(这个是在已经知道了确切的函数对象的前体下才会使用这个进行调用)
        ''' </summary>
        ''' <param name="func$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function FunctionEvaluate(func$, args As Object()) As Object

    End Module
End Namespace
