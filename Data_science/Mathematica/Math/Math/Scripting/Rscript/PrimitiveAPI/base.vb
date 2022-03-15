#Region "Microsoft.VisualBasic::b3d7bbafa268360066052b073b2f6ed7, sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Rscript\PrimitiveAPI\base.vb"

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

    '   Total Lines: 324
    '    Code Lines: 158
    ' Comment Lines: 119
    '   Blank Lines: 47
    '     File Size: 16.22 KB


    '     Module base
    ' 
    '         Function: [stop], [Try], All, Any, c
    '                   (+3 Overloads) C, getOption, IfElse, (+3 Overloads) Length, Missing
    '                   ncol, nrow, (+2 Overloads) Paste, (+2 Overloads) Rep, (+2 Overloads) warning
    '                   ZeroVector
    ' 
    '         Sub: [Try]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Scripting.Rscript

    ''' <summary>
    ''' R function bridge to VisualBasic
    ''' </summary>
    ''' <remarks></remarks>
    Public Module base

        ''' <summary>
        ''' Does a Formal Argument have a Value?
        ''' missing can be used to test whether a value was specified as an argument to a function.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">a formal argument.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Missing(x) is only reliable if x has not been altered since entering the function: in particular it will always be false after x &lt;- match.arg(x).
        ''' The example shows how a plotting function can be written to work with either a pair of vectors giving x and y coordinates of points to be plotted or a single vector giving y values to be plotted against their indices.
        ''' Currently missing can only be used in the immediate body of the function that defines the argument, not in the body of a nested function or a local call. This may change in the future.
        ''' This is a ‘special’ primitive function: it must not evaluate its argument.
        ''' </remarks>
        Public Function Missing(Of T)(x As T) As Boolean
            Return x Is Nothing
        End Function

        <ExportAPI("stop")>
        Public Function [stop](ex As String)
            Throw New Exception(ex)
        End Function

        Public Const NULL As Object = Nothing
        Public Const NAN As Double = Double.NaN

        ''' <summary>
        ''' Generates a warning message that corresponds to its argument(s) and (optionally) the expression or function from which it was called.
        ''' </summary>
        ''' <param name="args">zero or more objects which can be coerced to character (and which are pasted together with no separator) or a single condition object.</param>
        ''' <param name="Call">logical, indicating if the call should become part of the warning message.</param>
        ''' <param name="immediate">logical, indicating if the call should be output immediately, even if getOption("warn") &lt;= 0.</param>
        ''' <param name="noBreaks"></param>
        ''' <param name="domain"></param>
        ''' <returns>The warning message as character string, invisibly.</returns>
        ''' <remarks>
        ''' The result depends on the value of options("warn") and on handlers established in the executing code.
        ''' If a condition object is supplied it should be the only argument, and further arguments will be ignored, with a message.
        ''' warning signals a warning condition by (effectively) calling signalCondition. If there are no handlers or if all handlers return,
        ''' then the value of warn = getOption("warn") is used to determine the appropriate action. If warn is negative warnings are ignored;
        ''' if it is zero they are stored and printed after the top–level function has completed; if it is one they are printed as they occur
        ''' and if it is 2 (or larger) warnings are turned into errors. Calling warning(immediate. = TRUE) turns warn &lt;= 0 into warn = 1 for this call only.
        ''' If warn is zero (the default), a read-only variable last.warning is created. It contains the warnings which can be printed via a call to warnings.
        ''' Warnings will be truncated to getOption("warning.length") characters, default 1000, indicated by [... truncated].
        ''' While the warning is being processed, a muffleWarning restart is available. If this restart is invoked with invokeRestart, then warning returns immediately.
        ''' An attempt is made to coerce other types of inputs to warning to character vectors.
        ''' suppressWarnings evaluates its expression in a context that ignores all warnings.
        ''' </remarks>
        '''
        <ExportAPI("Warning")>
        Public Function warning(args As IEnumerable(Of Object),
                                <Parameter("call.", "logical, indicating if the call should become part of the warning message.")> Optional [Call] As Boolean = True,
                                <Parameter("immediate.", "logical, indicating if the call should be output immediately, even if getOption(""warn"") <= 0.")> Optional immediate As Boolean = False,
                                <Parameter("noBreaks.", "logical, indicating as far as possible the message should be output as a single line when options(warn = 1).")> Optional noBreaks As Boolean = False,
                                <Parameter("domain", "see gettext. If NA, messages will not be translated, see also the note in stop.")> Optional domain As Object = NULL) As String
            Throw New NotImplementedException
        End Function

        Public Const NA As Object = Nothing

        <ExportAPI("warning")>
        Public Function warning(args As String, Optional Domain As Object = NA) As String
            Throw New NotImplementedException
        End Function

        Public Function C(Of T)(ParamArray argvs As GenericVector(Of T)()) As GenericVector(Of T)
            Dim ChunkBuffer = argvs(0).AsList
            For Each Vector As GenericVector(Of T) In argvs.Skip(1)
                Call ChunkBuffer.AddRange(Vector)
            Next
            Return New GenericVector(Of T)(ChunkBuffer)
        End Function

        <ExportAPI("c")>
        Public Function C(ParamArray argvs As Vector()) As Vector
            Dim vec As New List(Of Double)(argvs(0))
            For Each x As Vector In argvs.Skip(1)
                Call vec.AddRange(x)
            Next
            Return New Vector(vec.ToArray)
        End Function

        <ExportAPI("c")>
        Public Function C(ParamArray args As Double()) As Vector
            Return New Vector(args)
        End Function

        Public Function c(ParamArray args As Object()) As Vector
            Dim output As New List(Of Double)

            For Each item In args
                If TypeOf item Is Double Then
                    output.Add(item)
                ElseIf TypeOf item Is Vector Then
                    output.AddRange(DirectCast(item, Vector))
                Else
                    Throw New NotImplementedException(item.GetType.FullName)
                End If
            Next

            Return New Vector(output)
        End Function

        ''' <summary>
        ''' Given a set of logical vectors, is at least one of the values true?
        ''' </summary>
        ''' <param name="x">zero or more logical vectors. Other objects of zero length are ignored, and the rest are coerced to logical ignoring any class.</param>
        ''' <param name="NaRM"></param>
        ''' <returns>The value is a logical vector of length one.</returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Any")>
        Public Function Any(x As BooleanVector,
                            <MetaData.Parameter("na.rm", "logical. If true NA values are removed before the result is computed.")>
                            Optional NaRM As Boolean = False) As BooleanVector

            If NaRM Then

            End If

            For Each b As Boolean In x
                If b = True Then
                    Return New BooleanVector({True})
                End If
            Next

            Return New BooleanVector({False})
        End Function

        ''' <summary>
        ''' Given a set of logical vectors, are all of the values true?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("All")>
        Public Function All(x As BooleanVector,
                            <MetaData.Parameter("na.rm", "logical. If true NA values are removed before the result is computed.")>
                            Optional NaRM As Boolean = False) As BooleanVector
            If NaRM Then

            End If

            For Each b As Boolean In x
                If b = False Then
                    Return New BooleanVector({False})
                End If
            Next

            Return New BooleanVector({True})
        End Function

        ''' <summary>
        ''' Get or set the length of vectors (including lists) and factors, and of any other R object for which a method has been defined.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">an R object. For replacement, a vector or factor.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Length(Of T)(x As GenericVector(Of T), Optional value As Integer = 0) As Vector
            If value > 0 Then
                Call x.Factor(value)
            End If

            Return New Vector(integers:={x.Dim})
        End Function

        ''' <summary>
        ''' Get or set the length of vectors (including lists) and factors, and of any other R object for which a method has been defined.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">an R object. For replacement, a vector or factor.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Length(Of T)(x As GenericVector(Of T)) As Vector
            Return New Vector(integers:={x.Dim})
        End Function

        ''' <summary>
        ''' Replicate Elements of Vectors and Lists
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="LengthOut">non-negative integer: the desired length of the output vector.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Rep")>
        Public Function Rep(x As Vector,
                            <Parameter("length.out", "non-negative integer: the desired length of the output vector.")>
                            Optional LengthOut As Integer = 0) As Vector

            Dim data As Double() = x.ToArray

            If LengthOut > 0 Then
                ReDim Preserve data(LengthOut - 1)
            End If

            Return New Vector(data)
        End Function

        ''' <summary>
        ''' Replicate Elements of Vectors and Lists
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="LengthOut">non-negative integer: the desired length of the output vector.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Rep")>
        Public Function Rep(x As BooleanVector,
                        <MetaData.Parameter("length.out", "non-negative integer: the desired length of the output vector.")> Optional LengthOut As Integer = 0) As BooleanVector

            Dim data As Boolean() = x.ToArray

            If LengthOut > 0 Then
                ReDim Preserve data(LengthOut - 1)
            End If

            Return New BooleanVector(data)
        End Function

        ''' <summary>
        ''' Concatenate Strings, Concatenate vectors after converting to character.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="sep">a character string to separate the terms. Not NA_character_.</param>
        ''' <returns>
        ''' A character vector of the concatenated values. This will be of length zero if all the objects are, unless collapse is non-NULL in which case it is a single empty string.
        ''' If any input into an element of the result is in UTF-8 (and none are declared with encoding "bytes", (see Encoding), that element will be in UTF-8, otherwise in the
        ''' current encoding in which case the encoding of the element is declared if the current locale is either Latin-1 or UTF-8, at least one of the corresponding inputs
        ''' (including separators) had a declared encoding and all inputs were either ASCII or declared.
        ''' If an input into an element is declared with encoding "bytes", no translation will be done of any of the elements and the resulting element will have encoding "bytes".
        ''' If collapse is non-NULL, this applies also to the second, collapsing, phase, but some translation may have been done in pasting object together in the first phase.
        ''' </returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Paste")>
        Public Function Paste(args As IEnumerable(Of String), Optional sep As String = " ") As String
            Return String.Join(sep, args)
        End Function

        <ExportAPI("Paste")>
        Public Function Paste(ParamArray args As Object()) As String
            Return String.Join("", (From obj In args Select str = obj.ToString).ToArray)
        End Function

        Public Function Length(Of T)(obj As IEnumerable(Of T)) As Integer
            If obj Is Nothing Then
                Return 0
            Else
                Return obj.Count
            End If
        End Function

        ''' <summary>
        ''' Conditional Element Selection
        ''' ifelse returns a value with the same shape as test which is filled with elements selected from either yes or no depending on whether the element of test is TRUE or FALSE.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="test">an object which can be coerced to logical mode.</param>
        ''' <param name="yes">return values for true elements of test.</param>
        ''' <param name="no">return values for false elements of test.</param>
        ''' <returns>A vector of the same length and attributes (including dimensions and "class") as test and data values from the values of yes or no. The mode of the answer
        ''' will be coerced from logical to accommodate first any values taken from yes and then any values taken from no.</returns>
        ''' <remarks>If yes or no are too short, their elements are recycled. yes will be evaluated if and only if any element of test is true, and analogously for no.
        ''' Missing values in test give missing values in the result.</remarks>
        Public Function IfElse(Of T)(test As BooleanVector, yes As GenericVector(Of T), no As GenericVector(Of T)) As T()
            Dim LQuery = (From i As Integer In test.Sequence Select If(test(i), yes(i), no(i))).ToArray
            Return LQuery
        End Function

        <ExportAPI("GetOption")>
        Public Function getOption(x As String, Optional [default] As Boolean = NULL) As Boolean
            Throw New NotImplementedException
        End Function

        Public Const T As Boolean = True
        Public Const F As Boolean = False

        <ExportAPI("Vector")>
        Public Function ZeroVector() As Vector
            Return New Vector(0)
        End Function

        ''' <summary>
        ''' Try an Expression Allowing Error Recovery
        ''' try is a wrapper to run an expression that might fail and allow the user's code to handle error-recovery.
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="silent">logical: should the report of error messages be suppressed?</param>
        ''' <remarks></remarks>
        Public Function [Try](Of T)(expr As Func(Of T), Optional silent As Boolean = False) As T

        End Function

        <ExportAPI("Try")>
        Public Sub [Try](expr As Action, Optional silent As Boolean = False)

        End Sub

        Public Function nrow(x As GeneralMatrix) As Integer
            Return x.RowDimension
        End Function

        Public Function ncol(x As GeneralMatrix) As Integer
            Return x.ColumnDimension
        End Function
    End Module
End Namespace
