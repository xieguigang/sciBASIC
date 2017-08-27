#Region "Microsoft.VisualBasic::4e83cbcb6c757991f46eb5759cd104d0, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\WinForm\SingleInstanceFormEntry.vb"

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

Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace Windows.Forms

    ''' <summary>
    ''' 只打开一个窗体，当窗体已经打开的时候，就会忽略当前的这一次单击事件，反之没有窗体被打开的时候就会打开新的窗体
    ''' </summary>
    ''' <typeparam name="TForm"></typeparam>
    Public Class SingleInstanceFormEntry(Of TForm As Form)

        Dim _clickEntry As Control
        Dim __getPosition As Func(Of Control, Form, Point)
        Dim _parentForm As Form
        Dim _ShowModel As Boolean

        Public ReadOnly Property Form As TForm

        Public Property Arguments As Object()

        Sub New(ControlEntry As Control,
           Optional ParentForm As Form = Nothing,
           Optional GetPosition As Func(Of Control, Form, Point) = Nothing,
           Optional ShowModelForm As Boolean = True)

            _clickEntry = ControlEntry
            __getPosition = GetPosition
            _parentForm = ParentForm
            _ShowModel = ShowModelForm

            AddHandler _clickEntry.Click, AddressOf __invokeEntry

            If GetPosition Is Nothing AndAlso Not _parentForm Is Nothing Then
                __getPosition = AddressOf __getDefaultPos
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
            AddHandler _clickEntry.Click, New EventHandler(Sub(obj, args) Call Handle(obj, args))
        End Sub

        ''' <summary>
        ''' 默认位置是控件的中间
        ''' </summary>
        ''' <param name="Control"></param>
        ''' <param name="Form"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function __getDefaultPos(Control As UserControl, Form As Form) As Point
            Dim Pt As Point = Form.PointToScreen(Control.Location)
            Pt = New Point(CInt(Pt.X + Control.Width / 2), CInt(Pt.Y + Control.Height / 2))
            Return Pt
        End Function

        Public Sub Invoke(ParamArray InvokeSets As NamedValue(Of Object)())
            If Not _shown Then
                _shown = True
                __invokeSets = InvokeSets
                Call __invokeEntry(Nothing, Nothing)
            Else

            End If
        End Sub

        Dim _shown As Boolean = False
        Dim __invokeSets As NamedValue(Of Object)()

        Private Sub __invokeEntry(sender As Object, EVtargs As EventArgs)
            _Form = DirectCast(Activator.CreateInstance(GetType(TForm), Arguments), TForm)

            If Not __getPosition Is Nothing Then
                Dim pt As Point = __getPosition(_clickEntry, Form)
                Form.Location = pt
            End If

            If Not __invokeSets.IsNullOrEmpty Then
                Dim setValue As New SetValue(Of TForm)()

                For Each arg As NamedValue(Of Object) In __invokeSets
                    Call setValue.InvokeSetValue(Form, arg.Name, arg.Value)
                Next
            End If

            If _ShowModel Then
                Call Form.ShowDialog()
                Call __clean()
            Else
                Call Form.Show()
                AddHandler Form.FormClosed, AddressOf __clean
            End If
        End Sub

        Private Sub __clean()
            Call Form.Free()
            _shown = False
        End Sub
    End Class
End Namespace
