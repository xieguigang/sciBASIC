#Region "Microsoft.VisualBasic::93a29262ff09a1330f332f1fae6d9419, mime\application%pdf\PdfReader\Document\PdfFunctionType3.vb"

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

    '   Total Lines: 64
    '    Code Lines: 47 (73.44%)
    ' Comment Lines: 3 (4.69%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (21.88%)
    '     File Size: 2.50 KB


    '     Class PdfFunctionType3
    ' 
    '         Properties: Bounds, Encode, Functions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Call]
    ' 
    '         Sub: Initialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfFunctionType3
        Inherits PdfFunction

        Private _functions As List(Of PdfFunction)
        Private _boundValues As Single()
        Private _encodeValues As Single()

        Public Sub New(parent As PdfObject, dictionary As PdfDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public ReadOnly Property Functions As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Functions")
            End Get
        End Property

        Public ReadOnly Property Bounds As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Bounds")
            End Get
        End Property

        Public ReadOnly Property Encode As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Encode")
            End Get
        End Property

        Public Overrides Function [Call](inputs As Single()) As Single()
            If inputs.Length <> 1 Then Throw New ArgumentOutOfRangeException($"Provided with '{inputs.Length}' values but Function Type 3 is defined to take 1 value.")

            ' Find the function that handles values below the Bounds value
            Dim i = 0, d = 0

            While i < _boundValues.Length
                If inputs(0) < _boundValues(i) Then Return _functions(i).Call(New Single() {Interpolate(inputs(0), If(i = 0, _domainValues(0), _boundValues(i - 1)), _boundValues(i), _encodeValues(d), _encodeValues(d + 1))})
                i += 1
                d += 2
            End While

            ' Value must be above the last bound, so use the last function
            Return _functions(_functions.Count - 1).Call(New Single() {Interpolate(inputs(0), _boundValues(_boundValues.Length - 1), _domainValues(_domainValues.Length - 1), _encodeValues(_encodeValues.Length - 2), _encodeValues(_encodeValues.Length - 1))})
        End Function

        Protected Overrides Sub Initialize()
            MyBase.Initialize()

            ' Extract and cache values from the dictionary
            _functions = New List(Of PdfFunction)()

            For Each obj In Functions.Objects
                _functions.Add(FromObject(Me, obj))
            Next

            _boundValues = Bounds.AsNumberArray()
            _encodeValues = Encode.AsNumberArray()
        End Sub
    End Class
End Namespace
