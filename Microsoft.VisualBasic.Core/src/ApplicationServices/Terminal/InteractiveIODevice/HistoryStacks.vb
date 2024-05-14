#Region "Microsoft.VisualBasic::6ae599b39eb0aee132d5a6b71ef4900f, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\HistoryStacks.vb"

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

    '   Total Lines: 136
    '    Code Lines: 104
    ' Comment Lines: 4
    '   Blank Lines: 28
    '     File Size: 4.12 KB


    '     Class HistoryStacks
    ' 
    '         Properties: FilePath, HistoryList, MimeType
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
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Terminal

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

        Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {MIME.UnknownType}
            End Get
        End Property

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
