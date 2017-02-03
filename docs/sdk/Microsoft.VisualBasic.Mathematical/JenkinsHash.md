# JenkinsHash
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

Original source from http://256.com/sources/jenkins_hash_java/
 
 This is a Bob Jenkins hashing algorithm implementation
 <br> 
 These are functions for producing 32-bit hashes for hash table lookup.
 hashword(), hashlittle(), hashlittle2(), hashbig(), mix(), and final()
 are externally useful functions. Routines to test the hash are included
 if SELF_TEST is defined. You can use this free for any purpose. It's in
 the public domain. It has no warranty.



### Methods

#### add
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.add(System.Int64,System.Int64)
```
Do addition and turn into 4 bytes.

|Parameter Name|Remarks|
|--------------|-------|
|val|-|
|___add|
 @return |


#### byteToLong
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.byteToLong(System.SByte)
```
Convert a byte into a long value without making it negative.

|Parameter Name|Remarks|
|--------------|-------|
|b|
 @return |


#### fourByteToLong
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.fourByteToLong(System.SByte[],System.Int32)
```
Convert 4 bytes from the buffer at offset into a long value.

|Parameter Name|Remarks|
|--------------|-------|
|bytes|-|
|offset|
 @return |


#### hash
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.hash(System.SByte[])
```
See hash(byte[] buffer, long initialValue)

|Parameter Name|Remarks|
|--------------|-------|
|buffer| Byte array that we are hashing on. |


_returns:  Hash value for the buffer. _

#### hashMix
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.hashMix
```
Mix up the values in the hash function.

#### leftShift
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.leftShift(System.Int64,System.Int32)
```
Left shift val by shift bits. Cut down to 4 bytes.

|Parameter Name|Remarks|
|--------------|-------|
|val|-|
|shift|
 @return |


#### subtract
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.subtract(System.Int64,System.Int64)
```
Do subtraction and turn into 4 bytes.

|Parameter Name|Remarks|
|--------------|-------|
|val|-|
|___subtract|
 @return |


#### xor
```csharp
Microsoft.VisualBasic.Mathematical.JenkinsHash.xor(System.Int64,System.Int64)
```
Left shift val by shift bits and turn in 4 bytes.

|Parameter Name|Remarks|
|--------------|-------|
|val|-|
|___xor|
 @return |



### Properties

#### MAX_VALUE
max value to limit it to 4 bytes
