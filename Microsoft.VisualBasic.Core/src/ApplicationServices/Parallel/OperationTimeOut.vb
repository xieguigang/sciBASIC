#Region "Microsoft.VisualBasic::82d8a48a4f051f64a98db1f0c21d8878, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\OperationTimeOut.vb"

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

    '   Total Lines: 74
    '    Code Lines: 37
    ' Comment Lines: 27
    '   Blank Lines: 10
    '     File Size: 2.65 KB


    '     Module TimeOutAPI
    ' 
    '         Function: (+3 Overloads) OperationTimeOut
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Parallel

    Public Module TimeOutAPI

        ''' <summary>
        ''' The returns value of TRUE represent of the target operation has been time out.(返回真，表示操作超时)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="handle"></param>
        ''' <param name="out"></param>
        ''' <param name="timeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function OperationTimeOut(Of T, TOut)(handle As Func(Of T, TOut), [in] As T, ByRef out As TOut, timeOut As Double) As Boolean
            Dim invoke As New BackgroundTask(Of TOut)(Function() handle([in]))
            Dim i As Integer

            timeOut = timeOut * 1000
            invoke.Start()

            Do While i < timeOut
                If invoke.TaskComplete Then
                    out = invoke.Value
                    Return False
                End If

                i += 1
                Call Thread.Sleep(1)
            Loop

            Call invoke.Abort()

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="handle"></param>
        ''' <param name="Out"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function OperationTimeOut(Of T)(handle As Func(Of T), ByRef Out As T, TimeOut As Double) As Boolean
            Return OperationTimeOut(Of Boolean, T)(Function(b) handle(), True, Out, TimeOut)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function OperationTimeOut(handle As Action, TimeOut As Double) As Boolean
            Dim invoke As Func(Of Boolean, Boolean) =
                Function(b) As Boolean
                    Call handle()
                    Return True
                End Function
            Return OperationTimeOut(invoke, True, True, TimeOut)
        End Function
    End Module
End Namespace
