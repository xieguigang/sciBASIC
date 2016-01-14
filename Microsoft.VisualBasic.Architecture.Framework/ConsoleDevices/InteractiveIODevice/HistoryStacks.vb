Namespace ConsoleDevice

    Public Class HistoryStacks : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile
        Implements ISaveHandle

        Dim InternalHistoryList As List(Of String)
        Dim InternalHistories As List(Of History)

        ''' <summary>
        ''' 指向<see cref="InternalHistoryList"></see>
        ''' </summary>
        ''' <remarks></remarks>
        Dim p As Integer

        Public Property HistoryList As List(Of History)
            Get
                Return InternalHistories
            End Get
            Set(value As List(Of History))
                InternalHistories = value
            End Set
        End Property

        Dim LastHistory As History

        Sub New()
            LastHistory = New History With {.Date = Now.ToString, .Histories = New List(Of String)}
        End Sub

        Public Sub StartInitialize()
            Call InternalInitialize()
            InternalHistoryList = (From item In InternalHistories Select item.Histories).ToArray.MatrixToList
            p = InternalHistoryList.Count - 1
            If p < 0 Then p = 0
        End Sub

        Public Sub PushStack(s As String)
            If InternalHistoryList.IsNullOrEmpty Then
                Call InternalInitialize()
            End If

            Call LastHistory.Histories.Add(s)
            Call InternalHistoryList.Insert(p, s)
        End Sub

        Private Sub InternalInitialize()
            InternalHistoryList = New List(Of String)
            If InternalHistories.IsNullOrEmpty Then InternalHistories = New List(Of History)
            Call InternalHistories.Add(LastHistory)
        End Sub

        Public Function MovePrevious() As String
            p -= 1
            Return Internal_getHistory()
        End Function

        Public Function MoveNext() As String
            p += 1
            Return Internal_getHistory()
        End Function

        Public Function MoveFirst() As String
            p = 0
            Return Internal_getHistory()
        End Function

        Public Function MoveLast() As String
            p = InternalHistoryList.Count - 1
            Return Internal_getHistory()
        End Function

        Private Function Internal_getHistory() As String
            If p < 0 Then
                p = 0
            End If

            If InternalHistoryList.IsNullOrEmpty Then
                Call InternalInitialize()
                Return ""
            End If

            If p > InternalHistoryList.Count - 1 Then
                p = InternalHistoryList.Count - 1
            End If

            Dim s As String = InternalHistoryList(p)
            Return s
        End Function

        Public Class History
            Public Property [Date] As String
            <Xml.Serialization.XmlElement("History-list")> Public Property Histories As List(Of String)

            Public Overrides Function ToString() As String
                Return String.Format("[{0}]  {1}......", Me.Date, String.Join(";   ", Histories.Take(3).ToArray))
            End Function
        End Class

        Public Overrides Function ToString() As String
            Return String.Join(";  ", InternalHistoryList.Take(3).ToArray) & "......."
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