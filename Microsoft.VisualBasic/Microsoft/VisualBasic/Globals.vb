Imports Microsoft.VisualBasic.CompilerServices
Imports System

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class Globals
        ' Properties
        Public Shared ReadOnly Property ScriptEngine As String
            Get
                Return "VB"
            End Get
        End Property

        Public Shared ReadOnly Property ScriptEngineBuildVersion As Integer
            Get
                Return &H40E
            End Get
        End Property

        Public Shared ReadOnly Property ScriptEngineMajorVersion As Integer
            Get
                Return 14
            End Get
        End Property

        Public Shared ReadOnly Property ScriptEngineMinorVersion As Integer
            Get
                Return 0
            End Get
        End Property

    End Class
End Namespace

