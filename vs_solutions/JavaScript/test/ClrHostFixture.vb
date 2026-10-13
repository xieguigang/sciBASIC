#Region "Microsoft.VisualBasic::8260a12cda38ade3b4e8cc8b615c9b25, vs_solutions\JavaScript\test\ClrHostFixture.vb"

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

    '   Total Lines: 202
    '    Code Lines: 108 (53.47%)
    ' Comment Lines: 42 (20.79%)
    '    - Xml Docs: 76.19%
    ' 
    '   Blank Lines: 52 (25.74%)
    '     File Size: 6.51 KB


    ' Enum FixtureLevel
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Structure FixturePoint
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ClrChildFixture
    ' 
    '     Properties: Label, Value
    ' 
    ' Class ClrHostFixture
    ' 
    '     Properties: Child, Hidden, Id, Items, Mode
    '                 OriginalName, Point, Sequence, Stamp, StaticLabel
    '                 Title, Token, WriteOnlyValue
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+3 Overloads) Add, EnumerateSequence, FailsAsync, FetchAsync, Greet
    '               MapIt, StaticSum, SumAll
    ' 
    '     Sub: Explode
    ' 
    ' Class ClrWhitelistFixture
    ' 
    '     Properties: Invisible, Visible
    ' 
    '     Function: InvisibleMethod, VisibleMethod
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

' Host types exercised by LiteJsClrInteropTests: the reflection bridge reads them
' through plain reflection, so no interface or base class is required — only the
' optional attributes that control visibility.

''' <summary>Enum property round-trip (script sees the numeric value).</summary>
Public Enum FixtureLevel
    Off = 0
    Active = 1
End Enum

''' <summary>Struct property: the bridge holds boxed copies, so writes do not persist.</summary>
Public Structure FixturePoint

    Public X As Integer
    Public Y As Integer

    Public Sub New(x As Integer, y As Integer)
        Me.X = x
        Me.Y = y
    End Sub

End Structure

''' <summary>A nested CLR object reachable through a property.</summary>
Public Class ClrChildFixture

    Public Property Label As String = "child"
    Public Property Value As Integer = 5

End Class

''' <summary>
''' Default-policy host object: every public member is visible except the
''' <c>[JsIgnore]</c> ones, and <c>[JsName]</c> renames a member.
''' </summary>
Public Class ClrHostFixture

    Public Sub New()
        Title = "hello"
        Items = New List(Of Double) From {1.5, 2.5, 3.0}
        Stamp = New Date(2020, 1, 2, 3, 4, 5)
        Token = Guid.Parse("11111111-2222-3333-4444-555555555555")
        Point = New FixturePoint(1, 2)
    End Sub

    ' ---------------- data members ----------------

    ''' <summary>Read/write property.</summary>
    Public Property Title As String

    ''' <summary>Public field — read/write like a property.</summary>
    Public Counter As Integer

    ''' <summary>Read-only property: writing it is a TypeError.</summary>
    Public ReadOnly Property Id As Integer = 42

    Private _writeOnly As Integer

    ''' <summary>Write-only property: reading it is a TypeError.</summary>
    Public WriteOnly Property WriteOnlyValue As Integer
        Set(value As Integer)
            _writeOnly = value
        End Set
    End Property

    ''' <summary>Enum property (script side: numeric, assignable by name or number).</summary>
    Public Property Mode As FixtureLevel = FixtureLevel.Off

    ''' <summary>Nested object: stays reachable with dot notation.</summary>
    Public ReadOnly Property Child As New ClrChildFixture()

    ''' <summary>DateTime → JsDate.</summary>
    Public ReadOnly Property Stamp As Date

    ''' <summary>Guid → string.</summary>
    Public ReadOnly Property Token As Guid

    ''' <summary>Value type → a boxed copy; writes never reach this property.</summary>
    Public ReadOnly Property Point As FixturePoint

    ' ---------------- collections ----------------

    ''' <summary>List → an indexable, iterable collection with a <c>length</c>.</summary>
    Public ReadOnly Property Items As List(Of Double)

    ''' <summary>A non-list IEnumerable: iterable, but has no indexer.</summary>
    Public ReadOnly Property Sequence As IEnumerable(Of Integer)
        Get
            Return EnumerateSequence()
        End Get
    End Property

    Private Iterator Function EnumerateSequence() As IEnumerable(Of Integer)
        Yield 10
        Yield 20
    End Function

    ' ---------------- indexer ----------------

    ''' <summary>Indexer reached with <c>obj[i]</c>.</summary>
    Default Public ReadOnly Property Item(index As Integer) As String
        Get
            Return "item-" & index.ToString()
        End Get
    End Property

    ' ---------------- methods ----------------

    ''' <summary>Overloaded by parameter type (script numbers pick the Double form).</summary>
    Public Function Add(a As Integer, b As Integer) As Integer
        Return a + b
    End Function

    Public Function Add(a As Double, b As Double) As Double
        Return a + b
    End Function

    Public Function Add(a As String, b As String) As String
        Return a & b
    End Function

    ''' <summary>Variadic parameters.</summary>
    Public Function SumAll(ParamArray values As Integer()) As Integer
        Dim total = 0
        For Each v In values
            total += v
        Next
        Return total
    End Function

    ''' <summary>Optional parameter.</summary>
    Public Function Greet(Optional name As String = "world") As String
        Return "hi " & name
    End Function

    ''' <summary>Delegate parameter — the bridge rejects it with a readable error.</summary>
    Public Function MapIt(selector As Func(Of Integer, Integer)) As Integer
        Return selector(1)
    End Function

    ''' <summary>Throws, so the script sees a catchable error.</summary>
    Public Sub Explode()
        Throw New InvalidOperationException("boom")
    End Sub

    ' ---------------- tasks ----------------

    ''' <summary>Completed Task(Of T) → fulfilled Promise.</summary>
    Public Function FetchAsync() As Task(Of Integer)
        Return Task.FromResult(41)
    End Function

    ''' <summary>Faulted Task → rejected Promise.</summary>
    Public Function FailsAsync() As Task
        Dim source As New TaskCompletionSource(Of Integer)()
        source.SetException(New InvalidOperationException("async boom"))
        Return source.Task
    End Function

    ' ---------------- visibility control ----------------

    ''' <summary>Hidden from script by [JsIgnore].</summary>
    <JsIgnore>
    Public Property Hidden As String = "hidden"

    ''' <summary>Visible as <c>renamed</c> (no implicit case mapping).</summary>
    <JsName("renamed")>
    Public Property OriginalName As String = "alias"

    ' ---------------- statics ----------------

    Public Shared Property StaticLabel As String = "static"

    Public Shared Function StaticSum(a As Integer, b As Integer) As Integer
        Return a + b
    End Function

End Class

''' <summary>
''' Whitelist-policy host object: one <c>[JsExpose]</c> member switches the whole
''' type to selective exposure — everything unmarked stays invisible.
''' </summary>
Public Class ClrWhitelistFixture

    <JsExpose>
    Public Property Visible As String = "yes"

    Public Property Invisible As String = "no"

    <JsExpose>
    Public Function VisibleMethod() As String
        Return "visible-method"
    End Function

    Public Function InvisibleMethod() As Integer
        Return 7
    End Function

End Class

