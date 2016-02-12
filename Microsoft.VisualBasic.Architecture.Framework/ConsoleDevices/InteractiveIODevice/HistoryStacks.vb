Imports Microsoft.VisualBasic.ComponentModel
Imports System.Xml.Serialization

Namespace ConsoleDevice

    Public Class HistoryStacks : Inherits ITextFile
        Implements ISaveHandle

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

        Dim LastHistory As History

        Sub New()
            LastHistory = New History With {
                .Date = Now.ToString,
                .Histories = New List(Of String)
            }
        End Sub

        Public Sub StartInitialize()
            Call __init()
            _historyList = (From his As History In _lsthistory Select his.Histories).MatrixToList
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

        Public Class History
            Public Property [Date] As String
            <XmlElement("History-list")> Public Property Histories As List(Of String)

            Public Overrides Function ToString() As String
                Return String.Format("[{0}]  {1}......", Me.Date, String.Join(";   ", Histories.Take(3).ToArray))
            End Function
        End Class

        Public Overrides Function ToString() As String
            Return String.Join(";  ", _historyList.Take(3).ToArray) & "......."
        End Function

        Public Overrides Function Save(Optional Path As String = "", Optional encoding As System.Text.Encoding = Nothing) As Boolean
            Path = MyBase.getPath(Path)
            Return Me.GetXml.SaveTo(Path, encoding)
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class
End Namespace