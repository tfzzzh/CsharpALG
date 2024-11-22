# C# Algorithms
This repository contains implementation of many algorithms using C#.
Some of the algorithms are classic and covered in many text book.
The others can be found in research papers. Most of these algorithms are
written based on standard libary of C#. I try my best not to use a third party package.

## Algorithms By Topics
#### Sort
| Algorithm        | Description    | Time Complexity    | Reference |
| ---------------- | -------------- | -------------------| ----------|
|Quick Sort        | sort via divide and conquer          |  **O(N log N)**    | [1]        |
|Merge Sort        | sort via divide and conquer          |  **O(N log N)**    | [1]        |
|Heap Sort         | sort via min-heap| **O(N log N)** | [1] |
|Count Sort         | sort via counting ocurrence| **O(N)** | [1] |
|Radix Sort         | sort via counting ocurrence| **O(N)** | [1] |
#### String
| Algorithm        | Description    | Time Complexity    | Reference |
| ---------------- | -------------- | -------------------| ----------|
|KMP               | Match a string with a pattern | **O(M + N)** | [1] |
|Manacher | find all palindrome substrings | **O(N)**||
|Regular Expression | Match a string with a regular expression |||
|Suffix Automa| | | |
|Trie | | | |
|Z-function| | | |
#### Tree

#### Graph

#### Max Flow

#### Numerical Methods

#### Optimization

#### Machine Learning

## References
[1] Introduction to Algorithm

## How to use this repository
***Downloand and install [.Net 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)***

***Run project by name***
```
dotnet run --project ./MachineLearning
```