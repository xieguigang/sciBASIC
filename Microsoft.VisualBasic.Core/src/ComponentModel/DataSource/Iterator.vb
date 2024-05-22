#Region "Microsoft.VisualBasic::6bac56438c6394f3ad3d004ef53f5677, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Iterator.vb"

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

    '   Total Lines: 172
    '    Code Lines: 92 (53.49%)
    ' Comment Lines: 52 (30.23%)
    '    - Xml Docs: 65.38%
    ' 
    '   Blank Lines: 28 (16.28%)
    '     File Size: 6.22 KB


    '     Class Iterator
    ' 
    '         Properties: GetCurrent
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Iterator
    ' 
    '         Properties: Current, ReadDone
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, MoveNext, Read
    ' 
    '         Sub: __moveNext, (+2 Overloads) Dispose, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' 在<see cref="Iterator"/>的基础之上所构建出来的一个泛型化的迭代器对象.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Iterator(Of T) : Inherits Iterator
        Implements IEnumerator(Of T)

        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(source)
        End Sub

        Public ReadOnly Property GetCurrent As T Implements IEnumerator(Of T).Current
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DirectCast(Current, T)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Scripting.ToString(Current)
        End Function
    End Class

    ''' <summary>
    ''' Implements for the <see cref="IEnumerable(Of T)"/>, Supports a simple iteration over a non-generic collection.
    ''' (这个迭代器对象主要是用在远程数据源之中的，对于本地的数据源而言，使用这个迭代器的效率太低了，但是对于远程数据源而言，由于存在网络延迟，所以这个迭代器的效率影响将可以被忽略不计)
    ''' </summary>
    Public Class Iterator : Implements IEnumerator
        Implements IDisposable

        ReadOnly _source As IEnumerable

        Sub New(source As IEnumerable)
            _source = source
            Reset()
        End Sub

        ''' <summary>
        ''' Gets the current element in the collection.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
        ''' <summary>
        ''' Indicates that there are no more characters in the string and tokenizer is finished.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ReadDone As Boolean = False

        ''' <summary>
        ''' Exposes an enumerator, which supports a simple iteration over a non-generic collection.To
        ''' browse the .NET Framework source code for this type, see the Reference Source.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetEnumerator() As IEnumerable
            Return _source
        End Function

        Dim receiveDone As New ManualResetEvent(False)

        Private Sub __moveNext()
            _ReadDone = False
            _started = True

            ' Single thread safely
            ' 单线程安全
            For Each x As Object In _source
                Call receiveDone.WaitOne()
                Call receiveDone.Reset()

                _Current = x
            Next

            _started = False
            _ReadDone = True
        End Sub

        ''' <summary>
        ''' Returns current and then automatically move to next position
        ''' </summary>
        ''' <returns></returns>
        Public Function Read() As Object
            Dim x As Object = Current
            Call MoveNext()
            Return x
        End Function

        Dim _forEach As Thread
        Dim _started As Boolean = False

        ''' <summary>
        ''' Sets the enumerator to its initial position, which is before the first element in the collection.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            If Not _forEach Is Nothing Then
                ' 终止这条线程然后再新建
                Call _forEach.Abort()
            End If

            _Current = Nothing
            _started = False
            _ReadDone = False
            _forEach = New Thread(AddressOf __moveNext)
            _forEach.Start()

            Do While _started = False
                Call Thread.Sleep(1)
            Loop
        End Sub

        ''' <summary>
        ''' Advances the enumerator to the next element of the collection.
        ''' </summary>
        ''' <returns>
        ''' true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        ''' </returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If ReadDone Then
                ' 在移动之前已经读取完毕了
                _Current = Nothing
                Return False
            End If

            Call receiveDone.Set()
            Call Thread.Sleep(1)

            If ReadDone Then
                Return False
            Else
                Return Not ReadDone
            End If
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call _forEach.Abort()
                    Call _forEach.Free
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
