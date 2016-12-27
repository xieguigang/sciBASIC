# PrimitiveAPI
_namespace: [Microsoft.VisualBasic.Mathematical.SyntaxAPI](./index.md)_

R function bridge to VisualBasic



### Methods

#### All
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.All(Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.BooleanVector,System.Boolean)
```
Given a set of logical vectors, are all of the values true?

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Any
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Any(Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.BooleanVector,System.Boolean)
```
Given a set of logical vectors, is at least one of the values true?

|Parameter Name|Remarks|
|--------------|-------|
|x|zero or more logical vectors. Other objects of zero length are ignored, and the rest are coerced to logical ignoring any class.|
|NaRM|-|


_returns: The value is a logical vector of length one._

#### IfElse``1
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.IfElse``1(Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.BooleanVector,Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.GenericVector{``0},Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.GenericVector{``0})
```
Conditional Element Selection
 ifelse returns a value with the same shape as test which is filled with elements selected from either yes or no depending on whether the element of test is TRUE or FALSE.

|Parameter Name|Remarks|
|--------------|-------|
|test|an object which can be coerced to logical mode.|
|yes|return values for true elements of test.|
|no|return values for false elements of test.|


_returns: A vector of the same length and attributes (including dimensions and "class") as test and data values from the values of yes or no. The mode of the answer
 will be coerced from logical to accommodate first any values taken from yes and then any values taken from no._
> If yes or no are too short, their elements are recycled. yes will be evaluated if and only if any element of test is true, and analogously for no.
>  Missing values in test give missing values in the result.

#### Length``1
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Length``1(Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.GenericVector{``0})
```
Get or set the length of vectors (including lists) and factors, and of any other R object for which a method has been defined.

|Parameter Name|Remarks|
|--------------|-------|
|x|an R object. For replacement, a vector or factor.|


#### Missing``1
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Missing``1(``0)
```
Does a Formal Argument have a Value?
 missing can be used to test whether a value was specified as an argument to a function.

|Parameter Name|Remarks|
|--------------|-------|
|x|a formal argument.|

> 
>  Missing(x) is only reliable if x has not been altered since entering the function: in particular it will always be false after x <- match.arg(x).
>  The example shows how a plotting function can be written to work with either a pair of vectors giving x and y coordinates of points to be plotted or a single vector giving y values to be plotted against their indices.
>  Currently missing can only be used in the immediate body of the function that defines the argument, not in the body of a nested function or a local call. This may change in the future.
>  This is a ‘special’ primitive function: it must not evaluate its argument.
>  

#### Paste
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Paste(System.Collections.Generic.IEnumerable{System.String},System.String)
```
Concatenate Strings, Concatenate vectors after converting to character.

|Parameter Name|Remarks|
|--------------|-------|
|args|-|
|sep|a character string to separate the terms. Not NA_character_.|


_returns: 
 A character vector of the concatenated values. This will be of length zero if all the objects are, unless collapse is non-NULL in which case it is a single empty string.
 If any input into an element of the result is in UTF-8 (and none are declared with encoding "bytes", (see Encoding), that element will be in UTF-8, otherwise in the
 current encoding in which case the encoding of the element is declared if the current locale is either Latin-1 or UTF-8, at least one of the corresponding inputs
 (including separators) had a declared encoding and all inputs were either ASCII or declared.
 If an input into an element is declared with encoding "bytes", no translation will be done of any of the elements and the resulting element will have encoding "bytes".
 If collapse is non-NULL, this applies also to the second, collapsing, phase, but some translation may have been done in pasting object together in the first phase.
 _

#### Rep
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Rep(Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors.BooleanVector,System.Int32)
```
Replicate Elements of Vectors and Lists

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|LengthOut|non-negative integer: the desired length of the output vector.|


#### Try``1
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.Try``1(System.Func{``0},System.Boolean)
```
Try an Expression Allowing Error Recovery
 try is a wrapper to run an expression that might fail and allow the user's code to handle error-recovery.

|Parameter Name|Remarks|
|--------------|-------|
|silent|logical: should the report of error messages be suppressed?|


#### warning
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.PrimitiveAPI.warning(System.Collections.Generic.IEnumerable{System.Object},System.Boolean,System.Boolean,System.Boolean,System.Object)
```
Generates a warning message that corresponds to its argument(s) and (optionally) the expression or function from which it was called.

|Parameter Name|Remarks|
|--------------|-------|
|args|zero or more objects which can be coerced to character (and which are pasted together with no separator) or a single condition object.|
|Call|logical, indicating if the call should become part of the warning message.|
|immediate|logical, indicating if the call should be output immediately, even if getOption("warn") <= 0.|
|noBreaks|-|
|domain|-|


_returns: The warning message as character string, invisibly._
> 
>  The result depends on the value of options("warn") and on handlers established in the executing code.
>  If a condition object is supplied it should be the only argument, and further arguments will be ignored, with a message.
>  warning signals a warning condition by (effectively) calling signalCondition. If there are no handlers or if all handlers return,
>  then the value of warn = getOption("warn") is used to determine the appropriate action. If warn is negative warnings are ignored;
>  if it is zero they are stored and printed after the top–level function has completed; if it is one they are printed as they occur
>  and if it is 2 (or larger) warnings are turned into errors. Calling warning(immediate. = TRUE) turns warn <= 0 into warn = 1 for this call only.
>  If warn is zero (the default), a read-only variable last.warning is created. It contains the warnings which can be printed via a call to warnings.
>  Warnings will be truncated to getOption("warning.length") characters, default 1000, indicated by [... truncated].
>  While the warning is being processed, a muffleWarning restart is available. If this restart is invoked with invokeRestart, then warning returns immediately.
>  An attempt is made to coerce other types of inputs to warning to character vectors.
>  suppressWarnings evaluates its expression in a context that ignores all warnings.
>  


