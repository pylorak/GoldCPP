Option Strict Off
Option Explicit On

Imports System.ComponentModel

Public Enum SymbolType
    Nonterminal = 0      'Nonterminal 
    Content = 1          'Passed to the parser
    Noise = 2            'Ignored by the parser
    [End] = 3            'End character (EOF)
    GroupStart = 4       'Group start  
    GroupEnd = 5         'Group end   
    'Note: There is no value 6. CommentLine was deprecated.
    [Error] = 7          'Error symbol
End Enum

Public Class Symbol
    '================================================================================
    ' Class Name:
    '      Symbol
    '
    ' Purpose:
    '       This class is used to store of the nonterminals used by the Deterministic
    '       Finite Automata (DFA) and LALR Parser. Symbols can be either
    '       terminals (which represent a class of tokens - such as identifiers) or
    '       nonterminals (which represent the rules and structures of the grammar).
    '       Terminal symbols fall into several catagories for use by the GOLD Parser
    '       Engine which are enumerated below.
    '
    ' Author(s):
    '      Devin Cook
    '
    ' Dependacies:
    '      (None)
    '
    '================================================================================

    Private m_Name As String
    Private m_Type As SymbolType
    Private m_TableIndex As Short

    Friend Group As Group

    Friend Sub New()
        'Nothing
    End Sub

    Friend Sub New(ByVal Name As String, ByVal Type As SymbolType, ByVal TableIndex As Short)
        m_Name = Name
        m_Type = Type
        m_TableIndex = TableIndex
    End Sub

    <Description("Returns the type of the symbol.")> _
    Public Property Type() As SymbolType
        Get
            Return m_Type
        End Get
        Friend Set(ByVal Value As SymbolType)
            m_Type = Value
        End Set
    End Property

    <Description("Returns the index of the symbol in the Symbol Table,")> _
    Public Function TableIndex() As Short
        Return m_TableIndex
    End Function

    <Description("Returns the name of the symbol.")> _
    Public Function Name() As String
        Return m_Name
    End Function

    <Description("Returns the text representing the text in BNF format.")> _
    Public Function Text(ByVal AlwaysDelimitTerminals As Boolean) As String
        Dim Result As String

        Select Case m_Type
            Case SymbolType.Nonterminal
                Result = "<" & Name() & ">"
            Case SymbolType.Content
                Result = LiteralFormat(Name, AlwaysDelimitTerminals)
            Case Else
                Result = "(" & Name() & ")"
        End Select

        Return Result
    End Function

    <Description("Returns the text representing the text in BNF format.")> _
    Public Function Text() As String
        Return Me.Text(False)
    End Function

    Private Function LiteralFormat(ByVal Source As String, ByVal ForceDelimit As Boolean) As String
        Dim n As Short
        Dim ch As Char

        If Source = "'" Then
            Return "''"
        Else
            n = 0
            Do While n < Source.Length And (Not ForceDelimit)
                ch = Source.Chars(n)
                ForceDelimit = Not (Char.IsLetter(ch) Or ch = "." Or ch = "_" Or ch = "-")
                n += 1
            Loop

            If ForceDelimit Then
                Return "'" & Source & "'"
            Else
                Return Source
            End If
        End If
    End Function

    Public Overrides Function ToString() As String
        Return Text()
    End Function
End Class

Public Class SymbolList
    Private m_Array As ArrayList   'CANNOT inherit, must hide methods that edit the list

    Friend Sub New()
        m_Array = New ArrayList
    End Sub

    Friend Sub New(ByVal Size As Integer)
        m_Array = New ArrayList
        ReDimension(Size)
    End Sub

    Friend Sub ReDimension(ByVal Size As Integer)
        'Increase the size of the array to Size empty elements.
        Dim n As Integer

        m_Array.Clear()
        For n = 0 To Size - 1
            m_Array.Add(Nothing)
        Next
    End Sub

    <Description("Returns the symbol with the specified index.")> _
    Default Public Property Item(ByVal Index As Integer) As Symbol
        Get
            If Index >= 0 And Index < m_Array.Count Then
                Return m_Array(Index)
            Else
                Return Nothing
            End If
        End Get

        Friend Set(ByVal Value As Symbol)
            m_Array(Index) = Value
        End Set
    End Property

    <Description("Returns the total number of symbols in the list.")> _
    Public Function Count() As Integer
        Return m_Array.Count
    End Function

    Friend Sub Clear()
        m_Array.Clear()
    End Sub

    Friend Function Add(ByVal Item As Symbol) As Integer
        Return m_Array.Add(Item)
    End Function

    Friend Function GetFirstOfType(ByVal Type As SymbolType) As Symbol
        Dim Found As Boolean, n As Short
        Dim Sym As Symbol, Result As Symbol = Nothing

        Found = False
        n = 0
        Do While (Not Found) And n < m_Array.Count
            Sym = m_Array.Item(n)
            If Sym.Type = Type Then
                Found = True
                Result = Sym
            End If
            n += 1
        Loop

        Return Result
    End Function

    Public Overrides Function ToString() As String
        Return Text()
    End Function

    <Description("Returns a list of the symbol names in BNF format.")> _
    Public Function Text(ByVal Separator As String, ByVal AlwaysDelimitTerminals As Boolean) As String
        Dim Result As String = ""
        Dim n As Integer
        Dim Sym As Symbol

        For n = 0 To m_Array.Count - 1
            Sym = m_Array(n)
            Result &= IIf(n = 0, "", Separator) & Sym.Text(AlwaysDelimitTerminals)
        Next

        Return Result
    End Function

    <Description("Returns a list of the symbol names in BNF format.")> _
    Public Function Text() As String
        Return Me.Text(", ", False)
    End Function

End Class