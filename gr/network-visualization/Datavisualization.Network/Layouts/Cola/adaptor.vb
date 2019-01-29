
Class LayoutAdaptor
	Inherits Layout

	' dummy functions in case not defined by client
	Private Overloads Sub trigger(e As [Event])
	End Sub
	Private Overloads Sub kick()
	End Sub
	Private Overloads Sub drag()
	End Sub

	Private Overloads Function [on](eventType As EventType, listener As Action) As LayoutAdaptor
		Return Me
	End Function

	Private Property dragstart() As Action(Of Object)
		Get
			Return m_dragstart
		End Get
		Set
			m_dragstart = Value
		End Set
	End Property
	Private m_dragstart As Action(Of Object)
	Private Property dragStart() As Action(Of Object)
		Get
			Return m_dragStart
		End Get
		Set
			m_dragStart = Value
		End Set
	End Property
	Private m_dragStart As Action(Of Object)
	Private Property dragend() As Action(Of Object)
		Get
			Return m_dragend
		End Get
		Set
			m_dragend = Value
		End Set
	End Property
	Private m_dragend As Action(Of Object)
	Private Property dragEnd() As Action(Of Object)
		Get
			Return m_dragEnd
		End Get
		Set
			m_dragEnd = Value
		End Set
	End Property
	Private m_dragEnd As Action(Of Object)

	Public Sub New(options As options)
		MyBase.New()

		' take in implementation as defined by client

		Dim self = Me
		Dim o = options

		If o.trigger IsNot Nothing Then
			AddressOf Me.trigger = o.trigger
		End If

		If o.kick IsNot Nothing Then
			AddressOf Me.kick = o.kick
		End If

		If o.drag IsNot Nothing Then
			AddressOf Me.drag = o.drag
		End If

		If o.[on] IsNot Nothing Then
			AddressOf Me.[on] = o.[on]
		End If

		Me.dragstart = InlineAssignHelper(Me.dragStart, Layout.dragStart)
		Me.dragend = InlineAssignHelper(Me.dragEnd, Layout.dragEnd)
	End Sub
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
End Class


Interface options

	Property trigger() As Action(Of Object)
	Property kick() As Action(Of Object)
	Property drag() As Action(Of Object)
	Property [on]() As Action(Of Object)
End Interface
