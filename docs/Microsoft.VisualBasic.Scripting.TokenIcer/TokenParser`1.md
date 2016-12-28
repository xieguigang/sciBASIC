# TokenParser`1
_namespace: [Microsoft.VisualBasic.Scripting.TokenIcer](./index.md)_

TokenParser

> 
>  TokenParser is the main parser engine for converting input into lexical tokens.
>  
>  Auto Generated from
>  http://www.codeproject.com/Articles/220042/Easily-Create-Your-Own-Parser
>  


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser`1.#ctor(System.Collections.Generic.Dictionary{`0,System.String},`0)
```
Default Constructor

|Parameter Name|Remarks|
|--------------|-------|
|tokens|Values is the regex expression|

> 
>  The constructor initalizes memory and adds all of the tokens to the token dictionary.
>  

#### GetToken
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser`1.GetToken
```
GetToken gets the next token in queue
> 
>  GetToken attempts to the match the next character(s) using the
>  Regex rules defined in the dictionary. If a match can not be
>  located, then an Undefined token will be created with an empty
>  string value. In addition, the token pointer will be incremented
>  by one so that this token doesn't attempt to get identified again by
>  GetToken()
>  

#### Peek
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser`1.Peek(Microsoft.VisualBasic.Scripting.TokenIcer.PeekToken{`0})
```
Returns the next token after the Token passed here

|Parameter Name|Remarks|
|--------------|-------|
|peekToken|The PeekToken token returned from a previous Peek() call|


#### PrepareRegex
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser`1.PrepareRegex
```
PrepareRegex prepares the regex for parsing by pre-matching the Regex tokens.

#### ResetParser
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser`1.ResetParser
```
ResetParser resets the parser to its inital state. Reloading InputString is required.


### Properties

#### InputString
InputString Property
