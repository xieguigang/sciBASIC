#Region "2d05e3ac476198fd85854bf6d7828ba8, ..\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Trigger.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Triggers

    Public MustInherit Class ITrigger : Inherits ICallbackInvoke

        Sub New(invoke As Action)
            Call MyBase.New(invoke)
        End Sub

        ''' <summary>
        ''' Test if success then run callback
        ''' </summary>
        Public Sub TestRun()
            If __test() Then
                Call _execute()
            End If
        End Sub

        Protected MustOverride Function __test() As Boolean

    End Class

    Public Interface ITimer : Inherits IObjectModel_Driver, ICallbackTask
        Property Interval As Integer

        Sub [Stop]()
    End Interface

    Public Class ConditionTrigger : Inherits ITrigger

        Public ReadOnly Property Condition As Func(Of Boolean)

        Sub New(invoke As Action, condit As Func(Of Boolean))
            Call MyBase.New(invoke)
        End Sub

        Protected Overrides Function __test() As Boolean
            Return _Condition()
        End Function
    End Class

    Public Class TimerTrigger : Inherits ITrigger
        Implements IObjectModel_Driver
        Implements ITimer

        Public ReadOnly Property Time As Date

        ''' <summary>
        ''' ms
        ''' </summary>
        ''' <returns></returns>
        Public Property Interval As Integer Implements ITimer.Interval
            Get
                Return __timer.Periods
            End Get
            Set(value As Integer)
                __timer.Periods = value
            End Set
        End Property

        ReadOnly __timer As UpdateThread

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="time"></param>
        ''' <param name="task"></param>
        ''' <param name="interval">ms</param>
        Sub New(time As Date, task As Action, Optional interval As Integer = 100)
            Call MyBase.New(task)

            Me.Time = time
            Me.Interval = interval
            Me.__timer = New UpdateThread(interval, AddressOf TestRun)
        End Sub

        ''' <summary>
        ''' 不计算毫秒
        ''' </summary>
        ''' <returns></returns>
        Protected Overrides Function __test() As Boolean
            Dim d As Date = Now

            If d.Year <> Time.Year Then
                Return False
            ElseIf d.Month <> Time.Month Then
                Return False
            ElseIf d.Day <> Time.Day Then
                Return False
            ElseIf d.Hour <> Time.Hour Then
                Return False
            ElseIf d.Minute <> Time.Minute Then
                Return False
            ElseIf d.Second <> Time.Second Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Function Start() As Integer Implements IObjectModel_Driver.Run
            Return __timer.Start
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Sub [Stop]() Implements ITimer.Stop
            Call __timer.Stop()
        End Sub
    End Class
End Namespace
