# Spinner
_namespace: [Microsoft.VisualBasic.Terminal.xConsole](./index.md)_

A list of spinners for your console ❤



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Spinner.#ctor(System.Char[])
```
Initialize a custom spinner

|Parameter Name|Remarks|
|--------------|-------|
|spinner|Set a custom spinner, no size limit.|


#### Break
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Spinner.Break
```
Breaks the spinner

#### Turn
```csharp
Microsoft.VisualBasic.Terminal.xConsole.Spinner.Turn(System.Int32)
```
Turn the spin!

|Parameter Name|Remarks|
|--------------|-------|
|time|Waiting time. Default 130 ms|


_returns: False if it has been stopped_


### Properties

#### inLoop
looplooplooplooplooplooplooplooplooploop[...]
#### Spinners
List of available spinners (you can add new)
#### SpinText
The base string for spinning. {0} will display the spinner. COLOR is SUPPORTED! 🆒
