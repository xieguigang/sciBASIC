#Region "Microsoft.VisualBasic::58acaf66ffae4c6d55490f29a08dba6c, vs_solutions\JavaScript\Runtime\JsPromise.vb"

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

    '   Total Lines: 150
    '    Code Lines: 113 (75.33%)
    ' Comment Lines: 15 (10.00%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 22 (14.67%)
    '     File Size: 5.74 KB


    '     Class JsPromise
    ' 
    ' 
    '         Enum PromiseState
    ' 
    '             Fulfilled, Pending, Rejected
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: IsPending, Result, State
    ' 
    '     Function: [Catch], [Finally], [Then], ToString
    ' 
    '     Sub: Flush, Fulfill, Reject, ResolveWith
    ' 
    '     Class PromiseReaction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Run, Schedule
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' JavaScript Promise.
    ''' <para>
    ''' The engine has no event loop, so settlement is driven by a synchronous
    ''' microtask queue (<see cref="JsRuntime.DrainMicrotasks"/>): reactions are
    ''' queued when their promise settles and run when the queue is drained —
    ''' after an <c>await</c> and at the end of a script.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsPromise

        Public Enum PromiseState
            Pending
            Fulfilled
            Rejected
        End Enum

        Public Property State As PromiseState = PromiseState.Pending
        Public Property Result As Object

        Private ReadOnly _reactions As New List(Of PromiseReaction)

        Public ReadOnly Property IsPending As Boolean
            Get
                Return State = PromiseState.Pending
            End Get
        End Property

        Public Sub Fulfill(value As Object)
            If State <> PromiseState.Pending Then Return
            State = PromiseState.Fulfilled
            Result = value
            Flush()
        End Sub

        Public Sub Reject(reason As Object)
            If State <> PromiseState.Pending Then Return
            State = PromiseState.Rejected
            Result = reason
            Flush()
        End Sub

        ''' <summary>Settle from a settled promise (thenable unwrapping is not modelled).</summary>
        Public Sub ResolveWith(value As Object)
            Dim inner = TryCast(value, JsPromise)
            If inner Is Nothing Then
                Fulfill(value)
            ElseIf inner.State = PromiseState.Fulfilled Then
                Fulfill(inner.Result)
            ElseIf inner.State = PromiseState.Rejected Then
                Reject(inner.Result)
            Else
                ' adopt the inner promise's settlement
                QueueMicrotask(Sub()
                                   If inner.State = PromiseState.Fulfilled Then Fulfill(inner.Result)
                                   If inner.State = PromiseState.Rejected Then Reject(inner.Result)
                               End Sub)
            End If
        End Sub

        ''' <summary>Register reactions; returns the chained promise.</summary>
        Public Function [Then](onFulfilled As Object, onRejected As Object) As JsPromise
            Dim child As New JsPromise()
            Dim reaction As New PromiseReaction(Me, onFulfilled, onRejected, child)
            If State = PromiseState.Pending Then
                _reactions.Add(reaction)
            Else
                reaction.Schedule()
            End If
            Return child
        End Function

        Public Function [Catch](onRejected As Object) As JsPromise
            Return [Then](Nothing, onRejected)
        End Function

        Public Function [Finally](handler As Object) As JsPromise
            Dim onOk As Func(Of Object(), Object) =
                Function(args As Object())
                    If JsRuntime.IsJsFunction(handler) Then JsRuntime.JsInvoke(handler, New Object() {})
                    Return If(args.Length > 0, args(0), JsRuntime.Undef)
                End Function
            Dim onErr As Func(Of Object(), Object) =
                Function(args As Object())
                    If JsRuntime.IsJsFunction(handler) Then JsRuntime.JsInvoke(handler, New Object() {})
                    Throw New JsRuntimeException(If(args.Length > 0, args(0), JsRuntime.Undef))
                End Function
            Return [Then](CObj(onOk), CObj(onErr))
        End Function

        Private Sub Flush()
            For Each r In _reactions.ToArray()
                r.Schedule()
            Next
            _reactions.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return "[object Promise]"
        End Function

    End Class

    ''' <summary>One (promise, handler, child) reaction triple of the promise machinery.</summary>
    Public NotInheritable Class PromiseReaction

        Private ReadOnly _source As JsPromise
        Private ReadOnly _onFulfilled As Object
        Private ReadOnly _onRejected As Object
        Private ReadOnly _child As JsPromise

        Public Sub New(source As JsPromise, onFulfilled As Object, onRejected As Object, child As JsPromise)
            _source = source
            _onFulfilled = onFulfilled
            _onRejected = onRejected
            _child = child
        End Sub

        ''' <summary>Queue the reaction as a microtask (JS: reactions never run synchronously).</summary>
        Public Sub Schedule()
            JsRuntime.QueueMicrotask(AddressOf Run)
        End Sub

        Private Sub Run()
            Dim handler = If(_source.State = JsPromise.PromiseState.Fulfilled, _onFulfilled, _onRejected)
            If Not JsRuntime.IsJsFunction(handler) Then
                ' no handler for this outcome → propagate to the chained promise
                If _source.State = JsPromise.PromiseState.Fulfilled Then
                    _child.ResolveWith(_source.Result)
                Else
                    _child.Reject(_source.Result)
                End If
                Return
            End If

            Try
                Dim value = JsRuntime.JsInvoke(handler, New Object() {_source.Result})
                _child.ResolveWith(value)
            Catch ex As JsRuntimeException
                _child.Reject(ex.Payload)
            Catch ex As Exception
                _child.Reject(ex.Message)
            End Try
        End Sub

    End Class

End Namespace

