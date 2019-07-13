#Region "Microsoft.VisualBasic::c8f36b0edfb7202aadaeacfc27e2b874, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\InteractiveIODevice\HistoryStacks.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class HistoryStacks
    ' 
    '         Properties: FilePath, HistoryList
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: __getHistory, MoveFirst, MoveLast, MoveNext, MovePrevious
    '                   (+2 Overloads) Save, ToString
    ' 
    '         Sub: __init, PushStack, StartInitialize
    '         Structure History
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Terminal

    Public Class HistoryStacks : Implements ISaveHandle
        Implements IFileReference

        Dim _historyList As List(Of String)
        Dim _lsthistory As List(Of History)

        ''' <summary>
        ''' 指向<see cref="_historyList"></see>
        ''' </summary>
        ''' <remarks></remarks>
        Dim p As Integer

        Public Property HistoryList As List(Of History)
            Get
                Return _lsthistory
            End Get
            Set(value As List(Of History))
                _lsthistory = value
            End Set
        End Property

        Public Property FilePath As String Implements IFileReference.FilePath

        Dim LastHistory As History

        Sub New()
            LastHistory = New History With {
                .Date = Now.ToString,
                .Histories = New List(Of String)
            }
        End Sub

        Sub New(path As String)
            Call Me.New
            FilePath = path
        End Sub

        Public Sub StartInitialize()
            Call __init()
            _historyList = (From his As History In _lsthistory Select his.Histories).Unlist
            p = _historyList.Count - 1
            If p < 0 Then p = 0
        End Sub

        Public Sub PushStack(s As String)
            If _historyList.IsNullOrEmpty Then
                Call __init()
            End If

            Call LastHistory.Histories.Add(s)
            Call _historyList.Insert(p, s)
        End Sub

        Private Sub __init()
            _historyList = New List(Of String)
            If _lsthistory.IsNullOrEmpty Then _lsthistory = New List(Of History)
            Call _lsthistory.Add(LastHistory)
        End Sub

        Public Function MovePrevious() As String
            p -= 1
            Return __getHistory()
        End Function

        Public Function MoveNext() As String
            p += 1
            Return __getHistory()
        End Function

        Public Function MoveFirst() As String
            p = 0
            Return __getHistory()
        End Function

        Public Function MoveLast() As String
            p = _historyList.Count - 1
            Return __getHistory()
        End Function

        Private Function __getHistory() As String
            If p < 0 Then
                p = 0
            End If

            If _historyList.IsNullOrEmpty Then
                Call __init()
                Return ""
            End If

            If p > _historyList.Count - 1 Then
                p = _historyList.Count - 1
            End If

            Dim s As String = _historyList(p)
            Return s
        End Function

        Public Structure History

            Public [Date] As String
            Public Histories As List(Of String)

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure

        Public Overrides Function ToString() As String
            Return String.Join(";  ", _historyList.Take(3).ToArray) & "......."
        End Function

        Public Function Save(Path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Path = Path Or FilePath.When(Path.StringEmpty)
            Return Me.GetXml.SaveTo(Path, encoding)
        End Function

        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function
    End Class
End Namespace
