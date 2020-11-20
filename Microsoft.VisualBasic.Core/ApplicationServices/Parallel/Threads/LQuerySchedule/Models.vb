#Region "Microsoft.VisualBasic::3be5c17214536ce52cfb48a329ec834f, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\LQuerySchedule\Models.vb"

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

    '     Structure TimeoutModel
    ' 
    '         Function: Invoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Parallel.Linq

    Public Structure TimeoutModel(Of out)

        Dim timeout As Double
        Dim task As Func(Of out())

        Public Function Invoke() As out()
            Dim result As out() = Nothing

            If OperationTimeOut(task, result, timeout) Then
                Return Nothing
            Else
                Return result
            End If
        End Function
    End Structure
End Namespace
