#Region "Microsoft.VisualBasic::4ab5c3cbfc31fa803e070a87f0d18930, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\LogicalModel\TabLogicalModel.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports HASH_ENTRY = System.Collections.Generic.KeyValuePair(Of Integer, System.Windows.Forms.Control)

Namespace API

    ''' <summary>
    ''' 仅涉及集合的逻辑操作，不涉及UI的修饰
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TabLogicalModel

        Implements Collections.Generic.IDictionary(Of Integer, Control)
        Implements Generic.IEnumerable(Of Control)

        Dim _containerParent As System.Windows.Forms.Control
        Dim _pageDictionary As Dictionary(Of Integer, Control) = New Dictionary(Of Integer, Control)

        ''' <summary>
        ''' 获取标签页的停靠大小
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GetSize As GetSizeInvoke
        ''' <summary>
        ''' 获取标签页的停靠位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GetLocation As GetLocationInvoke

        Public Delegate Function GetLocationInvoke(container As Control) As Point
        Public Delegate Function GetSizeInvoke(container As Control) As Size

        ''' <summary>
        ''' 返回标签页之中的当前的所打开的页面
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CurrentTab As Control

        Sub New(ByRef parent As System.Windows.Forms.Control, GetSize As GetSizeInvoke, GetLocation As GetLocationInvoke)
            _GetLocation = GetLocation
            _containerParent = parent
            _GetSize = GetSize

            AddHandler _containerParent.Resize, AddressOf __resizeCurrentPanel
        End Sub

        ''' <summary>
        ''' Dynamics add a tabpage control on the form. 
        ''' (函数返回Key，添加的新的标签页控件的哈希值，不能够跨线程添加控件)
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="select">是否将该控件切换至最前面</param>
        ''' <remarks></remarks>
        Public Function Add(Control As Control, [select] As Boolean) As Integer
            Call _containerParent.Controls.Add(Control)
            Dim Key As Integer = Control.GetHashCode()
            Call Add(Key, Control)

            If [select] Then
                Call Me.[Select](Control)
            Else
                Control.Size = _GetSize(_containerParent)
                Control.Location = _GetLocation(_containerParent)
            End If

            Return Key
        End Function

        Public Delegate Function CreateControlInvoke() As Control

        ''' <summary>
        ''' 使用控件的<see cref="Control.GetHashCode()"/>哈希值来作为对象的查找依据
        ''' </summary>
        ''' <param name="CreateControl"></param>
        ''' <returns></returns>
        Public Function Add(CreateControl As CreateControlInvoke) As Control
            Dim Control As Control = CreateControl()
            Call _containerParent.Controls.Add(Control)
            Dim Key As Integer = Control.GetHashCode()
            Call Add(Key, Control)
            Call [Select](Control)
            Return Control
        End Function

        ''' <summary>
        ''' 不存在则返回-1
        ''' </summary>
        ''' <param name="Control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetHandle(Control As Control) As Integer
            Dim LQuery = (From Entry As HASH_ENTRY In Me._pageDictionary
                          Where __findHandle(Control, Entry)
                          Select Entry.Key).ToArray

            If LQuery.IsNullOrEmpty Then
                Return -1
            Else
                Return LQuery.First
            End If
        End Function

        Private Function __findHandle(Control As Control, Entry As HASH_ENTRY) As Boolean
            For Each ctrl In Entry.Value.Controls
                If Control.Equals(ctrl) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 句柄值不存在就返回空值
        ''' </summary>
        ''' <param name="Key"><see cref="Control.GetHashCode()"/></param>
        ''' <returns></returns>
        Public Function [Select](Key As Integer) As Control
            Dim Panel = Me(Key)
            Call [Select](Panel)
            Return Panel
        End Function

        ''' <summary>
        ''' 函数在切换了指定的控件至最前之后还会自动定位以及调整大小
        ''' </summary>
        ''' <param name="Panel">所管理的标签页控件</param>
        Public Sub [Select](Panel As Control)
            If Panel Is Nothing Then Return

            If Not _CurrentTab Is Nothing Then
                _CurrentTab.Visible = False
            End If

            Panel.Visible = True
            Panel.BringToFront()

            Dim prePanel = _CurrentTab
            _CurrentTab = Panel

            Call __resizeCurrentPanel()
        End Sub

        Private Sub __resizeCurrentPanel()
            If CurrentTab Is Nothing Then
                Return
            End If
            Call LayoutControl(CurrentTab)
        End Sub

        Public Sub LayoutControl(ByRef ctrl As Control)
            ctrl.Size = _GetSize(_containerParent)
            ctrl.Location = _GetLocation(_containerParent)
        End Sub

        Public Overrides Function ToString() As String
            Return _containerParent.ToString
        End Function

        Public Function TryGetValue(Handle As Integer) As Control
            If Me._pageDictionary.ContainsKey(Handle) Then
                Return Me._pageDictionary(Handle)
            Else
                Return Nothing
            End If
        End Function

#Region "Implements Collections.Generic.IDictionary(Of Integer, Control) && Generic.IEnumerable(Of Control)"

        Public Sub Add(item As KeyValuePair(Of Integer, Control)) Implements ICollection(Of KeyValuePair(Of Integer, Control)).Add
            Call _pageDictionary.Add(item.Key, item.Value)
            Call _containerParent.Controls.Add(item.Value)

            item.Value.Visible = False
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of Integer, Control)).Clear
            For Each Control As Control In _pageDictionary.Values
                Call _containerParent.Controls.Remove(Control)
            Next

            Call _pageDictionary.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of Integer, Control)) As Boolean Implements ICollection(Of KeyValuePair(Of Integer, Control)).Contains
            Return _pageDictionary.ContainsKey(item.Key)
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of Integer, Control), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of Integer, Control)).CopyTo
            Call _pageDictionary.ToArray.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of Integer, Control)).Count
            Get
                Return _pageDictionary.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of Integer, Control)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyValuePair(Of Integer, Control)) As Boolean Implements ICollection(Of KeyValuePair(Of Integer, Control)).Remove
            Return Remove(item.Key)
        End Function

        ''' <summary>
        ''' 只会想字典里面添加控件句柄，而不会将控件切换到最前面
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        Public Sub Add(key As Integer, value As Control) Implements IDictionary(Of Integer, Control).Add
            Call Add(New KeyValuePair(Of Integer, Control)(key, value))
        End Sub

        Public Function ContainsKey(key As Integer) As Boolean Implements IDictionary(Of Integer, Control).ContainsKey
            Return _pageDictionary.ContainsKey(key)
        End Function

        ''' <summary>
        ''' 不存在则返回空值
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Default Public Property Item(key As Integer) As Control Implements IDictionary(Of Integer, Control).Item
            Get
                If Not _pageDictionary.ContainsKey(key) Then
                    Return Nothing
                Else
                    Return _pageDictionary(key)
                End If
            End Get
            Set(value As Control)
                If _pageDictionary.ContainsKey(key) Then
                    Remove(key)
                End If

                Call Add(key, value)
            End Set
        End Property

        Public ReadOnly Property Keys As ICollection(Of Integer) Implements IDictionary(Of Integer, Control).Keys
            Get
                Return _pageDictionary.Keys
            End Get
        End Property

        Public Function Remove(key As Integer) As Boolean Implements IDictionary(Of Integer, Control).Remove
            If Not _pageDictionary.ContainsKey(key) Then
                Return False
            End If

            Dim value = _pageDictionary(key)

            Call _pageDictionary.Remove(key)
            Call _containerParent.Controls.Remove(value)

            Return True
        End Function

        Public Function TryGetValue(key As Integer, ByRef value As Control) As Boolean Implements IDictionary(Of Integer, Control).TryGetValue
            Return _pageDictionary.TryGetValue(key, value)
        End Function

        Public ReadOnly Property Values As ICollection(Of Control) Implements IDictionary(Of Integer, Control).Values
            Get
                Return _pageDictionary.Values
            End Get
        End Property

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of Integer, Control)) Implements IEnumerable(Of KeyValuePair(Of Integer, Control)).GetEnumerator
            For Each obj In _pageDictionary
                Yield obj
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator2() As IEnumerator(Of Control) Implements IEnumerable(Of Control).GetEnumerator
            For Each obj In _pageDictionary.Values
                Yield obj
            Next
        End Function
#End Region

    End Class
End Namespace
