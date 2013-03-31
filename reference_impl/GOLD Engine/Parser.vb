Option Strict Off
Option Explicit On

Imports System.ComponentModel
Imports System.IO
Imports System

Public Class ParserException
    Inherits System.Exception

    Public Method As String

    Friend Sub New(ByVal Message As String)
        MyBase.New(Message)
        Me.Method = ""
    End Sub

    Friend Sub New(ByVal Message As String, ByVal Inner As Exception, ByVal Method As String)
        MyBase.New(Message, Inner)
        Me.Method = Method
    End Sub
End Class

'===== Parsing messages 
Public Enum ParseMessage
    TokenRead = 0         'A new token is read
    Reduction = 1         'A production is reduced
    Accept = 2            'Grammar complete
    NotLoadedError = 3    'The tables are not loaded
    LexicalError = 4      'Token not recognized
    SyntaxError = 5       'Token is not expected
    GroupError = 6        'Reached the end of the file inside a block
    InternalError = 7     'Something is wrong, very wrong
End Enum


Public Class GrammarProperties
    Private Const PropertyCount As Integer = 8

    Private Enum PropertyIndex
        Name = 0
        Version = 1
        Author = 2
        About = 3
        CharacterSet = 4
        CharacterMapping = 5
        GeneratedBy = 6
        GeneratedDate = 7
    End Enum

    Private m_Property(PropertyCount) As String

    Friend Sub New()
        Dim n As Integer

        For n = 0 To PropertyCount - 1
            m_Property(n) = ""
        Next
    End Sub

    Friend Sub SetValue(ByVal Index As Integer, ByVal Value As String)
        If Index >= 0 And Index < PropertyCount Then
            m_Property(Index) = Value
        End If
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Property(PropertyIndex.Name)
        End Get
    End Property

    Public ReadOnly Property Version() As String
        Get
            Return m_Property(PropertyIndex.Version)
        End Get
    End Property

    Public ReadOnly Property Author() As String
        Get
            Return m_Property(PropertyIndex.Author)
        End Get
    End Property

    Public ReadOnly Property About() As String
        Get
            Return m_Property(PropertyIndex.About)
        End Get
    End Property

    Public ReadOnly Property CharacterSet() As String
        Get
            Return m_Property(PropertyIndex.CharacterSet)
        End Get
    End Property

    Public ReadOnly Property CharacterMapping() As String
        Get
            Return m_Property(PropertyIndex.CharacterMapping)
        End Get
    End Property

    Public ReadOnly Property GeneratedBy() As String
        Get
            Return m_Property(PropertyIndex.GeneratedBy)
        End Get
    End Property

    Public ReadOnly Property GeneratedDate() As String
        Get
            Return m_Property(PropertyIndex.GeneratedDate)
        End Get
    End Property
End Class


Public Class Parser
    '===================================================================
    ' Class Name:
    '    Parser
    '
    ' Purpose:
    '    This is the main class in the GOLD Parser Engine and is used to perform
    '    all duties required to the parsing of a source text string. This class
    '    contains the LALR(1) State Machine code, the DFA State Machine code,
    '    character table (used by the DFA algorithm) and all other structures and
    '    methods needed to interact with the developer.
    '
    'Author(s):
    '   Devin Cook
    '
    'Public Dependencies:
    '   Token, TokenList, Production, ProductionList, Symbol, SymbolList, Reduction, Position
    '
    'Private Dependencies:
    '   CGTReader, TokenStack, TokenStackQueue, FAStateList, CharacterRange, CharacterSet,
    '   CharacterSetList, LRActionTableList
    '
    'Revision History:    
    '  2011-10-06
    '      * Added 5.0 logic.
    '===================================================================

    Private Const kVersion As String = "5.0"

    '===== Symbols recognized by the system
    Private m_SymbolTable As New SymbolList

    '===== DFA
    Private m_DFA As New FAStateList
    Private m_CharSetTable As New CharacterSetList
    Private m_LookaheadBuffer As String

    '===== Productions
    Private m_ProductionTable As New ProductionList

    '===== LALR
    Private m_LRStates As New LRStateList
    Private m_CurrentLALR As Integer
    Private m_Stack As New TokenStack

    '===== Used for Reductions & Errors
    Private m_ExpectedSymbols As New SymbolList       'This ENTIRE list will available to the user
    Private m_HaveReduction As Boolean
    Private m_TrimReductions As Boolean 'NEW 12/2001

    '===== Private control variables
    Private m_TablesLoaded As Boolean
    Private m_InputTokens As New TokenQueueStack  'Tokens to be analyzed - Hybred object!

    Private m_Source As TextReader

    '=== Line and column information. 
    Private m_SysPosition As New Position        'Internal - so user cannot mess with values
    Private m_CurrentPosition As New Position    'Last read terminal


    '===== The ParseLALR() function returns this value
    Private Enum ParseResult
        Accept = 1
        Shift = 2
        ReduceNormal = 3
        ReduceEliminated = 4            'Trim
        SyntaxError = 5
        InternalError = 6
    End Enum

    '===== Grammar Attributes
    Private m_Grammar As New GrammarProperties

    '===== Lexical Groups
    Private m_GroupStack As New TokenStack
    Private m_GroupTable As New GroupList

    Public Sub New()
        Restart()
        m_TablesLoaded = False

        '======= Default Properties
        m_TrimReductions = False
    End Sub

    <Description("Opens a string for parsing.")> _
    Public Function Open(ByRef Text As String) As Boolean
        Return Open(New StringReader(Text))
    End Function

    <Description("Opens a text stream for parsing.")> _
    Public Function Open(ByVal Reader As TextReader) As Boolean
        Dim Start As New Token

        Restart()
        m_Source = Reader

        '=== Create stack top item. Only needs state
        Start.State = m_LRStates.InitialState
        m_Stack.Push(Start)

        Return True
    End Function

    <Description("When the Parse() method returns a Reduce, this method will contain the current Reduction.")> _
    Public Property CurrentReduction() As Object
        Get
            If m_HaveReduction Then
                CurrentReduction = m_Stack.Top.Data
            Else
                CurrentReduction = Nothing
            End If
        End Get
        Set(ByVal Value As Object)
            If m_HaveReduction Then
                m_Stack.Top.Data = Value
            End If
        End Set
    End Property

    <Description("Determines if reductions will be trimmed in cases where a production contains a single element.")> _
    Public Property TrimReductions() As Boolean
        Get
            Return m_TrimReductions
        End Get
        Set(ByVal Value As Boolean)
            m_TrimReductions = Value
        End Set
    End Property

    <Description("Returns information about the current grammar.")> _
    Public Function Grammar() As GrammarProperties
        Return m_Grammar
    End Function

    <Description("Current line and column being read from the source.")> _
    Public Function CurrentPosition() As Position
        Return m_CurrentPosition
    End Function

    <Description("If the Parse() function returns TokenRead, this method will return that last read token.")> _
    Public Function CurrentToken() As Token
        Return m_InputTokens.Top
    End Function

    <Description("Removes the next token from the input queue.")> _
    Public Function DiscardCurrentToken() As Token
        Return m_InputTokens.Dequeue
    End Function

    <Description("Added a token onto the end of the input queue.")> _
    Public Sub EnqueueInput(ByRef TheToken As Token)
        m_InputTokens.Enqueue(TheToken)
    End Sub

    <Description("Pushes the token onto the top of the input queue. This token will be analyzed next.")> _
    Public Sub PushInput(ByRef TheToken As Token)
        m_InputTokens.Push(TheToken)
    End Sub

    Private Function LookaheadBuffer(ByVal Count As Integer) As String
        'Return Count characters from the lookahead buffer. DO NOT CONSUME
        'This is used to create the text stored in a token. It is disgarded
        'separately. Because of the design of the DFA algorithm, count should
        'never exceed the buffer length. The If-Statement below is fault-tolerate
        'programming, but not necessary.

        If Count > m_LookaheadBuffer.Length Then
            Count = m_LookaheadBuffer
        End If

        Return m_LookaheadBuffer.Substring(0, Count)
    End Function

    Private Function Lookahead(ByVal CharIndex As Integer) As String
        'Return single char at the index. This function will also increase 
        'buffer if the specified character is not present. It is used 
        'by the DFA algorithm.

        Dim ReadCount As Integer, n As Integer

        'Check if we must read characters from the Stream
        If CharIndex > m_LookaheadBuffer.Length Then
            ReadCount = CharIndex - m_LookaheadBuffer.Length
            For n = 1 To ReadCount
                m_LookaheadBuffer &= ChrW(m_Source.Read())
            Next
        End If

        'If the buffer is still smaller than the index, we have reached
        'the end of the text. In this case, return a null string - the DFA
        'code will understand.
        If CharIndex <= m_LookaheadBuffer.Length Then
            Return m_LookaheadBuffer.Chars(CharIndex - 1)
        Else
            Return ""
        End If
    End Function

    <Description("Library name and version.")> _
    Public Function About() As String
        Return "GOLD Parser Engine; Version " & kVersion
    End Function

    Friend Sub Clear()
        m_SymbolTable.Clear()
        m_ProductionTable.Clear()
        m_CharSetTable.Clear()
        m_DFA.Clear()
        m_LRStates.Clear()

        m_Stack.Clear()
        m_InputTokens.Clear()

        m_Grammar = New GrammarProperties()

        m_GroupStack.Clear()
        m_GroupTable.Clear()

        Restart()
    End Sub

    <Description("Loads parse tables from the specified filename. Only EGT (version 5.0) is supported.")> _
    Public Function LoadTables(ByVal Path As String) As Boolean
        Return LoadTables(New BinaryReader(File.Open(Path, FileMode.Open, FileAccess.Read)))
    End Function

    <Description("Loads parse tables from the specified BinaryReader. Only EGT (version 5.0) is supported.")> _
    Public Function LoadTables(ByVal Reader As BinaryReader) As Boolean
        Dim EGT As New EGTReader
        Dim Success As Boolean
        Dim RecType As EGTRecord

        Try
            EGT.Open(Reader)

            Restart()
            Success = True
            Do Until EGT.EndOfFile Or Success = False
                EGT.GetNextRecord()

                RecType = EGT.RetrieveByte

                Select Case RecType
                    Case EGTRecord.Property
                        'Index, Name, Value
                        Dim Index As Integer, Name As String

                        Index = EGT.RetrieveInt16()
                        Name = EGT.RetrieveString()     'Just discard
                        m_Grammar.SetValue(Index, EGT.RetrieveString)

                    Case EGTRecord.TableCounts
                        'Symbol, CharacterSet, Rule, DFA, LALR
                        m_SymbolTable = New SymbolList(EGT.RetrieveInt16)
                        m_CharSetTable = New CharacterSetList(EGT.RetrieveInt16)
                        m_ProductionTable = New ProductionList(EGT.RetrieveInt16)
                        m_DFA = New FAStateList(EGT.RetrieveInt16)
                        m_LRStates = New LRStateList(EGT.RetrieveInt16)
                        m_GroupTable = New GroupList(EGT.RetrieveInt16)

                    Case EGTRecord.InitialStates
                        'DFA, LALR
                        m_DFA.InitialState = EGT.RetrieveInt16()
                        m_LRStates.InitialState = EGT.RetrieveInt16()

                    Case EGTRecord.Symbol
                        '#, Name, Kind
                        Dim Index As Integer
                        Dim Name As String
                        Dim Type As SymbolType

                        Index = EGT.RetrieveInt16()
                        Name = EGT.RetrieveString()
                        Type = EGT.RetrieveInt16()

                        m_SymbolTable(Index) = New Symbol(Name, Type, Index)

                    Case EGTRecord.Group
                        '#, Name, Container#, Start#, End#, Tokenized, Open Ended, Reserved, Count, (Nested Group #...) 
                        Dim G As New Group
                        Dim Index, Count, n As Integer

                        With G
                            Index = EGT.RetrieveInt16()                 '# 

                            .Name = EGT.RetrieveString
                            .Container = SymbolTable(EGT.RetrieveInt16())
                            .Start = SymbolTable(EGT.RetrieveInt16())
                            .End = SymbolTable(EGT.RetrieveInt16())

                            .Advance = EGT.RetrieveInt16()
                            .Ending = EGT.RetrieveInt16()
                            EGT.RetrieveEntry()                  'Reserved

                            Count = EGT.RetrieveInt16()
                            For n = 1 To Count
                                .Nesting.Add(EGT.RetrieveInt16())
                            Next
                        End With

                        '=== Link back
                        G.Container.Group = G
                        G.Start.Group = G
                        G.End.Group = G

                        m_GroupTable(Index) = G

                    Case EGTRecord.CharRanges
                        '#, Total Sets, RESERVED, (Start#, End#  ...)
                        Dim Index As Integer
                        Dim Total As Integer

                        Index = EGT.RetrieveInt16
                        EGT.RetrieveInt16()       'Codepage
                        Total = EGT.RetrieveInt16
                        EGT.RetrieveEntry()       'Reserved

                        m_CharSetTable(Index) = New CharacterSet
                        Do Until EGT.RecordComplete
                            m_CharSetTable(Index).Add(New CharacterRange(EGT.RetrieveInt16, EGT.RetrieveInt16))
                        Loop

                    Case EGTRecord.Production
                        '#, ID#, Reserved, (Symbol#,  ...)
                        Dim Index, HeadIndex, SymIndex As Integer

                        Index = EGT.RetrieveInt16
                        HeadIndex = EGT.RetrieveInt16
                        EGT.RetrieveEntry() 'Reserved

                        m_ProductionTable(Index) = New Production(m_SymbolTable(HeadIndex), Index)

                        Do Until EGT.RecordComplete
                            SymIndex = EGT.RetrieveInt16
                            m_ProductionTable(Index).Handle.Add(m_SymbolTable(SymIndex))
                        Loop

                    Case EGTRecord.DFAState
                        '#, Accept?, Accept#, Reserved (CharSet#, Target#, Reserved)...
                        Dim Index As Integer
                        Dim Accept As Boolean
                        Dim AcceptIndex As Integer
                        Dim SetIndex As Integer
                        Dim Target As Integer

                        Index = EGT.RetrieveInt16
                        Accept = EGT.RetrieveBoolean
                        AcceptIndex = EGT.RetrieveInt16
                        EGT.RetrieveEntry() 'Reserved

                        If Accept Then
                            m_DFA(Index) = New FAState(m_SymbolTable(AcceptIndex))
                        Else
                            m_DFA(Index) = New FAState
                        End If

                        '(Edge chars, Target#, Reserved)...
                        Do Until EGT.RecordComplete
                            SetIndex = EGT.RetrieveInt16 'Char table index
                            Target = EGT.RetrieveInt16 'Target
                            EGT.RetrieveEntry() 'Reserved

                            m_DFA(Index).Edges.Add(New FAEdge(m_CharSetTable(SetIndex), Target))
                        Loop

                    Case EGTRecord.LRState
                        '#, Reserved (Symbol#, Action, Target#, Reserved)...
                        Dim Index As Integer
                        Dim SymIndex, Action, Target As Integer

                        Index = EGT.RetrieveInt16
                        EGT.RetrieveEntry() 'Reserved

                        m_LRStates(Index) = New LRState

                        '(Symbol#, Action, Target#, Reserved)...
                        Do Until EGT.RecordComplete
                            SymIndex = EGT.RetrieveInt16
                            Action = EGT.RetrieveInt16
                            Target = EGT.RetrieveInt16
                            EGT.RetrieveEntry() 'Reserved

                            m_LRStates(Index).Add(New LRAction(m_SymbolTable(SymIndex), Action, Target))
                        Loop

                    Case Else 'RecordIDComment
                        Success = False
                        Throw New ParserException("File Error. A record of type '" & ChrW(RecType) & "' was read. This is not a valid code.")
                End Select
            Loop

            EGT.Close()

        Catch ex As Exception
            Throw New ParserException(ex.Message, ex, "LoadTables")
        End Try

        m_TablesLoaded = Success

        Return Success
    End Function

    <Description("Returns a list of Symbols recognized by the grammar.")> _
    Public Function SymbolTable() As SymbolList
        Return m_SymbolTable
    End Function

    <Description("Returns a list of Productions recognized by the grammar.")> _
    Public Function ProductionTable() As ProductionList
        Return m_ProductionTable
    End Function

    <Description("If the Parse() method returns a SyntaxError, this method will contain a list of the symbols the grammar expected to see.")> _
    Public Function ExpectedSymbols() As SymbolList
        Return m_ExpectedSymbols
    End Function

    Private Function ParseLALR(ByRef NextToken As Token) As ParseResult
        'This function analyzes a token and either:
        '  1. Makes a SINGLE reduction and pushes a complete Reduction object on the m_Stack
        '  2. Accepts the token and shifts
        '  3. Errors and places the expected symbol indexes in the Tokens list
        'The Token is assumed to be valid and WILL be checked
        'If an action is performed that requires controlt to be returned to the user, the function returns true.
        'The Message parameter is then set to the type of action.

        Dim Index, n As Short
        Dim ParseAction As LRAction
        Dim Prod As Production
        Dim Head As Token
        Dim NewReduction As Reduction
        Dim Result As ParseResult

        ParseAction = m_LRStates(m_CurrentLALR).Item(NextToken.Parent)

        If Not ParseAction Is Nothing Then ' Work - shift or reduce
            m_HaveReduction = False 'Will be set true if a reduction is made
            ''Debug.WriteLine("Action: " & ParseAction.Text)

            Select Case ParseAction.Type
                Case LRActionType.Accept
                    m_HaveReduction = True
                    Result = ParseResult.Accept

                Case LRActionType.Shift
                    m_CurrentLALR = ParseAction.Value
                    NextToken.State = m_CurrentLALR
                    m_Stack.Push(NextToken)
                    Result = ParseResult.Shift

                Case LRActionType.Reduce
                    'Produce a reduction - remove as many tokens as members in the rule & push a nonterminal token
                    Prod = m_ProductionTable(ParseAction.Value)

                    '======== Create Reduction
                    If m_TrimReductions And Prod.ContainsOneNonTerminal Then
                        'The current rule only consists of a single nonterminal and can be trimmed from the
                        'parse tree. Usually we create a new Reduction, assign it to the Data property
                        'of Head and push it on the m_Stack. However, in this case, the Data property of the
                        'Head will be assigned the Data property of the reduced token (i.e. the only one
                        'on the m_Stack).
                        'In this case, to save code, the value popped of the m_Stack is changed into the head.

                        Head = m_Stack.Pop()
                        Head.Parent = Prod.Head

                        Result = ParseResult.ReduceEliminated
                    Else 'Build a Reduction
                        m_HaveReduction = True
                        NewReduction = New Reduction(Prod.Handle.Count)

                        With NewReduction
                            .Parent = Prod
                            For n = Prod.Handle.Count - 1 To 0 Step -1
                                .Item(n) = m_Stack.Pop()
                            Next
                        End With

                        Head = New Token(Prod.Head, NewReduction)
                        Result = ParseResult.ReduceNormal
                    End If

                    '========== Goto
                    Index = m_Stack.Top().State

                    '========= If n is -1 here, then we have an Internal Table Error!!!!
                    n = m_LRStates(Index).IndexOf(Prod.Head)
                    If n <> -1 Then
                        m_CurrentLALR = m_LRStates(Index).Item(n).Value

                        Head.State = m_CurrentLALR
                        m_Stack.Push(Head)
                    Else
                        Result = ParseResult.InternalError
                    End If
            End Select

        Else
            '=== Syntax Error! Fill Expected Tokens
            m_ExpectedSymbols.Clear()
            For Each Action As LRAction In m_LRStates(m_CurrentLALR)  '.Count - 1
                Select Case Action.Symbol.Type
                    Case SymbolType.Content, SymbolType.End, SymbolType.GroupStart, SymbolType.GroupEnd
                        m_ExpectedSymbols.Add(Action.Symbol)
                End Select
            Next
            Result = ParseResult.SyntaxError
        End If

        Return Result 'Very important
    End Function

    <Description("Restarts the parser. Loaded tables are retained.")> _
    Public Sub Restart()
        m_CurrentLALR = m_LRStates.InitialState

        '=== Lexer
        m_SysPosition.Column = 0
        m_SysPosition.Line = 0
        m_CurrentPosition.Line = 0
        m_CurrentPosition.Column = 0

        m_HaveReduction = False

        m_ExpectedSymbols.Clear()
        m_InputTokens.Clear()
        m_Stack.Clear()
        m_LookaheadBuffer = ""

        '==== V4
        m_GroupStack.Clear()
    End Sub

    <Description("Returns true if parse tables were loaded.")> _
    Public Function TablesLoaded() As Boolean
        Return m_TablesLoaded
    End Function

    Private Function LookaheadDFA() As Token
        'This function implements the DFA for th parser's lexer.
        'It generates a token which is used by the LALR state
        'machine.

        Dim Ch As String
        Dim n, Target, CurrentDFA As Integer
        Dim Found, Done As Boolean
        Dim Edge As FAEdge
        Dim CurrentPosition As Integer
        Dim LastAcceptState, LastAcceptPosition As Integer
        Dim Result As New Token

        '===================================================
        'Match DFA token
        '===================================================

        Done = False
        CurrentDFA = m_DFA.InitialState
        CurrentPosition = 1               'Next byte in the input Stream
        LastAcceptState = -1              'We have not yet accepted a character string
        LastAcceptPosition = -1

        Ch = Lookahead(1)
        If Not (Ch = "" Or AscW(Ch) = 65535) Then 'NO MORE DATA
            Do Until Done
                ' This code searches all the branches of the current DFA state
                ' for the next character in the input Stream. If found the
                ' target state is returned.

                Ch = Lookahead(CurrentPosition)
                If Ch = "" Then     'End reached, do not match
                    Found = False
                Else
                    n = 0
                    Found = False
                    Do While n < m_DFA(CurrentDFA).Edges.Count And Not Found
                        Edge = m_DFA(CurrentDFA).Edges(n)

                        '==== Look for character in the Character Set Table
                        If Edge.Characters.Contains(AscW(Ch)) Then
                            Found = True
                            Target = Edge.Target '.TableIndex
                        End If
                        n += 1
                    Loop
                End If

                ' This block-if statement checks whether an edge was found from the current state. If so, the state and current
                ' position advance. Otherwise it is time to exit the main loop and report the token found (if there was one). 
                ' If the LastAcceptState is -1, then we never found a match and the Error Token is created. Otherwise, a new 
                ' token is created using the Symbol in the Accept State and all the characters that comprise it.

                If Found Then
                    ' This code checks whether the target state accepts a token.
                    ' If so, it sets the appropiate variables so when the
                    ' algorithm in done, it can return the proper token and
                    ' number of characters.

                    If Not m_DFA(Target).Accept Is Nothing Then      'NOT is very important!
                        LastAcceptState = Target
                        LastAcceptPosition = CurrentPosition
                    End If

                    CurrentDFA = Target
                    CurrentPosition += 1

                Else 'No edge found
                    Done = True
                    If LastAcceptState = -1 Then     ' Lexer cannot recognize symbol
                        Result.Parent = m_SymbolTable.GetFirstOfType(SymbolType.Error)
                        Result.Data = LookaheadBuffer(1)
                    Else                            ' Create Token, read characters
                        Result.Parent = m_DFA(LastAcceptState).Accept
                        Result.Data = LookaheadBuffer(LastAcceptPosition)   'Data contains the total number of accept characters
                    End If
                End If
                'DoEvents
            Loop

        Else
            ' End of file reached, create End Token
            Result.Data = ""
            Result.Parent = m_SymbolTable.GetFirstOfType(SymbolType.End)
        End If

        '===================================================
        'Set the new token's position information
        '===================================================
        'Notice, this is a copy, not a linking of an instance. We don't want the user 
        'to be able to alter the main value indirectly.
        Result.Position.Copy(m_SysPosition)

        Return Result
    End Function

    Private Sub ConsumeBuffer(ByVal CharCount As Integer)
        'Consume/Remove the characters from the front of the buffer. 

        Dim n As Integer

        If CharCount <= m_LookaheadBuffer.Length Then
            ' Count Carriage Returns and increment the internal column and line
            ' numbers. This is done for the Developer and is not necessary for the
            ' DFA algorithm.
            For n = 0 To CharCount - 1
                Select Case m_LookaheadBuffer(n)
                    Case vbLf
                        m_SysPosition.Line += 1
                        m_SysPosition.Column = 0
                    Case vbCr
                        'Ignore, LF is used to inc line to be UNIX friendly
                    Case Else
                        m_SysPosition.Column += 1
                End Select
            Next

            m_LookaheadBuffer = m_LookaheadBuffer.Remove(0, CharCount)
        End If
    End Sub

    Private Function ProduceToken() As Token
        ' ** VERSION 5.0 **
        'This function creates a token and also takes into account the current
        'lexing mode of the parser. In particular, it contains the group logic. 
        '
        'A stack is used to track the current "group". This replaces the comment
        'level counter. Also, text is appended to the token on the top of the 
        'stack. This allows the group text to returned in one chunk.

        Dim Read, Pop, Top As Token
        Dim Result As Token
        Dim Done As Boolean
        Dim NestGroup As Boolean

        Done = False
        Result = Nothing
        Read = Nothing

        While Not Done
            Read = LookaheadDFA()

            'The logic - to determine if a group should be nested - requires that the top of the stack 
            'and the symbol's linked group need to be looked at. Both of these can be unset. So, this section
            'sets a Boolean and avoids errors. We will use this boolean in the logic chain below. 
            If Read.Type = SymbolType.GroupStart Then
                If m_GroupStack.Count = 0 Then
                    NestGroup = True
                Else
                    NestGroup = m_GroupStack.Top.Group.Nesting.Contains(Read.Group.TableIndex)
                End If
            Else
                NestGroup = False
            End If

            '=================================
            ' Logic chain
            '=================================

            If NestGroup Then
                ConsumeBuffer(Read.Data.Length)
                m_GroupStack.Push(Read)

            ElseIf m_GroupStack.Count = 0 Then
                'The token is ready to be analyzed.             
                ConsumeBuffer(Read.Data.Length)
                Result = Read
                Done = True

            ElseIf (m_GroupStack.Top.Group.End Is Read.Parent) Then
                'End the current group
                Pop = m_GroupStack.Pop()

                '=== Ending logic
                If Pop.Group.Ending = Group.EndingMode.Closed Then
                    Pop.Data &= Read.Data              'Append text
                    ConsumeBuffer(Read.Data.Length)    'Consume token
                End If

                If m_GroupStack.Count = 0 Then            'We are out of the group. Return pop'd token (which contains all the group text)
                    Pop.Parent = Pop.Group.Container      'Change symbol to parent
                    Result = Pop
                    Done = True
                Else
                    m_GroupStack.Top.Data &= Pop.Data     'Append group text to parent
                End If

            ElseIf Read.Type = SymbolType.End Then
                'EOF always stops the loop. The caller function (Parse) can flag a runaway group error.
                Result = Read
                Done = True

            Else
                'We are in a group, Append to the Token on the top of the stack.
                'Take into account the Token group mode  
                Top = m_GroupStack.Top

                If Top.Group.Advance = Group.AdvanceMode.Token Then
                    Top.Data &= Read.Data             ' Append all text
                    ConsumeBuffer(Read.Data.Length)
                Else
                    Top.Data &= Read.Data.Chars(0)    ' Append one character
                    ConsumeBuffer(1)
                End If
            End If
        End While

        Return Result
    End Function

    <Description("Performs a parse action on the input. This method is typically used in a loop until either grammar is accepted or an error occurs.")> _
    Public Function Parse() As ParseMessage
        Dim Message As ParseMessage
        Dim Done As Boolean
        Dim Read As Token
        Dim Action As ParseResult

        If Not m_TablesLoaded Then
            Return ParseMessage.NotLoadedError
        End If

        '===================================
        'Loop until breakable event
        '===================================
        Done = False
        While Not Done
            If m_InputTokens.Count = 0 Then
                Read = ProduceToken()
                m_InputTokens.Push(Read)

                Message = ParseMessage.TokenRead
                Done = True
            Else
                Read = m_InputTokens.Top()
                m_CurrentPosition.Copy(Read.Position)   'Update current position

                If m_GroupStack.Count <> 0 Then   'Runaway group
                    Message = ParseMessage.GroupError
                    Done = True
                ElseIf Read.Type = SymbolType.Noise Then
                    'Just discard. These were already reported to the user.
                    m_InputTokens.Pop()

                ElseIf Read.Type = SymbolType.Error Then
                    Message = ParseMessage.LexicalError
                    Done = True

                Else    'Finally, we can parse the token.
                    Action = ParseLALR(Read)             'SAME PROCEDURE AS v1

                    Select Case Action
                        Case ParseResult.Accept
                            Message = ParseMessage.Accept
                            Done = True

                        Case ParseResult.InternalError
                            Message = ParseMessage.InternalError
                            Done = True

                        Case ParseResult.ReduceNormal
                            Message = ParseMessage.Reduction
                            Done = True

                        Case ParseResult.Shift
                            'ParseToken() shifted the token on the front of the Token-Queue. 
                            'It now exists on the Token-Stack and must be eliminated from the queue.
                            m_InputTokens.Dequeue()

                        Case ParseResult.SyntaxError
                            Message = ParseMessage.SyntaxError
                            Done = True

                        Case Else
                            'Do nothing.
                    End Select
                End If
            End If
        End While

        Return Message
    End Function


End Class