Imports System.Windows.Forms
Imports System.Drawing

Public Class SingleInstanceFormEntry(Of Form As System.Windows.Forms.Form)

    Dim _InternalEntry As Control
    Dim _InternalInstance As Form
    Dim _GetPosition As Func(Of Control, System.Windows.Forms.Form, Point)
    Dim _InternalParentForm As System.Windows.Forms.Form
    Dim _ShowModel As Boolean

    Public ReadOnly Property FormInstance As Form
        Get
            Return _InternalInstance
        End Get
    End Property

    Public Property Arguments As Object()

    Sub New(ControlEntry As Control,
           Optional ParentForm As System.Windows.Forms.Form = Nothing,
           Optional GetPosition As Func(Of Control, System.Windows.Forms.Form, Point) = Nothing,
           Optional ShowModelForm As Boolean = True)

        _InternalEntry = ControlEntry
        _GetPosition = GetPosition
        _InternalParentForm = ParentForm
        _ShowModel = ShowModelForm

        AddHandler _InternalEntry.Click, AddressOf InternalInvokeEntry

        If GetPosition Is Nothing AndAlso Not _InternalParentForm Is Nothing Then
            _GetPosition = AddressOf InternalGetDefaultPosition
        End If
    End Sub

    ''' <summary>
    ''' 不做任何位置的设置操作
    ''' </summary>
    ''' <remarks></remarks>
    Sub New(Optional ShowModelForm As Boolean = True)
        _ShowModel = ShowModelForm
    End Sub

    Public Sub [AddHandler](Handle As Action(Of Object, EventArgs))
        AddHandler _InternalEntry.Click, New EventHandler(Sub(obj, args) Call Handle(obj, args))
    End Sub

    ''' <summary>
    ''' 默认位置是控件的中间
    ''' </summary>
    ''' <param name="Control"></param>
    ''' <param name="Form"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function InternalGetDefaultPosition(Control As UserControl, Form As System.Windows.Forms.Form) As Point
        Dim Pt As Point = Form.PointToScreen(Control.Location)
        Pt = New Point(CInt(Pt.X + Control.Width / 2), CInt(Pt.Y + Control.Height / 2))
        Return Pt
    End Function

    Public Sub Invoke(ParamArray InvokeSets As KeyValuePair(Of String, Object)())
        _InvokeSets = InvokeSets
        Call InternalInvokeEntry(Nothing, Nothing)
    End Sub

    Dim _InvokeSets As KeyValuePair(Of String, Object)()

    Private Sub InternalInvokeEntry(sender As Object, EVtargs As EventArgs)
        If _InternalInstance Is Nothing Then

            _InternalInstance = DirectCast(Activator.CreateInstance(GetType(Form), Arguments), Form)
            If Not _GetPosition Is Nothing Then
                Dim pt As Point = _GetPosition(_InternalEntry, _InternalInstance)
                _InternalInstance.Location = pt
            End If

            If Not _InvokeSets.IsNullOrEmpty Then
                For Each Entry In _InvokeSets
                    _InternalInstance.InvokeSet(Of Object)(Entry.Key, Entry.Value)
                Next
            End If

            If _ShowModel Then
                _InternalInstance.ShowDialog()
                _InternalInstance.Free()
            Else
                _InternalInstance.Show()
                AddHandler _InternalInstance.FormClosed, Sub() Call _InternalInstance.Free()
            End If
        End If
    End Sub
End Class
