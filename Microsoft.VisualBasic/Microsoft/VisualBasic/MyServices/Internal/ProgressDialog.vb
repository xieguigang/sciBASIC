Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Security
Imports System.Threading
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.MyServices.Internal
    Friend Class ProgressDialog
        Inherits Form
        ' Events

        Public Event UserHitCancel As UserHitCancelEventHandler

        ' Methods
        Friend Sub New()
            AddHandler MyBase.FormClosing, New FormClosingEventHandler(AddressOf Me.ProgressDialog_FormClosing)
            AddHandler MyBase.Resize, New EventHandler(AddressOf Me.ProgressDialog_Resize)
            AddHandler MyBase.Shown, New EventHandler(AddressOf Me.ProgressDialog_Activated)
            Me.m_Canceled = False
            Me.m_FormClosableSemaphore = New ManualResetEvent(False)
            Me.InitializeComponent
        End Sub

        Private Sub ButtonCloseDialog_Click(sender As Object, e As EventArgs)
            Me.ButtonCloseDialog.Enabled = False
            Me.m_Canceled = True
            Dim userHitCancelEvent As UserHitCancelEventHandler = Me.UserHitCancelEvent
            If (Not userHitCancelEvent Is Nothing) Then
                userHitCancelEvent.Invoke
            End If
        End Sub

        Public Sub CloseDialog()
            Me.m_CloseDialogInvoked = True
            MyBase.Close()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If (Not Me.components Is Nothing) Then
                    Me.components.Dispose()
                End If
                If (Not Me.m_FormClosableSemaphore Is Nothing) Then
                    Me.m_FormClosableSemaphore.Dispose()
                    Me.m_FormClosableSemaphore = Nothing
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        Public Sub Increment(incrementAmount As Integer)
            Me.ProgressBarWork.Increment(incrementAmount)
        End Sub

        Public Sub IndicateClosing()
            Me.m_Closing = True
        End Sub

        <DebuggerStepThrough>
        Private Sub InitializeComponent()
            Me.LabelInfo = New Label
            Me.ProgressBarWork = New ProgressBar
            Me.ButtonCloseDialog = New Button
            MyBase.SuspendLayout()
            Dim manager1 As New ComponentResourceManager(GetType(ProgressDialog))
            manager1.ApplyResources(Me.LabelInfo, "LabelInfo", CultureInfo.CurrentUICulture)
            Me.LabelInfo.MaximumSize = New Size(300, 0)
            Me.LabelInfo.Name = "LabelInfo"
            manager1.ApplyResources(Me.ProgressBarWork, "ProgressBarWork", CultureInfo.CurrentUICulture)
            Me.ProgressBarWork.Name = "ProgressBarWork"
            manager1.ApplyResources(Me.ButtonCloseDialog, "ButtonCloseDialog", CultureInfo.CurrentUICulture)
            Me.ButtonCloseDialog.Name = "ButtonCloseDialog"
            manager1.ApplyResources(Me, "$this", CultureInfo.CurrentUICulture)
            MyBase.Controls.Add(Me.ButtonCloseDialog)
            MyBase.Controls.Add(Me.ProgressBarWork)
            MyBase.Controls.Add(Me.LabelInfo)
            MyBase.FormBorderStyle = FormBorderStyle.FixedDialog
            MyBase.MaximizeBox = False
            MyBase.MinimizeBox = False
            MyBase.Name = "ProgressDialog"
            MyBase.ShowInTaskbar = False
            MyBase.ResumeLayout(False)
            MyBase.PerformLayout()
        End Sub

        Private Sub ProgressDialog_Activated(sender As Object, e As EventArgs)
            Me.m_FormClosableSemaphore.Set()
        End Sub

        Private Sub ProgressDialog_FormClosing(sender As Object, e As FormClosingEventArgs)
            If (((e.CloseReason = CloseReason.UserClosing) And Not Me.m_CloseDialogInvoked) AndAlso ((Me.ProgressBarWork.Value < 100) And Not Me.m_Canceled)) Then
                e.Cancel = True
                Me.m_Canceled = True
                Dim userHitCancelEvent As UserHitCancelEventHandler = Me.UserHitCancelEvent
                If (Not userHitCancelEvent Is Nothing) Then
                    userHitCancelEvent.Invoke
                End If
            End If
        End Sub

        Private Sub ProgressDialog_Resize(sender As Object, e As EventArgs)
            Me.LabelInfo.MaximumSize = New Size((MyBase.ClientSize.Width - 20), 0)
        End Sub

        Public Sub ShowProgressDialog()
            Try
                If Not Me.m_Closing Then
                    MyBase.ShowDialog()
                End If
            Finally
                Me.FormClosableSemaphore.Set()
            End Try
        End Sub


        ' Properties
        Friend Overridable Property ButtonCloseDialog As Button
            <CompilerGenerated>
            Get
                Return Me._ButtonCloseDialog
            End Get
            <MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated>
            Set(WithEventsValue As Button)
                Dim handler As EventHandler = New EventHandler(AddressOf Me.ButtonCloseDialog_Click)
                Dim button As Button = Me._ButtonCloseDialog
                If (Not button Is Nothing) Then
                    RemoveHandler button.Click, handler
                End If
                Me._ButtonCloseDialog = WithEventsValue
                button = Me._ButtonCloseDialog
                If (Not button Is Nothing) Then
                    AddHandler button.Click, handler
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            <SecuritySafeCritical>
            Get
                Dim createParams As CreateParams = MyBase.CreateParams
                createParams.Style = (createParams.Style Or &H40000)
                Return createParams
            End Get
        End Property

        Public ReadOnly Property FormClosableSemaphore As ManualResetEvent
            Get
                Return Me.m_FormClosableSemaphore
            End Get
        End Property

        Friend Overridable Property LabelInfo As Label
            <CompilerGenerated>
            Get
                Return Me._LabelInfo
            End Get
            <MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated>
            Set(WithEventsValue As Label)
                Me._LabelInfo = WithEventsValue
            End Set
        End Property

        Public Property LabelText As String
            Get
                Return Me.LabelInfo.Text
            End Get
            Set(Value As String)
                Me.LabelInfo.Text = Value
            End Set
        End Property

        Friend Overridable Property ProgressBarWork As ProgressBar
            <CompilerGenerated>
            Get
                Return Me._ProgressBarWork
            End Get
            <MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated>
            Set(WithEventsValue As ProgressBar)
                Me._ProgressBarWork = WithEventsValue
            End Set
        End Property

        Public ReadOnly Property UserCanceledTheDialog As Boolean
            Get
                Return Me.m_Canceled
            End Get
        End Property


        ' Fields
        <CompilerGenerated, AccessedThroughProperty("ButtonCloseDialog")> _
        Private _ButtonCloseDialog As Button
        <CompilerGenerated, AccessedThroughProperty("LabelInfo")> _
        Private _LabelInfo As Label
        <CompilerGenerated, AccessedThroughProperty("ProgressBarWork")> _
        Private _ProgressBarWork As ProgressBar
        Private Const BORDER_SIZE As Integer = 20
        Private components As IContainer
        Private m_Canceled As Boolean
        Private m_CloseDialogInvoked As Boolean
        Private m_Closing As Boolean
        Private m_FormClosableSemaphore As ManualResetEvent
        Private Const WS_THICKFRAME As Integer = &H40000

        ' Nested Types
        Public Delegate Sub UserHitCancelEventHandler()
    End Class
End Namespace

