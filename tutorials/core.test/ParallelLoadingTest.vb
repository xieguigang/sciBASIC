#Region "Microsoft.VisualBasic::b493f08649bb3120eb1d418929d10925, ..\sciBASIC#\tutorials\core.test\ParallelLoadingTest.vb"

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

<Serializable>
Public Class ParallelLoadingTest

    Public Property ddddd As String
    Public Property fffd As Date

    <Microsoft.VisualBasic.Parallel.ParallelLoading.LoadEntry>
    Public Shared Function Load(path As String) As ParallelLoadingTest()
        Call Threading.Thread.Sleep(10 * 1000)
        Return {New ParallelLoadingTest With {.ddddd = Rnd(), .fffd = Now}}
    End Function
End Class
