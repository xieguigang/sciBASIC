#Region "Microsoft.VisualBasic::8cf30edc2ed0d95b984e4457087ac479, mime\application%pdf\PdfReader\Parser\Parser.vb"

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

    '   Total Lines: 389
    '    Code Lines: 292
    ' Comment Lines: 31
    '   Blank Lines: 66
    '     File Size: 18.21 KB


    '     Class Parser
    ' 
    '         Properties: Tokenizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: IndirectObjectsToXRef, OnResolveReference, (+2 Overloads) ParseIndirectObject, ParseObject, ParseTrailer
    '                   (+2 Overloads) ParseXRef, ParseXRefOffset, ThrowIfNot
    ' 
    '         Sub: (+2 Overloads) Dispose, ParseHeader, ParseXRefSections, ThrowOnEmptyOrError, ThrowOnError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.IO
Imports System.Runtime.InteropServices

Namespace PdfReader

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/ComponentFactory/PdfReader
    ''' </remarks>
    Public Class Parser
        Implements IDisposable

        Private _disposed As Boolean
        Public Event ResolveReference As EventHandler(Of ParseResolveEventArgs)

        Public Sub New(stream As Stream, Optional allowIdentifiers As Boolean = False)
            Tokenizer = New Tokenizer(stream) With {
                .AllowIdentifiers = allowIdentifiers
            }
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

        Public Sub ParseHeader(<Out> ByRef major As Integer, <Out> ByRef minor As Integer)
            ' The header is a comment token
            Tokenizer.IgnoreComments = False
            Dim t As TokenObject = Tokenizer.GetToken()
            If Not (TypeOf t Is TokenComment) Then Throw New ApplicationException("Missing PDF header.")
            Dim c = CType(t, TokenComment)
            If Not c.Value.StartsWith("%PDF") Then Throw New ApplicationException("PDF Header must start with '%PDF'.")
            Dim splits = c.Value.Substring(5).Split("."c)
            If splits.Length <> 2 Then Throw New ApplicationException("PDF Header must have a <major>.<minor> version number.")
            If Not Integer.TryParse(splits(0).Trim(), major) Then Throw New ApplicationException("Could not parse the header major version number.")
            If Not Integer.TryParse(splits(1).Trim(), minor) Then Throw New ApplicationException("Could not parse the header minor version number.")
            Tokenizer.IgnoreComments = True
        End Sub

        Public Function ParseXRefOffset() As Long
            Return Tokenizer.GetXRefOffset()
        End Function

        Public Function ParseXRef(position As Long) As List(Of TokenXRefEntry)
            Tokenizer.Position = position
            Return ParseXRef()
        End Function

        Public Function ParseXRef() As List(Of TokenXRefEntry)
            Tokenizer.IgnoreComments = True
            Dim t As TokenObject = Tokenizer.GetToken()
            Dim keyword As TokenKeyword = TryCast(t, TokenKeyword)

            If keyword Is Nothing OrElse keyword.Value <> ParseKeyword.XRef Then
                ' Scan entire source creating XRef entries for each indirect object
                Return IndirectObjectsToXRef()
            Else
                Dim entries As List(Of TokenXRefEntry) = New List(Of TokenXRefEntry)()
                ParseXRefSections(entries)
                Return entries
            End If
        End Function

        Public Function IndirectObjectsToXRef() As List(Of TokenXRefEntry)
            Dim entries As List(Of TokenXRefEntry) = New List(Of TokenXRefEntry)()

            ' Start scanning from beginning of the source
            Tokenizer.IgnoreComments = False
            Tokenizer.Position = 0
            Dim lastTrailer As Long = -1

            Do
                Dim t1Position = Tokenizer.Position
                Dim t1 As TokenObject = Tokenizer.GetToken()

                If TypeOf t1 Is TokenInteger Then
                    Dim t2 As TokenInteger = TryCast(Tokenizer.GetToken(), TokenInteger)

                    If t2 IsNot Nothing Then
                        Dim t3 As TokenKeyword = TryCast(Tokenizer.GetToken(), TokenKeyword)

                        If t3 IsNot Nothing AndAlso t3.Value = ParseKeyword.Obj Then
                            Dim id = CType(t1, TokenInteger)
                            entries.Add(New TokenXRefEntry(id.Value, t2.Value, t1Position, True))
                        End If
                    End If
                ElseIf (TypeOf t1 Is TokenKeyword) AndAlso CType(t1, TokenKeyword).Value = ParseKeyword.Trailer Then
                    lastTrailer = t1Position
                End If
            Loop While Tokenizer.GotoNextLine()

            ' Leave with the position on the last 'trailer' as caller will then parse it
            If lastTrailer >= 0 Then Tokenizer.Position = lastTrailer
            Return entries
        End Function

        Public Sub ParseXRefSections(entries As List(Of TokenXRefEntry))
            While True
                Dim t As TokenObject = Tokenizer.GetToken()
                ThrowOnError(t)

                ' Cross-reference table ends when we find a 'trailer' keyword instead of another section
                If (TypeOf t Is TokenKeyword) AndAlso CType(t, TokenKeyword).Value = ParseKeyword.Trailer Then
                    Tokenizer.PushToken(t)
                    Return
                End If

                ' Section starts with an integer object number
                Dim start As TokenInteger = TryCast(t, TokenInteger)
                If start Is Nothing Then Throw New ApplicationException($"Cross-reference section number must be an integer.")
                t = Tokenizer.GetToken()
                ThrowOnError(t)

                ' Section then has an integer length number
                Dim length As TokenInteger = TryCast(t, TokenInteger)
                If length Is Nothing Then Throw New ApplicationException($"Cross-reference section length must be an integer.")

                ' Load each line in the section
                Dim i = 0, id = start.Value

                While i < length.Value
                    Dim entry = Tokenizer.GetXRefEntry(id)
                    ThrowOnError(entry)
                    entries.Add(CType(entry, TokenXRefEntry))
                    i += 1
                    id += 1
                End While
            End While
        End Sub

        Public Function ParseTrailer() As ParseDictionary
            Tokenizer.IgnoreComments = True
            Dim t As TokenObject = Tokenizer.GetToken()
            ThrowOnError(t)

            ' Cross-reference table ends when we find a 'trailer' keyword instead of another section
            If Not (TypeOf t Is TokenKeyword) OrElse CType(t, TokenKeyword).Value <> ParseKeyword.Trailer Then Throw New ApplicationException($"Trailer section must start with the 'trailer' keyword.")
            Dim obj As ParseObjectBase = ParseObject()
            If obj Is Nothing OrElse Not (TypeOf obj Is ParseDictionary) Then Throw New ApplicationException($"Trailer section must contain a dictionary.")
            Return CType(obj, ParseDictionary)
        End Function

        Public Function ParseIndirectObject(position As Long) As ParseIndirectObject
            Dim restore = Tokenizer.Position

            ' Set correct position for parsing the randomly positioned object
            Tokenizer.Position = position
            Dim ret As ParseIndirectObject = ParseIndirectObject()

            ' Must restore original position so caller can continue from where they left off
            Tokenizer.Position = restore
            Return ret
        End Function

        Public Function ParseIndirectObject() As ParseIndirectObject
            Tokenizer.IgnoreComments = True
            Dim t As TokenObject = Tokenizer.GetToken()
            ThrowOnEmptyOrError(t)

            ' Indirect object starts with an integer, the object identifier
            If Not (TypeOf t Is TokenInteger) Then
                Tokenizer.PushToken(t)
                Return Nothing
            End If

            ' Second is another integer, the generation number
            Dim u As TokenObject = Tokenizer.GetToken()
            ThrowOnEmptyOrError(u)

            If Not (TypeOf u Is TokenInteger) Then
                Tokenizer.PushToken(t)
                Tokenizer.PushToken(u)
                Return Nothing
            End If

            ' This is the keyword 'obj'
            Dim v As TokenObject = Tokenizer.GetToken()
            ThrowOnEmptyOrError(v)

            If Not (TypeOf v Is TokenKeyword) OrElse (TryCast(v, TokenKeyword).Value <> ParseKeyword.Obj) Then
                Tokenizer.PushToken(t)
                Tokenizer.PushToken(u)
                Tokenizer.PushToken(v)
                Return Nothing
            End If

            ' Get actual object that is the content
            Dim obj As ParseObjectBase = ParseObject()
            If obj Is Nothing Then Throw New ApplicationException($"Indirect object has missing content.")

            ' Must be followed by either 'endobj' or 'stream'
            v = Tokenizer.GetToken()
            ThrowOnEmptyOrError(v)
            Dim keyword = CType(v, TokenKeyword)
            If keyword Is Nothing Then Throw New ApplicationException($"Indirect object has missing 'endobj or 'stream'.")

            If keyword.Value = ParseKeyword.EndObj Then
                Return New ParseIndirectObject(TryCast(t, TokenInteger), TryCast(u, TokenInteger), obj)
            ElseIf keyword.Value = ParseKeyword.Stream Then
                Dim dictionary As ParseDictionary = TryCast(obj, ParseDictionary)
                If dictionary Is Nothing Then Throw New ApplicationException($"Stream must be preceded by a dictionary.")
                If Not dictionary.ContainsName("Length") Then Throw New ApplicationException($"Stream dictionary must contain a 'Length' entry.")
                Dim lengthObj = dictionary("Length")

                ' Resolve any object reference
                Dim reference As ParseObjectReference = TryCast(lengthObj, ParseObjectReference)
                If reference IsNot Nothing Then lengthObj = OnResolveReference(reference)
                Dim length As ParseInteger = TryCast(lengthObj, ParseInteger)
                If length Is Nothing Then Throw New ApplicationException($"Stream dictionary has a 'Length' entry that is not an integer entry.")
                If length.Value < 0 Then Throw New ApplicationException($"Stream dictionary has a 'Length' less than 0.")
                Dim bytes = Tokenizer.GetBytes(length.Value)
                If bytes Is Nothing Then Throw New ApplicationException($"Cannot read in expected {length.Value} bytes from stream.")

                ' Stream contents must be followed by 'endstream'
                v = Tokenizer.GetToken()
                ThrowOnEmptyOrError(v)
                keyword = CType(v, TokenKeyword)
                If keyword Is Nothing Then Throw New ApplicationException($"Stream has missing 'endstream' after content.")
                If keyword.Value <> ParseKeyword.EndStream Then Throw New ApplicationException($"Stream has unexpected keyword {keyword.Value} instead of 'endstream'.")

                ' Stream contents must be followed by 'endobj'
                v = Tokenizer.GetToken()
                ThrowOnEmptyOrError(v)
                keyword = CType(v, TokenKeyword)
                If keyword Is Nothing Then Throw New ApplicationException($"Indirect object has missing 'endobj'.")
                If keyword.Value <> ParseKeyword.EndObj Then Throw New ApplicationException($"Indirect object has unexpected keyword {keyword.Value} instead of 'endobj'.")
                Return New ParseIndirectObject(TryCast(t, TokenInteger), TryCast(u, TokenInteger), New ParseStream(dictionary, bytes))
            Else
                Throw New ApplicationException($"Indirect object has unexpected keyword {keyword.Value}.")
            End If
        End Function

        Public Function ParseObject(Optional allowEmpty As Boolean = False) As ParseObjectBase
            Tokenizer.IgnoreComments = True
            Dim t As TokenObject = Tokenizer.GetToken()

            If allowEmpty AndAlso (TypeOf t Is TokenEmpty) Then
                Return Nothing
            Else
                ThrowOnEmptyOrError(t)
            End If

            If TypeOf t Is TokenName Then
                ' Store one instance of each unique name to minimize memory footprint
                Dim tokenName = CType(t, TokenName)
                Return ParseName.GetParse(tokenName.Value)
            ElseIf TypeOf t Is TokenInteger Then
                Dim t2 As TokenObject = Tokenizer.GetToken()
                ThrowOnError(t2)

                ' An object reference has a second integer, the generation number
                If TypeOf t2 Is TokenInteger Then
                    Dim t3 As TokenObject = Tokenizer.GetToken()
                    ThrowOnError(t3)

                    ' An object reference has a third value which is the 'R' keyword
                    If (TypeOf t3 Is TokenKeyword) AndAlso CType(t3, TokenKeyword).Value = ParseKeyword.R Then Return New ParseObjectReference(TryCast(t, TokenInteger), TryCast(t2, TokenInteger))
                    Tokenizer.PushToken(t3)
                End If

                Tokenizer.PushToken(t2)
                Return New ParseInteger(TryCast(t, TokenInteger))
            ElseIf TypeOf t Is TokenReal Then
                Return New ParseReal(TryCast(t, TokenReal))
            ElseIf TypeOf t Is TokenStringHex Then
                Return New ParseString(TryCast(t, TokenStringHex))
            ElseIf TypeOf t Is TokenStringLiteral Then
                Return New ParseString(TryCast(t, TokenStringLiteral))
            ElseIf TypeOf t Is TokenArrayOpen Then
                Dim objects As List(Of ParseObjectBase) = New List(Of ParseObjectBase)()
                Dim entry As ParseObjectBase = Nothing

                While True
                    entry = ParseObject()

                    If entry Is Nothing Then
                        Exit While
                    Else
                        ThrowOnEmptyOrError(t)
                    End If

                    objects.Add(entry)
                End While

                Call ThrowIfNot(Of TokenArrayClose)(Tokenizer.GetToken())
                Return New ParseArray(objects)
            ElseIf TypeOf t Is TokenDictionaryOpen Then
                Dim names As List(Of String) = New List(Of String)()
                Dim entries As List(Of ParseObjectBase) = New List(Of ParseObjectBase)()
                Dim value1 As ParseObjectBase = Nothing
                Dim value2 As ParseObjectBase = Nothing

                While True
                    value1 = ParseObject()

                    If value1 Is Nothing Then
                        Exit While
                    Else
                        ThrowOnEmptyOrError(t)
                    End If

                    ' Key value must be a Name
                    Dim name As ParseName = TryCast(value1, ParseName)
                    If name Is Nothing Then Throw New ApplicationException($"Dictionary key must be a name instead of {name.GetType().Name}.")
                    value2 = ParseObject()

                    If value2 Is Nothing Then
                        Throw New ApplicationException($"Dictionary value missing for key {name.Value}.")
                    Else
                        ThrowOnEmptyOrError(t)
                    End If

                    names.Add(name.Value)
                    entries.Add(value2)
                End While

                Call ThrowIfNot(Of TokenDictionaryClose)(Tokenizer.GetToken())
                Return New ParseDictionary(names, entries)
            ElseIf TypeOf t Is TokenKeyword Then

                Select Case TryCast(t, TokenKeyword).Value
                    Case ParseKeyword.True
                        Return ParseObjectBase.True
                    Case ParseKeyword.False
                        Return ParseObjectBase.False
                    Case ParseKeyword.Null
                        Return ParseObjectBase.Null
                End Select
            ElseIf TypeOf t Is TokenIdentifier Then
                ' Store one instance of each unique identifier to minimize memory footprint
                Dim tokenIdentifier = CType(t, TokenIdentifier)
                Return ParseIdentifier.GetParse(tokenIdentifier.Value)
            End If

            ' Token is not one that starts an object, so put the token back
            Tokenizer.PushToken(t)
            Return Nothing
        End Function

        Protected Overridable Function OnResolveReference(reference As ParseObjectReference) As ParseObjectBase
            Dim args As ParseResolveEventArgs = New ParseResolveEventArgs() With {
                .Id = reference.Id,
                .Gen = reference.Gen
            }
            RaiseEvent ResolveReference(Me, args)
            Return args.Object
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    Tokenizer.Dispose()
                    Tokenizer = Nothing
                End If

                _disposed = True
            End If
        End Sub

        Private Function ThrowIfNot(Of tT As TokenObject)(t As TokenObject) As tT
            If TypeOf t Is TokenError Then
                Throw New ApplicationException(t.ToString())
            ElseIf TypeOf t Is TokenEmpty Then
                Throw New ApplicationException("Unexpected end of PDF document.")
            ElseIf Not (TypeOf t Is tT) Then
                Throw New ApplicationException($"Found {t.GetType().Name} instead of {GetType(tT).Name}.")
            End If

            Return t
        End Function

        Private Sub ThrowOnError(t As TokenObject)
            If TypeOf t Is TokenError Then Throw New ApplicationException(t.ToString())
        End Sub

        Private Sub ThrowOnEmptyOrError(t As TokenObject)
            If TypeOf t Is TokenError Then
                Throw New ApplicationException(t.ToString())
            ElseIf TypeOf t Is TokenEmpty Then
                Throw New ApplicationException("Unexpected end of PDF document.")
            End If
        End Sub

        Private Property Tokenizer As Tokenizer
    End Class
End Namespace
