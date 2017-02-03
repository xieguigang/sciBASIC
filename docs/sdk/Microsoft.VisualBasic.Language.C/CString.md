# CString
_namespace: [Microsoft.VisualBasic.Language.C](./index.md)_

This class provides the ability to simulate various classic C string functions
which don't have exact equivalents in the .NET Framework.



### Methods

#### ChangeCharacter
```csharp
Microsoft.VisualBasic.Language.C.CString.ChangeCharacter(System.String,System.Int32,System.Char)
```
This method allows replacing a single character in a string, to help convert
 C++ code where a single character in a character array is replaced.

|Parameter Name|Remarks|
|--------------|-------|
|sourcestring|-|
|charindex|-|
|changechar|-|


#### IsXDigit
```csharp
Microsoft.VisualBasic.Language.C.CString.IsXDigit(System.Char)
```
This method simulates the classic C string function 'isxdigit' (and 'iswxdigit').

|Parameter Name|Remarks|
|--------------|-------|
|character|-|


#### StrChr
```csharp
Microsoft.VisualBasic.Language.C.CString.StrChr(System.String,System.Char)
```
This method simulates the classic C string function 'strchr' (and 'wcschr').

|Parameter Name|Remarks|
|--------------|-------|
|stringtosearch|-|
|chartofind|-|


#### StrRChr
```csharp
Microsoft.VisualBasic.Language.C.CString.StrRChr(System.String,System.Char)
```
This method simulates the classic C string function 'strrchr' (and 'wcsrchr').

|Parameter Name|Remarks|
|--------------|-------|
|stringtosearch|-|
|chartofind|-|


#### StrStr
```csharp
Microsoft.VisualBasic.Language.C.CString.StrStr(System.String,System.String)
```
This method simulates the classic C string function 'strstr' (and 'wcsstr').

|Parameter Name|Remarks|
|--------------|-------|
|stringtosearch|-|
|stringtofind|-|


#### StrTok
```csharp
Microsoft.VisualBasic.Language.C.CString.StrTok(System.String,System.String)
```
This method simulates the classic C string function 'strtok' (and 'wcstok').
 Note that the .NET string 'Split' method cannot be used to simulate 'strtok' since
 it doesn't allow changing the delimiters between each token retrieval.

|Parameter Name|Remarks|
|--------------|-------|
|stringtotokenize|-|
|delimiters|-|



