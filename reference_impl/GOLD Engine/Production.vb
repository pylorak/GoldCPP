Option Strict Off
Option Explicit On

Imports System.ComponentModel

Public Class Production
    '================================================================================
    ' Class Name:
    '      Production 
    '
    ' Instancing:
    '      Public; Non-creatable  (VB Setting: 2- PublicNotCreatable)
    '
    ' Purpose:
    '      The Rule class is used to represent the logical structures of the grammar.
    '      Rules consist of a head containing a nonterminal followed by a series of
    '      both nonterminals and terminals.
    '
    ' Author(s):
    '      Devin Cook
    '      http://www.devincook.com/goldparser
    '
    ' Dependacies:
    '      Symbol Class, SymbolList Class
    '
    '================================================================================

    Private m_Head As Symbol
    Private m_Handle As SymbolList
    Private m_TableIndex As Short

    Friend Sub New(ByVal Head As Symbol, ByVal TableIndex As Short)
        m_Head = Head
        m_Handle = New SymbolList
        m_TableIndex = TableIndex
    End Sub

    Friend Sub New()
        'Nothing
    End Sub

    <Description("Returns the head of the production.")> _
    Public Function Head() As Symbol
        Return m_Head
    End Function

    <Description("Returns the symbol list containing the handle (body) of the production.")> _
    Public Function Handle() As SymbolList
        Return m_Handle
    End Function

    <Description("Returns the index of the production in the Production Table.")> _
    Public Function TableIndex() As Short
        Return m_TableIndex
    End Function

    Public Overrides Function ToString() As String
        Return Text()
    End Function

    <Description("Returns the production in BNF.")> _
    Public Function Text(Optional ByVal AlwaysDelimitTerminals As Boolean = False) As String
        Return m_Head.Text() & " ::= " & m_Handle.Text(" ", AlwaysDelimitTerminals)
    End Function

    Friend Function ContainsOneNonTerminal() As Boolean
        Dim Result As Boolean = False

        If m_Handle.Count = 1 Then
            If m_Handle(0).Type = SymbolType.Nonterminal Then
                Result = True
            End If
        End If

        Return Result
    End Function
End Class

Public Class ProductionList
    Private m_Array As ArrayList   'Cannot inherit, must hide methods that change the list

    Friend Sub New()
        m_Array = New ArrayList
    End Sub

    Friend Sub New(ByVal Size As Integer)
        m_Array = New ArrayList
        ReDimension(Size)
    End Sub

    Friend Sub Clear()
        m_Array.Clear()
    End Sub

    Friend Sub ReDimension(ByVal Size As Integer)
        'Increase the size of the array to Size empty elements.
        Dim n As Integer

        m_Array.Clear()
        For n = 0 To Size - 1
            m_Array.Add(Nothing)
        Next
    End Sub

    <Description("Returns the production with the specified index.")> _
    Default Public Shadows Property Item(ByVal Index As Integer) As Production
        Get
            Return m_Array(Index)
        End Get

        Friend Set(ByVal Value As Production)
            m_Array(Index) = Value
        End Set
    End Property

    <Description("Returns the total number of productions in the list.")> _
    Public Function Count() As Integer
        Return m_Array.Count
    End Function

    Friend Shadows Function Add(ByVal Item As Production) As Integer
        Return m_Array.Add(Item)
    End Function
End Class