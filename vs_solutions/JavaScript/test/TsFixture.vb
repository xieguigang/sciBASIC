#Region "Microsoft.VisualBasic::2d8a5c31b7e04f69a3c8d1e605b4f792, vs_solutions\JavaScript\test\TsFixture.vb"

    ' Author:
    ' 
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

    ' Types exercised by TypeDefinitionTests: every .d.ts emission shape plus the
    ' interop visibility attributes. The XML documentation comments written here are
    ' compiled into LiteJs.Tests.xml (GenerateDocumentationFile), so the generator
    ' auto-detects them and the JSDoc assertions run against real compiler output.

    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

' ---------------- global namespace fixtures ----------------

''' <summary>Enumeration of the widget state (script sees the numeric value).</summary>
Public Enum TsState
    ''' <summary>The widget is idle.</summary>
    Idle = 0
    ''' <summary>The widget is running.</summary>
    Running = 1
    ''' <summary>The widget is paused.</summary>
    Paused = 2
End Enum

''' <summary>A value type fixture with an explicit constructor.</summary>
Public Structure TsPoint

    ''' <summary>The X coordinate.</summary>
    Public X As Double
    ''' <summary>The Y coordinate.</summary>
    Public Y As Double

    ''' <summary>Creates a point from its coordinates.</summary>
    ''' <param name="x">The X coordinate.</param>
    ''' <param name="y">The Y coordinate.</param>
    Public Sub New(x As Double, y As Double)
        Me.X = x
        Me.Y = y
    End Sub

End Structure

''' <summary>A delegate fixture (emitted as a TypeScript function type).</summary>
Public Delegate Function TsTransform(value As Integer) As String

''' <summary>
''' The main widget fixture: data members, methods, statics, overloads, params,
''' optional parameters, an indexer, collections and the visibility attributes.
''' </summary>
Public Class TsWidget
    Implements TsDemo.Generic.TsShape

    ''' <summary>Creates a widget with a generated name.</summary>
    Public Sub New()
        Name = "widget"
    End Sub

    ''' <summary>Creates a named widget.</summary>
    ''' <param name="name">The display name.</param>
    Public Sub New(name As String)
        Me.Name = name
    End Sub

    ' ---------------- data members ----------------

    ''' <summary>Display name of the widget.</summary>
    Public Property Name As String

    ''' <summary>Read-only instance counter.</summary>
    Public ReadOnly Property Count As Integer = 7

    Private _writeOnly As Integer

    ''' <summary>Write-only property: reading it is a TypeError at runtime.</summary>
    Public WriteOnly Property WriteOnlyValue As Integer
        Set(value As Integer)
            _writeOnly = value
        End Set
    End Property

    ''' <summary>State as an enum.</summary>
    Public Property State As TsState = TsState.Idle

    ''' <summary>Creation timestamp (DateTime becomes a JsDate).</summary>
    Public ReadOnly Property Stamp As Date = New Date(2020, 1, 2)

    ''' <summary>Identity token (Guid becomes a string).</summary>
    Public ReadOnly Property Token As Guid = Guid.Empty

    ''' <summary>Item list: indexable, iterable, with a length.</summary>
    Public ReadOnly Property Items As List(Of Double) = New List(Of Double)

    ''' <summary>Plain public field, read/write like a property.</summary>
    Public Counter As Integer

    ''' <summary>Constant field.</summary>
    Public Const MaxCount As Integer = 100

    ''' <summary>Nullable measurement (null maps to Nothing).</summary>
    Public Property Score As Integer?

    ' ---------------- methods ----------------

    ''' <summary>Completed Task(Of T) becomes a fulfilled Promise.</summary>
    Public Function FetchAsync() As Task(Of Integer)
        Return Task.FromResult(42)
    End Function

    ''' <summary>Indexer reached with obj[i].</summary>
    Default Public ReadOnly Property Item(index As Integer) As String
        Get
            Return "item-" & index.ToString()
        End Get
    End Property

    ''' <summary>Adds two integers.</summary>
    ''' <param name="a">The first operand.</param>
    ''' <param name="b">The second operand.</param>
    ''' <returns>The integer sum.</returns>
    Public Function Add(a As Integer, b As Integer) As Integer
        Return a + b
    End Function

    ''' <summary>Concatenates two strings.</summary>
    Public Function Add(a As String, b As String) As String
        Return a & b
    End Function

    ''' <summary>Variadic parameters become a rest parameter.</summary>
    Public Function SumAll(ParamArray values As Integer()) As Integer
        Return 0
    End Function

    ''' <summary>Greets someone.</summary>
    ''' <param name="name">Who to greet.</param>
    ''' <returns>The greeting text.</returns>
    Public Function Greet(Optional name As String = "world") As String
        Return "hi " & name
    End Function

    ''' <summary>Delegate parameter — the bridge rejects it with a readable error.</summary>
    Public Function MapIt(selector As Func(Of Integer, Integer)) As Integer
        Return 0
    End Function

    ''' <summary>Shape area (interface implementation).</summary>
    ''' <returns>The area value.</returns>
    Public Function Area() As Double Implements TsDemo.Generic.TsShape.Area
        Return 1.0
    End Function

    ''' <summary>Obsolete member.</summary>
    ''' <returns>Always zero.</returns>
    <Obsolete("use Area instead")>
    Public Function LegacyArea() As Double
        Return 0.0
    End Function

    ' ---------------- visibility control ----------------

    ''' <summary>Hidden from script by JsIgnore.</summary>
    <JsIgnore>
    Public Property Hidden As String = "hidden"

    ''' <summary>Visible as renamed (no implicit case mapping).</summary>
    <JsName("renamed")>
    Public Property OriginalName As String = "alias"

    ' ---------------- statics ----------------

    ''' <summary>Static factory.</summary>
    Public Shared Function Create() As TsWidget
        Return New TsWidget()
    End Function

    ''' <summary>Static label.</summary>
    Public Shared Property StaticLabel As String = "static"

End Class

''' <summary>
''' Whitelist-policy fixture: one JsExpose member switches the whole type to
''' selective exposure — everything unmarked stays invisible.
''' </summary>
Public Class TsWhitelist

    ''' <summary>Exposed property.</summary>
    <JsExpose>
    Public Property Visible As String = "yes"

    ''' <summary>Unexposed property.</summary>
    Public Property Invisible As String = "no"

    ''' <summary>Exposed method.</summary>
    <JsExpose>
    Public Function VisibleMethod() As String
        Return "visible-method"
    End Function

    ''' <summary>Unexposed method.</summary>
    Public Function InvisibleMethod() As Integer
        Return 7
    End Function

End Class

''' <summary>A VB Module fixture: every member is static.</summary>
Public Module TsToolkit

    ''' <summary>Adds two numbers.</summary>
    ''' <param name="a">The first operand.</param>
    ''' <param name="b">The second operand.</param>
    ''' <returns>The sum.</returns>
    Public Function Add(a As Integer, b As Integer) As Integer
        Return a + b
    End Function

End Module

''' <summary>Nested type fixtures.</summary>
Public Class TsOuter

    ''' <summary>Outer label.</summary>
    Public Property Label As String

    ''' <summary>A type nested inside TsOuter.</summary>
    Public Class TsInner

        ''' <summary>Inner value.</summary>
        Public Property Value As Integer

    End Class

End Class

' ---------------- namespaced fixtures ----------------

Namespace TsDemo.Inner

    ''' <summary>A namespaced widget.</summary>
    Public Class TsNamespaced

        ''' <summary>Namespaced label.</summary>
        Public Property Label As String

    End Class

End Namespace

Namespace TsDemo.Other

    ''' <summary>A colliding simple name: only the first global alias survives.</summary>
    Public Class TsNamespaced

        ''' <summary>Another namespaced label.</summary>
        Public Property Other As String

    End Class

End Namespace

Namespace TsDemo.Generic

    ''' <summary>Interface fixture.</summary>
    Public Interface TsShape

        ''' <summary>Area of the shape.</summary>
        ''' <returns>The area value.</returns>
        Function Area() As Double

    End Interface

    ''' <summary>Generic class fixture with a generic method.</summary>
    Public Class TsBox(Of T)

        ''' <summary>The boxed value.</summary>
        Public Property Value As T

        ''' <summary>Builds a typed box.</summary>
        ''' <param name="value">The value to box.</param>
        Public Shared Function Box(value As T) As TsBox(Of T)
            Return New TsBox(Of T) With {.Value = value}
        End Function

        ''' <summary>Maps the boxed value through a selector.</summary>
        Public Function Map(Of U)(selector As Func(Of T, U)) As TsBox(Of U)
            Return New TsBox(Of U)()
        End Function

        ''' <summary>Wraps a concrete integer (concrete generic argument).</summary>
        Public Function WrapInt(v As Integer) As TsBox(Of Integer)
            Return New TsBox(Of Integer) With {.Value = v}
        End Function

        ''' <summary>A property typed with a concrete generic argument.</summary>
        Public Property TypedBox As TsBox(Of Integer)

        ''' <summary>Returns itself typed with the class parameter (instance method).</summary>
        Public Function Self() As TsBox(Of T)
            Return Me
        End Function

    End Class

    ''' <summary>Two-parameter generic fixture.</summary>
    Public Class TsPair(Of A, B)

        ''' <summary>First slot.</summary>
        Public Property First As A
        ''' <summary>Second slot.</summary>
        Public Property Second As B

        ''' <summary>Boxes the second slot.</summary>
        Public Function BoxSecond() As TsBox(Of B)
            Return New TsBox(Of B)()
        End Function

        ''' <summary>Boxes the first slot.</summary>
        Public Function BoxFirst() As TsBox(Of A)
            Return New TsBox(Of A)()
        End Function

    End Class

End Namespace
