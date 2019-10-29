<img src="https://raw.githubusercontent.com/yemrekeskin/SwiftNotationParser/master/nuget/package_ico.png" width="100" height="70"> 

# SwiftNotationParser
This project provides parsing field/element used in MT messages for Financial SWIFT Messages 
> **Note:** SWIFT  = Society for Worldwide Inter-bank Financial Telecommunication

## MT (Message Text) Format

The Usage Guideline Editor allows the formalization of a field/element format as an MT Format. The MT format language is defined by SWIFT. It describes how a field is structure by specifying :

-   which type of characters can be used in that field
-   what are the restrictions on the length of the field (i.e. how many times each type of character can appear and in which order)

**Here is a simplified definition of those two attributes:**
![enter image description here](https://raw.githubusercontent.com/yemrekeskin/SwiftNotationParser/master/nuget/MT_Format.png)

## Features
 - parses the message tag fields and sub-fields
 - parses bic code (business Institution code)  or swift codes in other words

## Limitations
- do not parse mt messages
 - one-way parsing only - doesn't generate the MT messages

## Sample Notations

 - notation : 3!a15d     value : EUR50000,00
 - notation : 5n[/5n]    value : 123/11
 - notation : 1!a6!n3!a15d   value : A123456ABC1234,

## Installation
 Installation from nuget -- [https://www.nuget.org/packages/SwiftNotationParser](https://www.nuget.org/packages/SwiftNotationParser)
 ``` 
 Install-Package SwiftNotationParser -Version 1.0.1
```

## Usage

``` csharp
string notation = "3!a15d";
string value = "EUR50000,00";

SwiftParser parser = new SwiftParser(notation);
var result = parser.parse(value);
```

## Useful Links

 - [Swift Organization](https://www.swift.com/)
 - [Standarts MT](https://www2.swift.com/knowledgecentre/products/Standards%20MT)
 - [Reference Guide Portal](https://www2.swift.com)
