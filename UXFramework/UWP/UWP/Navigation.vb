
<AttributeUsage(AttributeTargets.Event, AllowMultiple:=False, Inherited:=True)>
Public Class Navigation : Inherits Attribute

    Public Overloads Shared ReadOnly Property TypeID As Type =
        GetType(Navigation)

    ''' <summary>
    ''' 事件属性使用的
    ''' </summary>
    Sub New()
    End Sub

    ReadOnly _handler As NavigationEvent
    ReadOnly _ctrl As Control
    ReadOnly _event As System.Reflection.EventInfo
    ReadOnly _eventTrigger As System.Reflection.MethodInfo
    ''' <summary>
    ''' 因为不能够预知如何生成参数，所以这里全部为空值
    ''' </summary>
    ReadOnly _args As Object()

    Public ReadOnly Property CanbeTrigger As Boolean
        Get
            Return Not _eventTrigger Is Nothing
        End Get
    End Property

    Sub New(ctrl As Control, [Event] As System.Reflection.EventInfo, handler As NavigationEvent)
        Dim __navigation As System.Reflection.MethodInfo =
            GetType(Navigation).GetMethod(NameOf(Navigation), System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
        Dim _delegate = [Delegate].CreateDelegate([Event].EventHandlerType, Me, __navigation)
        Call [Event].AddEventHandler(ctrl, _delegate)
        _handler = handler
        _ctrl = ctrl
        _event = [Event]
        _eventTrigger = _event.GetRaiseMethod()

        If Not _eventTrigger Is Nothing Then
            _args = _eventTrigger.GetParameters.Length.Sequence.ToArray(Function(idx) Nothing)
        Else
            _args = New Object() {}
        End If
    End Sub

    ''' <summary>
    ''' 将动作压栈
    ''' </summary>
    Private Sub Navigation()
        If _eventTrigger Is Nothing Then
            Return
        Else
            Call _handler.Navigations.Push(New MethodInvoker(AddressOf __invokeEvent))
        End If
    End Sub

    Private Sub __invokeEvent()
        Call _eventTrigger.Invoke(_ctrl, _args)
    End Sub

    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function
End Class

Public Class NavigationEvent

    Public ReadOnly Property Navigations As New Stack(Of MethodInvoker)
    Public ReadOnly Property [Handles] As FormWin10

    Sub New(Form As FormWin10)
        [Handles] = Form
        Call __addHandler([Handles])

        AddHandler [Handles].ControlAdded, AddressOf FormWin10ControlAdded
        AddHandler [Handles].Back.ButtonClick, AddressOf __navigationStack
    End Sub

    Private Sub __navigationStack()
        If Navigations.Count > 0 Then
            Dim [Event] = Navigations.Pop
            Call [Event]()
        End If

        If Navigations.Count = 0 Then
            Call [Handles].Back.SetEnabled(False)
        End If
    End Sub

    Private Sub FormWin10ControlAdded(sender As Object, e As ControlEventArgs)
        Dim ctrl As Control = e.Control
        Call __addHandler(ctrl)
    End Sub

    Private Function __addHandler(ctrl As Control) As Navigation()
        Dim Events = From evnt As System.Reflection.EventInfo
                     In ctrl.GetType.GetEvents
                     Let attrs As Object() = evnt.GetCustomAttributes(attributeType:=Navigation.TypeID, inherit:=True)
                     Where Not attrs.IsNullOrEmpty
                     Select evnt
        Dim Navigations = (From evnt As System.Reflection.EventInfo In Events Select New Navigation(ctrl, evnt, handler:=Me)).ToList

        If ctrl.Controls.Count > 0 Then
            Call Navigations.AddRange((From child As Control In ctrl.Controls Select __addHandler(child)).ToArray.MatrixToList)
        End If

        Return Navigations.ToArray
    End Function
End Class