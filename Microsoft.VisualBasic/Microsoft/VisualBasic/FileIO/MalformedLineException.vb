Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Runtime.Serialization
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.FileIO
    <Serializable> _
    Public Class MalformedLineException
        Inherits Exception
        ' Methods
        Public Sub New()
        End Sub

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info, context)
            If (Not info Is Nothing) Then
                Me.m_LineNumber = info.GetInt32("LineNumber")
            Else
                Me.m_LineNumber = -1
            End If
        End Sub

        Public Sub New(message As String, innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

        Public Sub New(message As String, lineNumber As Long)
            MyBase.New(message)
            Me.m_LineNumber = lineNumber
        End Sub

        Public Sub New(message As String, lineNumber As Long, innerException As Exception)
            MyBase.New(message, innerException)
            Me.m_LineNumber = lineNumber
        End Sub

        <SecurityCritical, EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.Demand, SerializationFormatter:=True)>
        Public Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
            If (Not info Is Nothing) Then
                info.AddValue("LineNumber", Me.m_LineNumber, GetType(Long))
            End If
            MyBase.GetObjectData(info, context)
        End Sub

        Public Overrides Function ToString() As String
            Dim args As String() = New String() {Me.LineNumber.ToString(CultureInfo.InvariantCulture)}
            Return (MyBase.ToString & " " & Utils.GetResourceString("TextFieldParser_MalformedExtraData", args))
        End Function


        ' Properties
        <EditorBrowsable(EditorBrowsableState.Always)>
        Public Property LineNumber As Long
            Get
                Return Me.m_LineNumber
            End Get
            Set(value As Long)
                Me.m_LineNumber = value
            End Set
        End Property


        ' Fields
        Private Const LINE_NUMBER_PROPERTY As String = "LineNumber"
        Private m_LineNumber As Long
    End Class
End Namespace

