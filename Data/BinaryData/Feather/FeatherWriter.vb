Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl
Imports Microsoft.VisualBasic.Math.Information
Imports std = System.Math

''' <summary>
''' Indicates how a FeatherWriter should schedule writing to disk.
''' </summary>
Public Enum WriteMode
    ''' <summary>
    ''' Writes are queued, but not actually executed until the writer is being discarded.
    ''' </summary>
    Lazy = 1
    ''' <summary>
    ''' Writes occur immediately, and references to any parameters to the writer are discarded where possible.
    ''' </summary>
    Eager = 2
End Enum

''' <summary>
''' Class for writing dataframes (or equivalently, sets of columns) to a file in the Feather format.
''' 
''' Supports writing lazily (the default) or eagerly.
''' 
''' Eager writes are meant for the cases where individual inputs consume considerable resources, and
''' it is undesirable for a FeatherWriter to hold references to inputs.  An example would be a case
''' where there are many billions of rows in a dataframe, and the GC being able to reclaim whole columns
''' during dataframe persisting is necessary for adequate performance.
''' </summary>
Public NotInheritable Class FeatherWriter
    Implements IDisposable

    ''' <summary>
    ''' WriteMode this FeatherWriter is configured in.
    ''' </summary>

    ''' <summary>
    ''' Number of rows in the dataframe being written
    ''' </summary>
    Private _Mode As WriteMode, _NumRows As Long
    Private NullIndex As Long
    Private DataIndex As Long
    Private VariableIndex As Long

    Private NullStream As BinaryWriter
    Private DataStream As BinaryWriter
    Private VariableStream As BinaryWriter

    Private PendingColumns As LinkedList(Of WriteColumnConfig)
    Private Metadata As LinkedList(Of ColumnMetadata)

    Public Property Mode As WriteMode
        Get
            Return _Mode
        End Get
        Private Set(value As WriteMode)
            _Mode = value
        End Set
    End Property

    Public Property NumRows As Long
        Get
            Return _NumRows
        End Get
        Private Set(value As Long)
            _NumRows = value
        End Set
    End Property
    ''' <summary>
    ''' Number of columns added to this dataframe
    ''' </summary>
    Public ReadOnly Property NumColumns As Integer
        Get
            Return Metadata.Count
        End Get
    End Property

    ''' <summary>
    ''' Create a new FeatherWriter that will persist to the given file.
    ''' Writes are performed lazily.
    ''' 
    ''' Throws if not able to create the file.
    ''' </summary>
    Public Sub New(filePath As String)
        Me.New(filePath, WriteMode.Lazy)
    End Sub

    ''' <summary>
    ''' Create a new FeatherWriter that will persist to the given file.
    ''' 
    ''' Throws if not able to create the file.
    ''' </summary>
    Public Sub New(filePath As String, mode As WriteMode)
        If Equals(filePath, Nothing) Then Throw New ArgumentNullException(NameOf(filePath))

        Select Case mode
            Case WriteMode.Eager, WriteMode.Lazy
            Case Else

                Throw New ArgumentException($"Unexpected Write Mode {mode}", NameOf(mode))
        End Select

        Try
            DataStream = New BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            NullStream = New BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            VariableStream = New BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))

            Setup(mode)
        Catch
            DataStream?.Dispose()
            NullStream?.Dispose()
            VariableStream?.Dispose()

            VariableStream = Nothing
            NullStream = Nothing
            DataStream = Nothing

            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Create a new FeatherWriter that will write to the given stream.
    ''' </summary>
    Public Sub New(outputStream As Stream, mode As WriteMode)
        If outputStream Is Nothing Then Throw New ArgumentNullException(NameOf(outputStream))
        Select Case mode
            Case WriteMode.Eager, WriteMode.Lazy
            Case Else

                Throw New ArgumentException($"Unexpected Write Mode {mode}", NameOf(mode))
        End Select

        Dim multiplexer = New MultiStreamProvider(outputStream)

        Try
            DataStream = New BinaryWriter(multiplexer.CreateChildStream())
            NullStream = New BinaryWriter(multiplexer.CreateChildStream())
            VariableStream = New BinaryWriter(multiplexer.CreateChildStream())

            Setup(mode)
        Catch
            DataStream?.Dispose()
            NullStream?.Dispose()
            VariableStream?.Dispose()

            VariableStream = Nothing
            NullStream = Nothing
            DataStream = Nothing

            Throw
        End Try
    End Sub

    Private Sub Setup(mode As WriteMode)
        Me.Mode = mode
        DataIndex = 0
        NullIndex = 0
        VariableIndex = 0
        PendingColumns = New LinkedList(Of WriteColumnConfig)()
        Metadata = New LinkedList(Of ColumnMetadata)()

    End Sub

    ''' <summary>
    ''' Append a single column to the the dataframe.
    ''' </summary>
    Public Sub AddColumn(Of T)(name As String, column As IEnumerable(Of T))
        If Equals(name, Nothing) Then Throw New ArgumentNullException(NameOf(column))
        If column Is Nothing Then Throw New ArgumentNullException(NameOf(column))
        Dim length As Long
        Dim numNulls As Long
        Dim effectiveType = GetType(T)
        If effectiveType Is GetType(Object) Then
            ' untyped requires inferencing
            DetermineTypeLengthAndNullCount(column, effectiveType, length, numNulls)
        Else
            If IsNonNullable(effectiveType) Then
                numNulls = 0
                length = DetermineLength(column)
            Else
                DetermineLengthAndNullCount(column, length, numNulls)
            End If
        End If

        AddColumnImpl(name, effectiveType, column, length, numNulls)
    End Sub

    ''' <summary>
    ''' Append a single column with a known length to the the dataframe.
    ''' </summary>
    Public Sub AddColumn(Of T)(name As String, column As IEnumerable(Of T), length As Long)
        If Equals(name, Nothing) Then Throw New ArgumentNullException(NameOf(column))
        If column Is Nothing Then Throw New ArgumentNullException(NameOf(column))
        If length < 0 Then Throw New ArgumentOutOfRangeException(NameOf(length), $"Expected value >= 0, found {length}")

        Dim numNulls As Long
        Dim effectiveType = GetType(T)
        If effectiveType Is GetType(Object) Then
            DetermineTypeAndNullCount(column, effectiveType, numNulls)
        Else
            If IsNonNullable(effectiveType) Then
                numNulls = 0
            Else
                numNulls = CountNulls(column)
            End If
        End If

        AddColumnImpl(name, effectiveType, column, length, numNulls)
    End Sub

    ''' <summary>
    ''' Append a single column to the the dataframe.
    ''' </summary>
    Public Sub AddColumn(name As String, column As IEnumerable)
        If column Is Nothing Then Throw New ArgumentNullException(NameOf(column))

        Dim type As Type = Nothing
        Dim length As Long
        Dim nullCount As Long
        DetermineTypeLengthAndNullCount(column, type, length, nullCount)

        AddColumnImpl(name, type, column, length, nullCount)
    End Sub

    ''' <summary>
    ''' Append a single column with a known length to the the dataframe.
    ''' </summary>
    Public Sub AddColumn(name As String, column As IEnumerable, length As Long)
        If Equals(name, Nothing) Then Throw New ArgumentNullException(NameOf(name))
        If column Is Nothing Then Throw New ArgumentNullException(NameOf(column))
        If length < 0 Then Throw New ArgumentOutOfRangeException(NameOf(length), $"Expected value >= 0, found {length}")

        Dim type As Type = Nothing
        Dim nullCount As Long
        DetermineTypeAndNullCount(column, type, nullCount)

        AddColumnImpl(name, type, column, length, nullCount)
    End Sub

    ''' <summary>
    ''' Append a set of columns to the dataframe.
    ''' </summary>
    Public Sub AddColumns(names As IEnumerable(Of String), columns As IEnumerable(Of IEnumerable))
        If names Is Nothing Then Throw New ArgumentNullException(NameOf(names))
        If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))

        Using namesE = names.GetEnumerator()
            Using columnE = columns.GetEnumerator()
                While True
                    Dim cMoveNext = columnE.MoveNext()
                    Dim nMoveNext = namesE.MoveNext()

                    If Not cMoveNext AndAlso Not nMoveNext Then Return

                    If Not cMoveNext Then
                        Throw New InvalidOperationException($"Columns enumerable ran out before names enumerable")
                    End If

                    If Not nMoveNext Then
                        Throw New InvalidOperationException($"Names enumerable ran out before columns enumerable")
                    End If

                    Dim name = namesE.Current
                    Dim column = columnE.Current

                    Dim type As Type = Nothing
                    Dim length As Long
                    Dim nullCount As Long
                    DetermineTypeLengthAndNullCount(column, type, length, nullCount)

                    AddColumnImpl(name, type, column, length, nullCount)
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Append a set of columns with known lengths to the dataframe.
    ''' </summary>
    Public Sub AddColumns(names As IEnumerable(Of String), columns As IEnumerable(Of IEnumerable), lengths As IEnumerable(Of Long))
        If names Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
        If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
        If lengths Is Nothing Then Throw New ArgumentNullException(NameOf(columns))

        Using namesE = names.GetEnumerator()
            Using columnE = columns.GetEnumerator()
                Using lengthE = lengths.GetEnumerator()
                    While True
                        Dim nMoveNext = namesE.MoveNext()
                        Dim cMoveNext = columnE.MoveNext()
                        Dim lMoveNext = lengthE.MoveNext()

                        If Not nMoveNext AndAlso Not cMoveNext AndAlso Not lMoveNext Then Return

                        If Not nMoveNext Then
                            Throw New InvalidOperationException($"Names enumerable ran out before columns and lengths enumerables")
                        End If

                        If Not cMoveNext Then
                            Throw New InvalidOperationException($"Columns enumerable ran out before names and lengths enumerables")
                        End If

                        If Not lMoveNext Then
                            Throw New InvalidOperationException($"Lengths enumerable ran out before columns and data enumerables")
                        End If

                        Dim name = namesE.Current
                        Dim column = columnE.Current
                        Dim length = lengthE.Current
                        Dim type As Type = Nothing
                        Dim nullCount As Long
                        DetermineTypeAndNullCount(column, type, nullCount)

                        AddColumnImpl(name, type, column, length, nullCount)
                    End While
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Append a set of columns to the dataframe.
    ''' </summary>
    Public Sub AddColumns(Of T)(names As IEnumerable(Of String), columns As IEnumerable(Of IEnumerable(Of T)))
        If names Is Nothing Then Throw New ArgumentNullException(NameOf(names))
        If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))

        Dim type = GetType(T)

        Using namesE = names.GetEnumerator()
            Using columnE = columns.GetEnumerator()
                While True
                    Dim nMoveNext = namesE.MoveNext()
                    Dim cMoveNext = columnE.MoveNext()

                    If Not nMoveNext AndAlso Not cMoveNext Then Return

                    If Not nMoveNext Then
                        Throw New InvalidOperationException($"Names enumerable ran out before columns and lengths enumerables")
                    End If

                    If Not cMoveNext Then
                        Throw New InvalidOperationException($"Columns enumerable ran out before names and lengths enumerables")
                    End If

                    Dim name = namesE.Current
                    Dim column = columnE.Current
                    Dim length As Long
                    Dim nullCount As Long
                    Dim effectiveType = type

                    If effectiveType Is GetType(Object) Then
                        ' have to do this inferencing per-column
                        DetermineTypeLengthAndNullCount(column, effectiveType, length, nullCount)
                    Else
                        If IsNonNullable(effectiveType) Then
                            length = DetermineLength(column)
                            nullCount = 0
                        Else
                            DetermineLengthAndNullCount(column, length, nullCount)
                        End If
                    End If

                    AddColumnImpl(name, effectiveType, column, length, nullCount)
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Append a set of columns with known lengths to the dataframe.
    ''' </summary>
    Public Sub AddColumns(Of T)(names As IEnumerable(Of String), columns As IEnumerable(Of IEnumerable(Of T)), lengths As IEnumerable(Of Long))
        If names Is Nothing Then Throw New ArgumentNullException(NameOf(names))
        If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
        If lengths Is Nothing Then Throw New ArgumentNullException(NameOf(lengths))

        Dim type = GetType(T)

        Using namesE = names.GetEnumerator()
            Using columnE = columns.GetEnumerator()
                Using lengthE = lengths.GetEnumerator()
                    While True
                        Dim nMoveNext = namesE.MoveNext()
                        Dim cMoveNext = columnE.MoveNext()
                        Dim lMoveNext = lengthE.MoveNext()

                        If Not nMoveNext AndAlso Not cMoveNext AndAlso Not lMoveNext Then Return

                        If Not nMoveNext Then
                            Throw New InvalidOperationException($"Names enumerable ran out before columns and lengths enumerables")
                        End If

                        If Not cMoveNext Then
                            Throw New InvalidOperationException($"Columns enumerable ran out before names and lengths enumerables")
                        End If

                        If Not lMoveNext Then
                            Throw New InvalidOperationException($"Lengths enumerable ran out before columns and data enumerables")
                        End If

                        Dim name = namesE.Current
                        Dim column = columnE.Current
                        Dim length = lengthE.Current
                        Dim nullCount As Long
                        Dim effectiveType = type

                        If effectiveType Is GetType(Object) Then
                            ' have to do this inferencing per-column
                            DetermineTypeLengthAndNullCount(column, effectiveType, length, nullCount)
                        Else
                            If IsNonNullable(effectiveType) Then
                                nullCount = 0
                            Else
                                nullCount = CountNulls(column)
                            End If
                        End If

                        AddColumnImpl(name, effectiveType, column, length, nullCount)
                    End While
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' <see cref="IDisposable.Dispose"/>
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dim dataCopy = DataStream
        Dim nullCopy = NullStream
        Dim variableCopy = VariableStream

        If dataCopy IsNot Nothing AndAlso nullCopy IsNot Nothing AndAlso variableCopy IsNot Nothing Then
            SerializeAndDiscard()
        End If
    End Sub

    Private Shared Sub DetermineTypeLengthAndNullCount(data As IEnumerable, <Out> ByRef type As System.Type, <Out> ByRef length As Long, <Out> ByRef numNulls As Long)
        Dim realType = data.GetType()
        If realType.IsArray Then
            type = realType.GetElementType()

            If type Is GetType(Object) Then
                ' doesn't matter that we can short-circuit length,
                '   we have to enumerate the whole thing for type
                '   anyway
                GoTo inferFromUntyped
            End If

            length = CType(data, Array).LongLength

            If IsNonNullable(type) Then
                numNulls = 0
            Else
                numNulls = CountNulls(data)
            End If

            Return
        End If

        ' this should cover all the System.Collections.Generic types
        Dim realInterfaces = realType.GetInterfaces()
        Dim iCollection = realInterfaces.FirstOrDefault(Function(i) i.IsGenericType AndAlso i.GetGenericTypeDefinition() Is GetType(ICollection(Of)))
        If iCollection IsNot Nothing Then
            type = iCollection.GetGenericArguments()(0)

            If type Is GetType(Object) Then
                ' doesn't matter that we can short-circuit length,
                '   we have to enumerate the whole thing for type
                '   anyway
                GoTo inferFromUntyped
            End If

            length = type.GetLength(data)

            If IsNonNullable(type) Then
                numNulls = 0
            Else
                numNulls = CountNulls(data)
            End If

            Return
        End If

        ' we have no clue, so inspect every element to figure out what's
        '   actually in the column
inferFromUntyped:
        Dim nulls As Long = 0
        Dim count As Long = 0
        Dim observedTypes = New HashSet(Of Type)()
        For Each elem In data
            count += 1

            If elem Is Nothing Then
                nulls += 1
                Continue For
            End If

            Dim elemType = elem.GetType()
            observedTypes.Add(elemType)
        Next

        Dim widestType = DetermineCoveringType(observedTypes, nulls > 0)

        If widestType Is Nothing Then Throw New InvalidOperationException($"Could not infer column")

        type = widestType
        length = count
        numNulls = nulls
    End Sub

    Private Shared Function DetermineCoveringType(seenTypes As IEnumerable(Of Type), mustBeNullable As Boolean) As Type
        Dim hasDateTime = False
        Dim hasTimeSpan = False
        Dim hasEnums = False

        Dim mostToLeastPermissive = seenTypes _
            .OrderBy(Function(rawType)
                         Dim t = If(Nullable.GetUnderlyingType(rawType), rawType)

                         If t Is GetType(String) Then Return 0
                         If t Is GetType(Double) Then Return 1
                         If t Is GetType(Single) Then Return 2
                         If t Is GetType(Long) Then Return 3
                         If t Is GetType(ULong) Then Return 4
                         If t Is GetType(Integer) Then Return 5
                         If t Is GetType(UInteger) Then Return 6
                         If t Is GetType(Short) Then Return 7
                         If t Is GetType(UShort) Then Return 8
                         If t Is GetType(Byte) Then Return 9
                         If t Is GetType(SByte) Then Return 10

                         If t Is GetType(Date) Then
                             hasDateTime = True
                             Return 11
                         End If
                         If t Is GetType(DateTimeOffset) Then
                             hasDateTime = True
                             Return 11
                         End If

                         If t Is GetType(TimeSpan) Then
                             hasTimeSpan = True
                             Return 12
                         End If

                         If t.IsEnum Then
                             hasEnums = True
                             Return 13
                         End If

                         Throw New InvalidOperationException($"Unexpected type found when trying to determing covering type for untyped column {rawType.Name}")
                     End Function) _
            .ToList()

        If mostToLeastPermissive.Count = 0 Then Return Nothing

        Dim widest = mostToLeastPermissive.First()

        If hasEnums Then
            Dim allEnums = seenTypes.All(Function(t) If(Nullable.GetUnderlyingType(t), t).IsEnum)
            If allEnums Then
                If seenTypes.Count() = 1 Then
                    ' ready made enum, go for it
                    Dim target = seenTypes.[Single]()
                    If mustBeNullable Then
                        target = AssureNullable(target)
                    End If

                    Return target
                End If

                ' multiple enums, so let's merge them all together but maintain the category nature of it
                Dim synthEnum As System.Type = SyntheticEnum.Lookup(seenTypes.[Select](Function(t) If(Nullable.GetUnderlyingType(t), t)))

                If mustBeNullable Then
                    synthEnum = AssureNullable(synthEnum)
                End If

                Return synthEnum
            End If

            ' if there's an enum _AND ANYTHING ELSE_ it's gotta be a string column
            Return GetType(String)
        End If

        If hasTimeSpan Then
            ' it's OK to convert TimeSpan to a string
            If widest Is GetType(String) Then
                Return GetType(String)
            End If

            Dim otherWidest = If(Nullable.GetUnderlyingType(widest), widest)
            If otherWidest IsNot GetType(TimeSpan) Then
                Throw New InvalidOperationException($"Found a mix of types in addition to TimeSpan.  With time values, only TimeSpan and strings may appear in the column data.")
            End If

            Dim target = GetType(TimeSpan)
            If mustBeNullable Then
                target = AssureNullable(target)
            End If

            Return target
        End If

        If hasDateTime Then
            ' it's OK to convert DateTime's to a string
            If widest Is GetType(String) Then
                Return GetType(String)
            End If

            Dim otherWidest = If(Nullable.GetUnderlyingType(widest), widest)
            If otherWidest IsNot GetType(Date) AndAlso otherWidest IsNot GetType(DateTimeOffset) Then
                Throw New InvalidOperationException($"Found a mix of types in addition to DateTime-y types.  With date time values, only DateTime, DateTimeOffset, and string values may appear in the column data.")
            End If

            ' force it to DateTime, not bothering with a special adapter for DateTimeOffset
            Dim target = GetType(Date)
            If mustBeNullable Then
                target = AssureNullable(target)
            End If

            Return target
        End If

        Dim signed = 0
        Dim unsigned = 0

        For Each type In mostToLeastPermissive
            If type Is GetType(Long) OrElse type Is GetType(Integer) OrElse type Is GetType(Short) OrElse type Is GetType(SByte) Then
                signed += 1
                Continue For
            End If

            If type Is GetType(ULong) OrElse type Is GetType(UInteger) OrElse type Is GetType(UShort) OrElse type Is GetType(Byte) Then
                unsigned += 1
                Continue For
            End If
        Next

        Dim basicType As Type
        If widest Is GetType(String) Then
            basicType = GetType(String)
        Else
            If IsFloating(widest) Then
                basicType = widest
            Else
                ' widest is integral now
                If signed > 0 AndAlso unsigned > 0 Then
                    basicType = GetType(Double)     ' expand
                Else
                    ' all the same sign, so just take the biggest one
                    basicType = widest
                End If
            End If
        End If

        Dim ret = If(mustBeNullable, AssureNullable(basicType), basicType)

        Return ret
    End Function

    Private Shared Function AssureNullable(t As Type) As Type
        If t Is Nothing Then Return Nothing

        If Not t.IsValueType Then Return t
        If Nullable.GetUnderlyingType(t) IsNot Nothing Then Return t

        Return GetType(Nullable(Of)).MakeGenericType(t)
    End Function

    Private Shared Function Widen(a As Type, b As Type) As Type
        ' same type? keep it
        If a Is b Then Return a

        ' one is the Type? version of the other, take the Type?
        If AssureNullable(a) Is b Then Return b
        If AssureNullable(b) Is a Then Return a

        ' one is an enum or enum?, the other is a string - take the string
        If a.IsEnum AndAlso b Is GetType(String) Then Return GetType(String)
        If If(Nullable.GetUnderlyingType(a)?.IsEnum, False AndAlso b Is GetType(String)) Then Return GetType(String)
        If b.IsEnum AndAlso a Is GetType(String) Then Return GetType(String)
        If If(Nullable.GetUnderlyingType(b)?.IsEnum, False AndAlso a Is GetType(String)) Then Return GetType(String)

        ' they're both enums (but different ones by definition now) - so convert to string
        If (a.IsEnum OrElse If(Nullable.GetUnderlyingType(a)?.IsEnum, False)) AndAlso (b.IsEnum OrElse If(Nullable.GetUnderlyingType(b)?.IsEnum, False)) Then Return GetType(String)

        ' DateTime widens to DateTimeOffset
        If a Is GetType(Date) AndAlso b Is GetType(DateTimeOffset) Then Return GetType(DateTimeOffset)
        If a Is GetType(Date) AndAlso b Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)
        If a Is GetType(Date?) AndAlso b Is GetType(DateTimeOffset) Then Return GetType(DateTimeOffset?)
        If a Is GetType(Date) AndAlso b Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)
        If a Is GetType(Date?) AndAlso b Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)
        If b Is GetType(Date) AndAlso a Is GetType(DateTimeOffset) Then Return GetType(DateTimeOffset)
        If b Is GetType(Date) AndAlso a Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)
        If b Is GetType(Date?) AndAlso a Is GetType(DateTimeOffset) Then Return GetType(DateTimeOffset?)
        If b Is GetType(Date) AndAlso a Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)
        If b Is GetType(Date?) AndAlso a Is GetType(DateTimeOffset?) Then Return GetType(DateTimeOffset?)

        ' all that's left is number types now
        Dim aIsIntegral = IsIntegral(a)
        Dim aIsFloating = IsFloating(a)
        Dim bIsIntegral = IsIntegral(b)
        Dim bIsFloating = IsFloating(b)

        If Not aIsIntegral AndAlso Not aIsFloating Then Throw New InvalidOperationException($"Cannot map type {a.Name} to a Feather column type")
        If Not bIsIntegral AndAlso Not bIsFloating Then Throw New InvalidOperationException($"Cannot map type {b.Name} to a Feather column type")

        ' can always widen to a floating point from an integral
        If aIsIntegral AndAlso bIsFloating Then Return b
        If bIsIntegral AndAlso aIsFloating Then Return a

        Dim aSize = NumericSize(a)
        Dim bSize = NumericSize(b)

        If aIsFloating AndAlso bIsFloating Then
            If aSize > bSize Then Return a
            If bSize > aSize Then Return b

            Throw New InvalidOperationException($"How can {a.Name} be the same size as {b.Name}; for floating point types")
        End If

        Dim aSign = IntegralSign(a)
        Dim bSign = IntegralSign(b)

        If aSign <> bSign Then
            Throw New InvalidOperationException($"Cannot coerce values of types {a.Name} and {b.Name} to coexist in the same column; signs don't match")
        End If

        If aSize > bSize Then Return a
        If bSize > aSize Then Return b

        Throw New InvalidOperationException($"How can {a.Name} be the same size as {b.Name}; for integral types")
    End Function

    Private Shared Function IsIntegral(t As Type) As Boolean
        Return t Is GetType(Byte) OrElse t Is GetType(SByte) OrElse t Is GetType(Short) OrElse t Is GetType(UShort) OrElse t Is GetType(Integer) OrElse t Is GetType(UInteger) OrElse t Is GetType(Long) OrElse t Is GetType(ULong)
    End Function
    Private Shared Function IsFloating(t As Type) As Boolean
        Return t Is GetType(Single) OrElse t Is GetType(Double)
    End Function

    Private Shared Function NumericSize(t As Type) As Byte
        If t Is GetType(Byte) OrElse t Is GetType(SByte) Then Return 1
        If t Is GetType(Short) OrElse t Is GetType(UShort) Then Return 2
        If t Is GetType(Integer) OrElse t Is GetType(UInteger) Then Return 4
        If t Is GetType(Long) OrElse t Is GetType(ULong) Then Return 8
        If t Is GetType(Single) Then Return 4
        If t Is GetType(Double) Then Return 8

        Throw New InvalidOperationException($"Cannot size unexpected non-numeric type {t.Name}")
    End Function

    Private Shared Function IntegralSign(t As Type) As Integer
        If t Is GetType(SByte) OrElse t Is GetType(Short) OrElse t Is GetType(Integer) OrElse t Is GetType(Long) Then Return -1
        If t Is GetType(Byte) OrElse t Is GetType(UShort) OrElse t Is GetType(UInteger) OrElse t Is GetType(ULong) Then Return 1

        Throw New InvalidOperationException($"Cannot determine sign of non-integral type {t.Name}")
    End Function

    Private Shared Function IsNonNullable(t As Type) As Boolean
        Return t.IsValueType AndAlso Nullable.GetUnderlyingType(t) Is Nothing
    End Function

    Private Shared Function DetermineLength(data As IEnumerable) As Long
        ' arrays!
        Dim realType = data.GetType()
        If realType.IsArray Then
            Return CType(data, Array).GetLongLength(0)
        End If

        ' this should cover all the System.Collections.Generic types
        Dim realInterfaces = realType.GetInterfaces()
        Dim iCollection = realInterfaces.FirstOrDefault(Function(i) i.IsGenericType AndAlso i.GetGenericTypeDefinition() Is GetType(ICollection(Of)))
        If iCollection IsNot Nothing Then
            Return iCollection.GetGenericArguments()(0).GetLength(data)
        End If

        ' handling the System.Collection types
        Dim arrList = TryCast(data, ArrayList)
        If arrList IsNot Nothing Then Return arrList.Count

        Dim bitArr = TryCast(data, BitArray)
        If bitArr IsNot Nothing Then Return bitArr.Count

        Dim hashTable = TryCast(data, Hashtable)
        If hashTable IsNot Nothing Then Return hashTable.Count

        Dim queue = TryCast(data, Queue)
        If queue IsNot Nothing Then Return queue.Count

        Dim sortedList = TryCast(data, SortedList)
        If sortedList IsNot Nothing Then Return sortedList.Count

        Dim stack = TryCast(data, Stack)
        If stack IsNot Nothing Then Return stack.Count

        ' out of tricks for an O(1), just iterate over the whole thing now
        Dim ret As Long = 0
        For Each __ In data
            ret += 1
        Next

        Return ret
    End Function

    Private Shared Sub DetermineTypeAndNullCount(data As IEnumerable, <Out> ByRef type As Type, <Out> ByRef nullCount As Long)
        Dim __ As Long
        DetermineTypeLengthAndNullCount(data, type, __, nullCount)
    End Sub

    Private Shared Sub DetermineLengthAndNullCount(data As IEnumerable, <Out> ByRef length As Long, <Out> ByRef nullCount As Long)
        ' arrays!
        Dim realType = data.GetType()
        If realType.IsArray Then
            Dim elemType = realType.GetElementType()
            length = CType(data, Array).GetLongLength(0)

            If IsNonNullable(elemType) Then
                nullCount = 0
                Return
            End If

            nullCount = CountNulls(data)
            Return
        End If

        ' this should cover all the System.Collections.Generic types
        Dim realInterfaces = realType.GetInterfaces()
        Dim iCollection = realInterfaces.FirstOrDefault(Function(i) i.IsGenericType AndAlso i.GetGenericTypeDefinition() Is GetType(ICollection(Of)))
        If iCollection IsNot Nothing Then
            Dim elemType = iCollection.GetGenericArguments()(0)
            length = elemType.GetLength(data)

            If IsNonNullable(elemType) Then
                nullCount = 0
            Else
                nullCount = CountNulls(data)
            End If

            Return
        End If

        Dim nulls As Long = 0
        Dim count As Long = 0
        For Each elem In data
            count += 1

            If elem Is Nothing Then
                nulls += 1
                Continue For
            End If
        Next

        length = count
        nullCount = nulls
    End Sub

    Private Shared Function CountNulls(data As IEnumerable) As Long
        Dim nulls As Long = 0
        For Each obj In data
            ' boxed Nullable<T> will also == null, so no need for more sophistication
            If obj Is Nothing Then
                nulls += 1
            End If
        Next

        Return nulls
    End Function

    Private Sub AddColumnImpl(name As String, type As Type, data As IEnumerable, length As Long, nullCount As Long)
        Dim onDiskType = MapToDiskType(type, nullCount)

        If PendingColumns.Count = 0 Then
            NumRows = length
        Else
            If NumRows <> length Then
                Throw New InvalidOperationException($"Tried to add a column without the correct number of rows.  Expected {NumRows:N0}, found {length:N0}")
            End If
        End If

        PendingColumns.AddLast(New WriteColumnConfig(name, type, onDiskType, length, data, nullCount))

        If Mode = WriteMode.Eager Then
            SerializePending()
        End If
    End Sub

    Private Shared Function MapToDiskType(type As Type, nullCount As Long) As ColumnType
        ' TODO: Binary & NullableBinary

        If type Is GetType(Boolean) OrElse type Is GetType(Boolean?) Then
            Return If(nullCount > 0, ColumnType.NullableBool, ColumnType.Bool)
        End If

        If type Is GetType(Double) OrElse type Is GetType(Double?) Then
            Return If(nullCount > 0, ColumnType.NullableDouble, ColumnType.Double)
        End If

        If type Is GetType(Single) OrElse type Is GetType(Single?) Then
            Return If(nullCount > 0, ColumnType.NullableFloat, ColumnType.Float)
        End If

        If type Is GetType(Byte) OrElse type Is GetType(Byte?) Then
            Return If(nullCount > 0, ColumnType.NullableUint8, ColumnType.Uint8)
        End If

        If type Is GetType(SByte) OrElse type Is GetType(SByte?) Then
            Return If(nullCount > 0, ColumnType.NullableInt8, ColumnType.Int8)
        End If

        If type Is GetType(SByte) OrElse type Is GetType(SByte?) Then
            Return If(nullCount > 0, ColumnType.NullableInt8, ColumnType.Int8)
        End If

        If type Is GetType(Short) OrElse type Is GetType(Short?) Then
            Return If(nullCount > 0, ColumnType.NullableInt16, ColumnType.Int16)
        End If

        If type Is GetType(UShort) OrElse type Is GetType(UShort?) Then
            Return If(nullCount > 0, ColumnType.NullableUint16, ColumnType.Uint16)
        End If

        If type Is GetType(Integer) OrElse type Is GetType(Integer?) Then
            Return If(nullCount > 0, ColumnType.NullableInt32, ColumnType.Int32)
        End If

        If type Is GetType(UInteger) OrElse type Is GetType(UInteger?) Then
            Return If(nullCount > 0, ColumnType.NullableUint32, ColumnType.Uint32)
        End If

        If type Is GetType(Long) OrElse type Is GetType(Long?) Then
            Return If(nullCount > 0, ColumnType.NullableInt64, ColumnType.Int64)
        End If

        If type Is GetType(ULong) OrElse type Is GetType(ULong?) Then
            Return If(nullCount > 0, ColumnType.NullableUint64, ColumnType.Uint64)
        End If

        If type Is GetType(Date) OrElse type Is GetType(DateTimeOffset) OrElse type Is GetType(Date?) OrElse type Is GetType(DateTimeOffset?) Then
            ' .NET only tracks out to .1 microseconds anyway, so no reason to record at Nanosecond scale
            Return If(nullCount > 0, ColumnType.NullableTimestamp_Microsecond, ColumnType.Timestamp_Microsecond)
        End If

        If type Is GetType(TimeSpan) OrElse type Is GetType(TimeSpan?) Then
            ' .NET only tracks out to .1 microseconds anyway, so no reason to record at Nanosecond scale
            Return If(nullCount > 0, ColumnType.NullableTime_Microsecond, ColumnType.Time_Microsecond)
        End If

        If type Is GetType(String) Then
            Return If(nullCount > 0, ColumnType.NullableString, ColumnType.String)
        End If

        If type.IsEnum OrElse If(Nullable.GetUnderlyingType(type)?.IsEnum, False) Then
            Return If(nullCount > 0, ColumnType.NullableCategory, ColumnType.Category)
        End If

        Throw New InvalidOperationException($"Couldn't map {type.Name} to a Feather type")
    End Function

    Private Sub SerializePending()
        If DataIndex = 0 Then
            WriteMagic()
        End If

        While PendingColumns.Count > 0
            AlignIndexToArrowAlignment(DataStream, DataIndex)

            Dim toProcess = PendingColumns.First.Value
            PendingColumns.RemoveFirst()

            Dim metadataEntry = WriteColumn(toProcess)
            Metadata.AddLast(metadataEntry)
        End While
    End Sub

    Private Shared Sub AlignIndexToArrowAlignment(stream As BinaryWriter, ByRef currentIndex As Long)
        Dim advanceBy = currentIndex Mod ARROW_ALIGNMENT
        If advanceBy = 0 Then Return

        If stream.BaseStream.CanSeek Then
            stream.BaseStream.Seek(advanceBy, SeekOrigin.Current)
        Else
            ' handle streams we can't seek on -_-
            For i = 0 To advanceBy - 1
                stream.BaseStream.ReadByte()
            Next
        End If

        currentIndex += advanceBy
    End Sub

    Private Sub AdvanceNullStreamTo(position As Long)
        AdvanceStreamTo(NullStream, NullIndex, position)
    End Sub
    Private Sub AdvanceDataStreamTo(position As Long)
        AdvanceStreamTo(DataStream, DataIndex, position)
    End Sub
    Private Sub AdvanceVariableStreamTo(position As Long)
        AdvanceStreamTo(VariableStream, VariableIndex, position)
    End Sub

    Private Shared Sub AdvanceStreamTo(stream As BinaryWriter, ByRef index As Long, position As Long)
        Dim advanceBy = position - index

        If advanceBy < 0 Then Throw New Exception($"Shouldn't be possible, tried to move stream back in the file; From: {index:N0} To: {position:N0}")

        If advanceBy = 0 Then Return

        If stream.BaseStream.CanSeek Then
            stream.BaseStream.Seek(advanceBy, SeekOrigin.Current)
        Else
            ' handle streams we can't seek on -_-
            For i = 0 To advanceBy - 1
                stream.BaseStream.ReadByte()
            Next
        End If

        stream.Flush()

        index = position
    End Sub

    Private Function WriteColumn(ByRef column As WriteColumnConfig) As ColumnMetadata
        Dim dataOffset As Long

        Dim levels As String() = Nothing
        Dim timezone As String = Nothing
        Dim unit As DateTimePrecisionType

        ' advance all streams to latest point, so any backing buffers know they can flush
        Dim furtherPoint = std.Max(std.Max(DataIndex, NullIndex), VariableIndex)
        AdvanceDataStreamTo(furtherPoint)
        AdvanceNullStreamTo(furtherPoint)
        AdvanceVariableStreamTo(furtherPoint)

        Dim onDiskType = column.OnDiskType

        Dim hasNulls = onDiskType = ColumnType.NullableBinary OrElse onDiskType = ColumnType.NullableBool OrElse onDiskType = ColumnType.NullableCategory OrElse onDiskType = ColumnType.NullableDate OrElse onDiskType = ColumnType.NullableDouble OrElse onDiskType = ColumnType.NullableFloat OrElse onDiskType = ColumnType.NullableInt16 OrElse onDiskType = ColumnType.NullableInt32 OrElse onDiskType = ColumnType.NullableInt64 OrElse onDiskType = ColumnType.NullableInt8 OrElse onDiskType = ColumnType.NullableString OrElse onDiskType = ColumnType.NullableTimestamp_Microsecond OrElse onDiskType = ColumnType.NullableTimestamp_Millisecond OrElse onDiskType = ColumnType.NullableTimestamp_Nanosecond OrElse onDiskType = ColumnType.NullableTimestamp_Second OrElse onDiskType = ColumnType.NullableTime_Microsecond OrElse onDiskType = ColumnType.NullableTime_Millisecond OrElse onDiskType = ColumnType.NullableTime_Nanosecond OrElse onDiskType = ColumnType.NullableTime_Second OrElse onDiskType = ColumnType.NullableUint16 OrElse onDiskType = ColumnType.NullableUint32 OrElse onDiskType = ColumnType.NullableUint64 OrElse onDiskType = ColumnType.NullableUint8

        Dim isVariableSized = onDiskType = ColumnType.Binary OrElse onDiskType = ColumnType.String OrElse onDiskType = ColumnType.NullableBinary OrElse onDiskType = ColumnType.NullableString

        Dim bytesWritten As Long

        If Not isVariableSized Then
            If hasNulls Then
                WriteNullableData(column.DotNetType, column.Data, column.Length, column.OnDiskType, dataOffset, bytesWritten, levels, timezone, unit)
            Else
                WriteData(column.DotNetType, column.Data, column.Length, column.OnDiskType, dataOffset, bytesWritten, levels, timezone, unit)
            End If
        Else
            If hasNulls Then
                WriteNullableVariableSizedData(column.DotNetType, column.Data, column.Length, column.OnDiskType, dataOffset, bytesWritten, levels, timezone, unit)
            Else
                WriteVariableSizedData(column.DotNetType, column.Data, column.Length, column.OnDiskType, dataOffset, bytesWritten, levels, timezone, unit)
            End If
        End If

        Return New ColumnMetadata With {
.Encoding = FbsMetadata.Encoding.PLAIN,
.Length = column.Length,
.Levels = levels,
.Name = column.Name,
.NullCount = column.NullCount,
.Offset = dataOffset,
.Ordered = False,
.TimeZone = timezone,
.TotalBytes = bytesWritten,
.Type = column.OnDiskType,
.Unit = unit
}
    End Function

    Friend Sub WriteLevels(levels As String(), <Out> ByRef startIndex As Long, <Out> ByRef numBytes As Long)
        AlignIndexToArrowAlignment(DataStream, DataIndex)

        startIndex = DataIndex

        Dim __ As String() = Nothing
        Dim ___ As String = Nothing
        Dim ____ As DateTimePrecisionType = Nothing
        WriteVariableSizedData(GetType(String), levels, levels.Length, ColumnType.String, startIndex, numBytes, __, ___, ____)
    End Sub

    Private Sub WriteVariableSizedData(dataType As Type, data As IEnumerable, length As Long, onDiskType As ColumnType, <Out> ByRef dataOffset As Long, <Out> ByRef bytesWritten As Long, <Out> ByRef categoryLevels As String(), <Out> ByRef timestampTimezone As String, <Out> ByRef timeUnit As DateTimePrecisionType)
        dataOffset = DataIndex

        Dim dataStartIndex = DataIndex
        AdvanceDataStreamTo(dataStartIndex)

        Dim neededDataEntries = length + 1                         ' need the last index to read into when parsing the strings out
        Dim neededDataBytes = neededDataEntries * HeapSizeOf.int

        Dim variableStartIndex = dataStartIndex + neededDataBytes

        Dim variableStartPadding = 0
        If variableStartIndex Mod ARROW_ALIGNMENT <> 0 Then
            variableStartPadding = ARROW_ALIGNMENT - CInt(variableStartIndex Mod ARROW_ALIGNMENT)
        End If
        variableStartIndex += variableStartPadding

        AdvanceVariableStreamTo(variableStartIndex)

        Dim adapter = LookupAdapter(data.GetType(), dataType, onDiskType)
        adapter(Me, data)

        Dim finalIndex = VariableIndex

        ' skip past all the data we just wrote
        AdvanceDataStreamTo(VariableIndex)

        ' nothing variable sized ever has metadata
        categoryLevels = Nothing
        timestampTimezone = Nothing
        timeUnit = DateTimePrecisionType.NONE

        bytesWritten = finalIndex - dataOffset
    End Sub

    Private Sub WriteNullableVariableSizedData(dataType As Type, data As IEnumerable, length As Long, onDiskType As ColumnType, <Out> ByRef dataOffset As Long, <Out> ByRef bytesWritten As Long, <Out> ByRef categoryLevels As String(), <Out> ByRef timestampTimezone As String, <Out> ByRef timeUnit As DateTimePrecisionType)
        dataOffset = DataIndex

        Dim nullStartIndex = DataIndex
        AdvanceNullStreamTo(nullStartIndex)

        Dim neededNullBytes = length / 8
        If length Mod 8 <> 0 Then neededNullBytes += 1

        Dim nullPadding = 0
        If neededNullBytes Mod NULL_BITMASK_ALIGNMENT <> 0 Then
            nullPadding = NULL_BITMASK_ALIGNMENT - CInt(neededNullBytes Mod NULL_BITMASK_ALIGNMENT)
        End If

        Dim dataStartIndex = nullStartIndex + neededNullBytes + nullPadding
        AdvanceDataStreamTo(dataStartIndex)

        Dim neededDataEntries = length + 1                         ' need the last index to read into when parsing the strings out
        Dim neededDataBytes = neededDataEntries * HeapSizeOf.int

        Dim variableStartIndex = dataStartIndex + neededDataBytes

        Dim variableStartPadding = 0
        If variableStartIndex Mod ARROW_ALIGNMENT <> 0 Then
            variableStartPadding = ARROW_ALIGNMENT - CInt(variableStartIndex Mod ARROW_ALIGNMENT)
        End If
        variableStartIndex += variableStartPadding

        AdvanceVariableStreamTo(variableStartIndex)

        Dim adapter = LookupAdapter(data.GetType(), dataType, onDiskType)
        adapter(Me, data)

        Dim finalIndex = VariableIndex

        ' skip past all the crap we just wrote
        AdvanceDataStreamTo(VariableIndex)

        ' nothing variable sized ever has metadata
        categoryLevels = Nothing
        timestampTimezone = Nothing
        timeUnit = DateTimePrecisionType.NONE

        bytesWritten = finalIndex - dataOffset
    End Sub

    Private Sub WriteNullableData(dataType As Type, data As IEnumerable, length As Long, onDiskType As ColumnType, <Out> ByRef dataOffset As Long, <Out> ByRef bytesWritten As Long, <Out> ByRef categoryLevels As String(), <Out> ByRef timestampTimezone As String, <Out> ByRef timeUnit As DateTimePrecisionType)
        dataOffset = DataIndex

        Dim nullStartIndex = DataIndex
        AdvanceNullStreamTo(nullStartIndex)

        Dim neededNullBytes = length / 8
        If length Mod 8 <> 0 Then neededNullBytes += 1

        Dim nullPadding = 0
        If neededNullBytes Mod NULL_BITMASK_ALIGNMENT <> 0 Then
            nullPadding = NULL_BITMASK_ALIGNMENT - CInt(neededNullBytes Mod NULL_BITMASK_ALIGNMENT)
        End If

        Dim dataStartIndex = nullStartIndex + neededNullBytes + nullPadding
        AdvanceDataStreamTo(dataStartIndex)

        Dim adapter = LookupAdapter(data.GetType(), dataType, onDiskType)
        adapter(Me, data)

        Dim finalIndex = DataIndex

        Dim isDateTime = onDiskType = ColumnType.Date OrElse onDiskType = ColumnType.NullableDate OrElse onDiskType = ColumnType.Timestamp_Microsecond OrElse onDiskType = ColumnType.Timestamp_Millisecond OrElse onDiskType = ColumnType.Timestamp_Nanosecond OrElse onDiskType = ColumnType.Timestamp_Second OrElse onDiskType = ColumnType.NullableTimestamp_Microsecond OrElse onDiskType = ColumnType.NullableTimestamp_Millisecond OrElse onDiskType = ColumnType.NullableTimestamp_Nanosecond OrElse onDiskType = ColumnType.NullableTimestamp_Second

        Dim isTime = onDiskType = ColumnType.Time_Microsecond OrElse onDiskType = ColumnType.Time_Millisecond OrElse onDiskType = ColumnType.Time_Nanosecond OrElse onDiskType = ColumnType.Time_Second OrElse onDiskType = ColumnType.NullableTime_Microsecond OrElse onDiskType = ColumnType.NullableTime_Millisecond OrElse onDiskType = ColumnType.NullableTime_Nanosecond OrElse onDiskType = ColumnType.NullableTime_Second

        Dim isEnum = onDiskType = ColumnType.Category OrElse onDiskType = ColumnType.NullableCategory

        If isDateTime Then
            categoryLevels = Nothing
            timestampTimezone = "GMT"                          ' always UTC
            timeUnit = DateTimePrecisionType.Microsecond       ' always microsecond (.1 tick) precision
        Else
            If isTime Then
                categoryLevels = Nothing
                timestampTimezone = Nothing
                timeUnit = DateTimePrecisionType.Microsecond       ' always microsecond (.1 tick) precision
            Else
                If isEnum Then
                    timestampTimezone = Nothing
                    timeUnit = DateTimePrecisionType.NONE
                    categoryLevels = GetLevels(dataType)
                Else
                    categoryLevels = Nothing
                    timestampTimezone = Nothing
                    timeUnit = DateTimePrecisionType.NONE
                End If
            End If
        End If

        bytesWritten = finalIndex - dataOffset
    End Sub

    Private Sub WriteData(dataType As Type, data As IEnumerable, length As Long, onDiskType As ColumnType, <Out> ByRef dataOffset As Long, <Out> ByRef bytesWritten As Long, <Out> ByRef categoryLevels As String(), <Out> ByRef timestampTimezone As String, <Out> ByRef timeUnit As DateTimePrecisionType)
        dataOffset = DataIndex

        Dim adapter = LookupAdapter(data.GetType(), dataType, onDiskType)
        adapter(Me, data)
        Dim finalIndex = DataIndex

        Dim isDateTime = onDiskType = ColumnType.Date OrElse onDiskType = ColumnType.NullableDate OrElse onDiskType = ColumnType.Timestamp_Microsecond OrElse onDiskType = ColumnType.Timestamp_Millisecond OrElse onDiskType = ColumnType.Timestamp_Nanosecond OrElse onDiskType = ColumnType.Timestamp_Second OrElse onDiskType = ColumnType.NullableTimestamp_Microsecond OrElse onDiskType = ColumnType.NullableTimestamp_Millisecond OrElse onDiskType = ColumnType.NullableTimestamp_Nanosecond OrElse onDiskType = ColumnType.NullableTimestamp_Second

        Dim isTime = onDiskType = ColumnType.Time_Microsecond OrElse onDiskType = ColumnType.Time_Millisecond OrElse onDiskType = ColumnType.Time_Nanosecond OrElse onDiskType = ColumnType.Time_Second OrElse onDiskType = ColumnType.NullableTime_Microsecond OrElse onDiskType = ColumnType.NullableTime_Millisecond OrElse onDiskType = ColumnType.NullableTime_Nanosecond OrElse onDiskType = ColumnType.NullableTime_Second

        Dim isEnum = onDiskType = ColumnType.Category OrElse onDiskType = ColumnType.NullableCategory

        If isDateTime Then
            categoryLevels = Nothing
            timestampTimezone = "GMT"                          ' always UTC
            timeUnit = DateTimePrecisionType.Microsecond       ' always microsecond (.1 tick) precision
        Else
            If isTime Then
                categoryLevels = Nothing
                timestampTimezone = Nothing
                timeUnit = DateTimePrecisionType.Microsecond       ' always microsecond (.1 tick) precision
            Else
                If isEnum Then
                    timestampTimezone = Nothing
                    timeUnit = DateTimePrecisionType.NONE
                    categoryLevels = GetLevels(dataType)
                Else
                    categoryLevels = Nothing
                    timestampTimezone = Nothing
                    timeUnit = DateTimePrecisionType.NONE
                End If
            End If
        End If

        bytesWritten = finalIndex - dataOffset
    End Sub

    Private Shared Function NumberOfBytesForNullMask(ByRef config As WriteColumnConfig) As Long
        Dim numNullBytes As Long
        If config.NullCount = 0 Then
            Return 0
        Else
            numNullBytes = config.Length / 8
            If config.Length Mod 8 <> 0 Then
                numNullBytes += 1
            End If
        End If

        Dim numNullBytesWithPadding As Long
        If numNullBytes Mod NULL_BITMASK_ALIGNMENT = 0 Then
            numNullBytesWithPadding = numNullBytes
        Else
            numNullBytesWithPadding = numNullBytes + numNullBytes Mod NULL_BITMASK_ALIGNMENT
        End If

        Return numNullBytes
    End Function

    Private Sub CloseAndDiscard()
        WriteMetadata()

        WriteTrailingMagic()

        DataStream.Dispose()
        NullStream.Dispose()
        VariableStream.Dispose()

        VariableStream = Nothing
        NullStream = Nothing
        DataStream = Nothing
    End Sub

    Private Sub WriteMetadata()
        Dim bufferBuilder = New FlatBuffers.FlatBufferBuilder(128)

        ' note: R & Python feather impls don't handle the default (ForceDefaults = false)
        '    configuration correctly so always write default values
        bufferBuilder.ForceDefaults = True

        Dim columns = New List(Of FlatBuffers.Offset(Of FbsMetadata.Column))()
        For Each col In Metadata
            Dim colName = bufferBuilder.CreateString(col.Name)

            Dim primArr = FbsMetadata.PrimitiveArray.CreatePrimitiveArray(bufferBuilder, col.Type.MapToFeatherEnum(), col.Encoding, col.Offset, col.Length, col.NullCount, col.TotalBytes)

            Dim metadataType As FbsMetadata.TypeMetadata
            Dim levels As FlatBuffers.Offset(Of FbsMetadata.PrimitiveArray)
            Dim metaDataOffset As Integer

            col.CreateMetadata(bufferBuilder, Me, metaDataOffset, metadataType, levels)

            Dim colRef = FbsMetadata.Column.CreateColumn(bufferBuilder, colName, primArr, metadataType, metaDataOffset, Nothing)

            columns.Add(colRef)
        Next

        Dim columnsVec = FbsMetadata.CTable.CreateColumnsVector(bufferBuilder, columns.ToArray())

        Dim ctable = FbsMetadata.CTable.CreateCTable(bufferBuilder, Nothing, NumRows, columnsVec, FEATHER_VERSION, Nothing)

        bufferBuilder.Finish(ctable.Value)

        Dim bytes = bufferBuilder.SizedByteArray()

        AlignIndexToArrowAlignment(DataStream, DataIndex)
        DataStream.Write(bytes, 0, bytes.Length)
        DataStream.Write(bytes.Length)

        DataIndex += bytes.Length
        DataIndex += HeapSizeOf.int
    End Sub

    Private Sub WriteMagic()
        DataStream.Write(MAGIC_HEADER)
        DataIndex += MAGIC_HEADER_SIZE
    End Sub

    Private Sub WriteLeadingMagic()
        WriteMagic()
    End Sub
    Private Sub WriteTrailingMagic()
        WriteMagic()
    End Sub

    Private Sub SerializeAndDiscard()
        SerializePending()
        CloseAndDiscard()
    End Sub

    Friend Sub BlitNonNullableBoolArray(arr As Boolean())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim b = arr(i)

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                DataStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If
        Next

        If ix <> 0 Then
            DataStream.Write(currentByte)
        End If

        DataIndex += arr.LongLength / 8
        If arr.LongLength Mod 8 <> 0 Then
            DataIndex += 1
        End If
    End Sub

    Friend Sub CopyNonNullableBoolCollection(col As ICollection(Of Boolean))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each b In col
            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                DataStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If
        Next

        If ix <> 0 Then
            DataStream.Write(currentByte)
        End If

        DataIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            DataIndex += 1
        End If
    End Sub

    Friend Sub CopyNonNullableBoolIEnumerable(col As IEnumerable(Of Boolean))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each b In col
            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                DataStream.Write(currentByte)
                ix = 0
                currentByte = 0
                DataIndex += 1
            End If
        Next

        If ix <> 0 Then
            DataStream.Write(currentByte)
            DataIndex += 1
        End If
    End Sub

    Friend Sub BlitNullableBoolArray(arr As Boolean?())
        Dim nullableByte As Byte = 0
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim b = arr(i)

            If b.HasValue Then
                nullableByte = nullableByte Or CByte(1 << ix)
                If b.Value Then
                    currentByte = currentByte Or CByte(1 << ix)
                End If
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(nullableByte)
                DataStream.Write(currentByte)
                ix = 0
                nullableByte = 0
                currentByte = 0
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(nullableByte)
            DataStream.Write(currentByte)
        End If

        Dim bytesSpace = arr.LongLength / 8

        NullIndex += bytesSpace
        DataIndex += bytesSpace
        If arr.LongLength Mod 8 <> 0 Then
            NullIndex += 1
            DataIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableBoolCollection(col As ICollection(Of Boolean?))
        Dim nullableByte As Byte = 0
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each b In col
            If b.HasValue Then
                nullableByte = nullableByte Or CByte(1 << ix)
                If b.Value Then
                    currentByte = currentByte Or CByte(1 << ix)
                End If
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(nullableByte)
                DataStream.Write(currentByte)
                ix = 0
                nullableByte = 0
                currentByte = 0
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(nullableByte)
            DataStream.Write(currentByte)
        End If

        Dim bytesSpace = col.Count / 8

        NullIndex += bytesSpace
        DataIndex += bytesSpace
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
            DataIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableBoolIEnumerable(col As IEnumerable(Of Boolean?))
        Dim nullableByte As Byte = 0
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each b In col
            If b.HasValue Then
                nullableByte = nullableByte Or CByte(1 << ix)
                If b.Value Then
                    currentByte = currentByte Or CByte(1 << ix)
                End If
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(nullableByte)
                DataStream.Write(currentByte)
                ix = 0
                nullableByte = 0
                currentByte = 0

                NullIndex += 1
                DataIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(nullableByte)
            DataStream.Write(currentByte)
            NullIndex += 1
            DataIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Double())
        For Each val As Double In arr
            Dim asLong = BitConverter.DoubleToInt64Bits(val)
            DataStream.Write(asLong)
        Next

        DataIndex += arr.Length * HeapSizeOf.double
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Double))
        For Each val As Double In col
            Dim asLong = BitConverter.DoubleToInt64Bits(val)
            DataStream.Write(asLong)
        Next

        DataIndex += col.Count * HeapSizeOf.double
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Double))
        For Each val As Double In col
            Dim asLong = BitConverter.DoubleToInt64Bits(val)
            DataStream.Write(asLong)
            DataIndex += HeapSizeOf.double
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Double?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(BitConverter.DoubleToInt64Bits(If(val, Double.NaN)))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.double

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Double?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Double? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(BitConverter.DoubleToInt64Bits(If(val, Double.NaN)))
        Next

        DataIndex += col.Count * HeapSizeOf.double

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Double?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Double? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(BitConverter.DoubleToInt64Bits(If(val, Double.NaN)))
            DataIndex += HeapSizeOf.double
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Single())
        For Each val As Single In arr
            Dim asInt = UncheckHelpers.SingleToInt32Bits(val)
            DataStream.Write(asInt)
        Next

        DataIndex += arr.Length * HeapSizeOf.float
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Single))
        For Each val As Single In col
            Dim asInt = UncheckHelpers.SingleToInt32Bits(val)
            DataStream.Write(asInt)
        Next

        DataIndex += col.Count * HeapSizeOf.float
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Single))
        For Each val As Single In col
            Dim asLong = UncheckHelpers.SingleToInt32Bits(val)
            DataStream.Write(asLong)
            DataIndex += HeapSizeOf.float
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Single?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(SingleToInt32Bits(If(val, Single.NaN)))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.float

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Single?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Single? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(UncheckHelpers.SingleToInt32Bits(If(val, Single.NaN)))
        Next

        DataIndex += col.Count * HeapSizeOf.float

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Single?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Single? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(UncheckHelpers.SingleToInt32Bits(If(val, Single.NaN)))
            DataIndex += HeapSizeOf.float
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Long())
        For Each val As Long In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.long
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Long))
        For Each val As Long In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Long))
        For Each val As Long In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.long
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Long?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Long.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.long

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Long?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Long? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Long.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.long

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Long?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Long? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, Long.MaxValue))
            DataIndex += HeapSizeOf.long
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As ULong())
        For Each val As ULong In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.ulong
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of ULong))
        For Each val As ULong In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.ulong
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of ULong))
        For Each val As ULong In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.ulong
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As ULong?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, ULong.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.ulong

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of ULong?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As ULong? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, ULong.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.ulong

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of ULong?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As ULong? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, ULong.MaxValue))
            DataIndex += HeapSizeOf.ulong
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Integer())
        For Each val As Integer In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.int
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Integer))
        For Each val As Integer In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.int
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Integer))
        For Each val As Integer In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.int
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Integer?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Integer.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Integer?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Integer? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Integer.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Integer?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Integer? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, Integer.MaxValue))
            DataIndex += HeapSizeOf.int
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As UInteger())
        For Each val As UInteger In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.uint
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of UInteger))
        For Each val As UInteger In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.uint
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of UInteger))
        For Each val As UInteger In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.uint
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As UInteger?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, UInteger.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.uint

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of UInteger?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As UInteger? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, UInteger.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.uint

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of UInteger?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As UInteger? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, UInteger.MaxValue))
            DataIndex += HeapSizeOf.uint
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Short())
        For Each val As Short In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.short
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Short))
        For Each val As Short In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.short
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Short))
        For Each val As Short In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.short
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Short?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Short.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.short

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Short?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Short? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Short.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.short

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Short?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Short? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, Short.MaxValue))
            DataIndex += HeapSizeOf.short
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As UShort())
        For Each val As UShort In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.ushort
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of UShort))
        For Each val As UShort In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.ushort
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of UShort))
        For Each val As UShort In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.ushort
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As UShort?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, UShort.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.ushort

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of UShort?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As UShort? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, UShort.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.ushort

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of UShort?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As UShort? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, UShort.MaxValue))
            DataIndex += HeapSizeOf.ushort
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As Byte())
        DataStream.Write(arr, 0, arr.Length)
        DataIndex += arr.Length * HeapSizeOf.byte
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of Byte))
        For Each val As Byte In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.byte
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of Byte))
        For Each val As Byte In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.byte
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As Byte?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Byte.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.byte

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of Byte?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Byte? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, Byte.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.byte

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of Byte?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As Byte? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, Byte.MaxValue))
            DataIndex += HeapSizeOf.byte
        Next


        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableArray(arr As SByte())
        For Each val As SByte In arr
            DataStream.Write(val)
        Next

        DataIndex += arr.Length * HeapSizeOf.sbyte
    End Sub

    Friend Sub CopyNonNullableCollection(col As ICollection(Of SByte))
        For Each val As SByte In col
            DataStream.Write(val)
        Next

        DataIndex += col.Count * HeapSizeOf.sbyte
    End Sub

    Friend Sub CopyNonNullableIEnumerable(col As IEnumerable(Of SByte))
        For Each val As SByte In col
            DataStream.Write(val)
            DataIndex += HeapSizeOf.sbyte
        Next
    End Sub

    Friend Sub BlitNullableArray(arr As SByte?())
        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To arr.LongLength - 1
            Dim val = arr(i)
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, SByte.MaxValue))
        Next

        DataIndex += arr.LongLength * HeapSizeOf.sbyte

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += arr.Length / 8
        If arr.Length Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableCollection(col As ICollection(Of SByte?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As SByte? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If

            DataStream.Write(If(val, SByte.MaxValue))
        Next

        DataIndex += col.Count * HeapSizeOf.sbyte

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableIEnumerable(col As IEnumerable(Of SByte?))
        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As SByte? In col
            Dim b = val IsNot Nothing

            If b Then
                currentByte = currentByte Or CByte(1 << ix)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += 1
            End If

            DataStream.Write(If(val, SByte.MaxValue))
            DataIndex += HeapSizeOf.sbyte
        Next

        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyStringArray(col As String())
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        For i = 0 To col.Length - 1
            Dim str = col(i)
            Dim bytes = utf8.GetBytes(str)
            VariableStream.Write(bytes)
            VariableIndex += bytes.Length

            DataStream.Write(offset)

            offset += bytes.Length
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += (col.LongLength + 1) * HeapSizeOf.int
    End Sub

    Friend Sub CopyStringCollection(col As ICollection(Of String))
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        For Each str As String In col
            Dim bytes = utf8.GetBytes(str)
            VariableStream.Write(bytes)
            VariableIndex += bytes.Length

            DataStream.Write(offset)

            offset += bytes.Length
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += (col.Count + 1) * HeapSizeOf.int
    End Sub

    Friend Sub CopyStringIEnumerable(col As IEnumerable(Of String))
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        For Each str As String In col
            Dim bytes = utf8.GetBytes(str)
            VariableStream.Write(bytes)
            VariableIndex += bytes.Length

            DataStream.Write(offset)
            DataIndex += HeapSizeOf.int

            offset += bytes.Length
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += HeapSizeOf.int
    End Sub

    Friend Sub CopyNullableStringArray(col As String())
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For i = 0 To col.Length - 1
            Dim writtenVariableBytes = 0
            Dim str = col(i)
            If Not Equals(str, Nothing) Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim bytes = utf8.GetBytes(str)
                VariableStream.Write(bytes)
                VariableIndex += bytes.Length
                writtenVariableBytes = bytes.Length
            End If
            DataStream.Write(offset)

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If

            offset += writtenVariableBytes
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += (col.LongLength + 1) * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableStringCollection(col As ICollection(Of String))
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each str As String In col
            Dim writtenVariableBytes = 0
            If Not Equals(str, Nothing) Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim bytes = utf8.GetBytes(str)
                VariableStream.Write(bytes)
                VariableIndex += bytes.Length
                writtenVariableBytes = bytes.Length
            End If
            DataStream.Write(offset)

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If

            offset += writtenVariableBytes
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += (col.Count + 1) * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableStringIEnumerable(col As IEnumerable(Of String))
        Dim utf8 = Encoding.UTF8
        Dim offset = 0

        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each str As String In col
            Dim writtenVariableBytes = 0
            If Not Equals(str, Nothing) Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim bytes = utf8.GetBytes(str)
                VariableStream.Write(bytes)
                VariableIndex += bytes.Length
                writtenVariableBytes = bytes.Length
            End If
            DataStream.Write(offset)
            DataIndex += HeapSizeOf.int

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If

            offset += writtenVariableBytes
        Next

        ' write the last index out
        DataStream.Write(offset)
        DataIndex += HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Const SECONDS_PER_TICK As Double = 0.0000001
    Const MILLISECONDS_PER_TICK As Double = 0.0001
    Const MICROSECONDS_PER_TICK As Double = 0.1
    Const NANOSECONDS_PER_TICK As Double = 100
    Private Shared Function MapToDiskType(elapsedTicks As Long) As Long
        Return std.Round(MICROSECONDS_PER_TICK * elapsedTicks)
    End Function

    Friend Sub BlitDateTimeArray(col As Date())
        For i = 0 To col.Length - 1
            Dim dt = col(i)
            If dt.Kind <> DateTimeKind.Utc Then
                dt = dt.ToUniversalTime()
            End If

            Dim timeSinceEpoch = dt - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
        Next

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub BlitDateTimeOffsetArray(col As DateTimeOffset())
        For i = 0 To col.Length - 1
            Dim dt = col(i).UtcDateTime

            Dim timeSinceEpoch = dt - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
        Next

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub CopyDateTimeCollection(col As ICollection(Of Date))
        For Each dt In col
            Dim dtCopy = dt
            If dtCopy.Kind <> DateTimeKind.Utc Then
                dtCopy = dtCopy.ToUniversalTime()
            End If

            Dim timeSinceEpoch = dtCopy - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
        Next

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyDateTimeOffsetCollection(col As ICollection(Of DateTimeOffset))
        For Each dt In col
            Dim dtCopy = dt.UtcDateTime

            Dim timeSinceEpoch = dtCopy - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
        Next

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyDateTimeIEnumerable(col As IEnumerable(Of Date))
        For Each dt In col
            Dim dtCopy = dt
            If dtCopy.Kind <> DateTimeKind.Utc Then
                dtCopy = dtCopy.ToUniversalTime()
            End If

            Dim timeSinceEpoch = dtCopy - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
            DataIndex += HeapSizeOf.long
        Next
    End Sub

    Friend Sub CopyDateTimeOffsetIEnumerable(col As IEnumerable(Of DateTimeOffset))
        For Each dt In col
            Dim dtCopy = dt.UtcDateTime

            Dim timeSinceEpoch = dtCopy - DATETIME_EPOCH
            Dim elapsedTicks = timeSinceEpoch.Ticks

            Dim value = MapToDiskType(elapsedTicks)
            DataStream.Write(value)
            DataIndex += HeapSizeOf.long
        Next
    End Sub

    Friend Sub BlitNullableDateTimeArray(col As Date?())
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For i = 0 To col.Length - 1
            Dim dt = col(i)
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value

                If dtValue.Kind <> DateTimeKind.Utc Then
                    dtValue = dtValue.ToUniversalTime()
                End If

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub BlitNullableDateTimeOffsetArray(col As DateTimeOffset?())
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For i = 0 To col.Length - 1
            Dim dt = col(i)
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value.UtcDateTime

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub CopyNullableDateTimeCollection(col As ICollection(Of Date?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each dt In col
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value

                If dtValue.Kind <> DateTimeKind.Utc Then
                    dtValue = dtValue.ToUniversalTime()
                End If

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyNullableDateTimeOffsetCollection(col As ICollection(Of DateTimeOffset?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each dt In col
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value.UtcDateTime

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyNullableDateTimeIEnumerable(col As IEnumerable(Of Date?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each dt In col
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value

                If dtValue.Kind <> DateTimeKind.Utc Then
                    dtValue = dtValue.ToUniversalTime()
                End If

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            DataIndex += HeapSizeOf.long

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableDateTimeOffsetIEnumerable(col As IEnumerable(Of DateTimeOffset?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each dt In col
            If dt IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim dtValue = dt.Value.UtcDateTime

                Dim timeSinceEpoch = dtValue - DATETIME_EPOCH
                Dim elapsedTicks = timeSinceEpoch.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            DataIndex += HeapSizeOf.long

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitTimeSpanArray(col As TimeSpan())
        For i = 0 To col.Length - 1
            Dim ts = col(i)
            Dim ticks = ts.Ticks

            Dim value = MapToDiskType(ticks)
            DataStream.Write(value)
        Next

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub CopyTimeSpanCollection(col As ICollection(Of TimeSpan))
        For Each ts In col
            Dim ticks = ts.Ticks

            Dim value = MapToDiskType(ticks)
            DataStream.Write(value)
        Next

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyTimeSpanIEnumerable(col As IEnumerable(Of TimeSpan))
        For Each ts In col
            Dim ticks = ts.Ticks

            Dim value = MapToDiskType(ticks)
            DataStream.Write(value)
            DataIndex += HeapSizeOf.long
        Next
    End Sub

    Friend Sub BlitNullableTimeSpanArray(col As TimeSpan?())
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For i = 0 To col.Length - 1
            Dim ts = col(i)
            If ts IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim elapsedTicks = ts.Value.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.LongLength * HeapSizeOf.long
    End Sub

    Friend Sub CopyNullableTimeSpanCollection(col As ICollection(Of TimeSpan?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each ts In col
            If ts IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim elapsedTicks = ts.Value.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
            Else
                DataStream.Write(Long.MaxValue)
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If

        DataIndex += col.Count * HeapSizeOf.long
    End Sub

    Friend Sub CopyNullableTimeSpanIEnumerable(col As IEnumerable(Of TimeSpan?))
        Dim currentNullByte As Byte = 0
        Dim ix = 0

        For Each ts In col
            If ts IsNot Nothing Then
                currentNullByte = currentNullByte Or CByte(1 << ix)

                Dim elapsedTicks = ts.Value.Ticks

                Dim value = MapToDiskType(elapsedTicks)
                DataStream.Write(value)
                DataIndex += HeapSizeOf.long
            Else
                DataStream.Write(Long.MaxValue)
                DataIndex += HeapSizeOf.long
            End If

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentNullByte)
                ix = 0
                currentNullByte = 0
                NullIndex += 1
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentNullByte)
            NullIndex += 1
        End If
    End Sub

    Friend Sub BlitNonNullableEnumArray(Of TEnum As Structure)(col As TEnum())
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        For i = 0 To col.LongLength - 1
            Dim val = col(i)
            Dim asLong = convert(val)

            Dim categoryIndex As Integer
            If Not map.TryGetValue(asLong, categoryIndex) Then
                Throw New InvalidOperationException($"Found undefined value {val} for enum {GetType(TEnum).Name}")
            End If

            DataStream.Write(categoryIndex)
        Next

        DataIndex += col.LongLength * HeapSizeOf.int
    End Sub

    Friend Sub CopyNonNullableEnumCollection(Of TEnum As Structure)(col As ICollection(Of TEnum))
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        For Each val As TEnum In col
            Dim asLong = convert(val)

            Dim categoryIndex As Integer
            If Not map.TryGetValue(asLong, categoryIndex) Then
                Throw New InvalidOperationException($"Found undefined value {val} for enum {GetType(TEnum).Name}")
            End If

            DataStream.Write(categoryIndex)
        Next

        DataIndex += col.Count * HeapSizeOf.int
    End Sub

    Friend Sub CopyNonNullableEnumIEnumerable(Of TEnum As Structure)(col As IEnumerable(Of TEnum))
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        For Each val As TEnum In col
            Dim asLong = convert(val)

            Dim categoryIndex As Integer
            If Not map.TryGetValue(asLong, categoryIndex) Then
                Throw New InvalidOperationException($"Found undefined value {val} for enum {GetType(TEnum).Name}")
            End If

            DataStream.Write(categoryIndex)
            DataIndex += HeapSizeOf.int
        Next
    End Sub

    Friend Sub BlitNullableEnumArray(Of TEnum As Structure)(col As TEnum?())
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        Dim currentByte As Byte = 0
        Dim ix = 0
        For i = 0L To col.LongLength - 1
            Dim val = col(i)
            Dim b = val IsNot Nothing

            Dim asInt As Integer

            If b Then
                currentByte = currentByte Or CByte(1 << ix)

                Dim asLong = convert(val.Value)
                If Not map.TryGetValue(asLong, asInt) Then
                    Throw New InvalidOperationException($"Found undefined value {val.Value} for enum {GetType(TEnum).Name}")
                End If
            Else
                asInt = Integer.MaxValue
            End If

            DataStream.Write(asInt)

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If
        Next

        DataIndex += col.LongLength * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.LongLength / 8
        If col.LongLength Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableEnumCollection(Of TEnum As Structure)(col As ICollection(Of TEnum?))
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As TEnum? In col
            Dim b = val IsNot Nothing

            Dim asInt As Integer

            If b Then
                currentByte = currentByte Or CByte(1 << ix)

                Dim asLong = convert(val.Value)
                If Not map.TryGetValue(asLong, asInt) Then
                    Throw New InvalidOperationException($"Found undefined value {val.Value} for enum {GetType(TEnum).Name}")
                End If
            Else
                asInt = Integer.MaxValue
            End If

            DataStream.Write(asInt)

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
            End If
        Next

        DataIndex += col.Count * HeapSizeOf.int

        If ix <> 0 Then
            NullStream.Write(currentByte)
        End If

        NullIndex += col.Count / 8
        If col.Count Mod 8 <> 0 Then
            NullIndex += 1
        End If
    End Sub

    Friend Sub CopyNullableEnumIEnumerable(Of TEnum As Structure)(col As IEnumerable(Of TEnum?))
        Dim map = GetLevelIndexLookup(GetType(TEnum))
        Dim convert = GetConvertToLongDelegate(Of TEnum)()

        Dim currentByte As Byte = 0
        Dim ix = 0
        For Each val As TEnum? In col
            Dim b = val IsNot Nothing

            Dim asInt As Integer

            If b Then
                currentByte = currentByte Or CByte(1 << ix)

                Dim asLong = convert(val.Value)
                If Not map.TryGetValue(asLong, asInt) Then
                    Throw New InvalidOperationException($"Found undefined value {val.Value} for enum {GetType(TEnum).Name}")
                End If
            Else
                asInt = Integer.MaxValue
            End If

            DataStream.Write(asInt)
            DataIndex += HeapSizeOf.int

            ix += 1
            If ix = 8 Then
                NullStream.Write(currentByte)
                ix = 0
                currentByte = 0
                NullIndex += HeapSizeOf.byte
            End If
        Next

        If ix <> 0 Then
            NullStream.Write(currentByte)
            NullIndex += 1
        End If
    End Sub
End Class
