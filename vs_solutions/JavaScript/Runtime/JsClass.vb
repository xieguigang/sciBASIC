#Region "Microsoft.VisualBasic::5a4004faba52a7107b1c381670fb16b8, vs_solutions\JavaScript\Runtime\JsClass.vb"

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

    '   Total Lines: 43
    '    Code Lines: 18 (41.86%)
    ' Comment Lines: 18 (41.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.82 KB


    '     Class JsClass
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' A JavaScript class: a constructor, an instance-member prototype and static
    ''' members.
    ''' <para>
    ''' Instances stay plain object dictionaries (<c>Dictionary(Of String, Object)</c>)
    ''' carrying two internal keys: <c>__proto__</c> → <see cref="Prototype"/> and
    ''' <c>__class__</c> → this class (used for method-home resolution by
    ''' <c>super</c>). Both keys are invisible to for-in / Object.keys.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsClass

        Public ReadOnly Name As String
        ''' <summary>Constructor body (a callable), or Nothing for an empty constructor.</summary>
        Public ReadOnly Constructor As Object
        ''' <summary>Instance members: methods and accessor pairs.</summary>
        Public ReadOnly Prototype As New Dictionary(Of String, Object)
        ''' <summary>Static members: methods, accessors and static fields.</summary>
        Public ReadOnly Statics As New Dictionary(Of String, Object)
        ''' <summary>Superclass, or Nothing for a base class.</summary>
        Public ReadOnly SuperClass As JsClass
        ''' <summary>
        ''' Instance field initialisers (name → zero-argument callable), evaluated
        ''' per construction with <c>this</c> bound to the new instance.
        ''' </summary>
        Public ReadOnly Fields As New List(Of KeyValuePair(Of String, Object))

        Public Sub New(name As String, ctor As Object, superClass As JsClass)
            Me.Name = name
            Me.Constructor = ctor
            Me.SuperClass = superClass
        End Sub

        Public Overrides Function ToString() As String
            Return "class " & Name
        End Function

    End Class

End Namespace

