#include "Parser.h"
#include "EGT.h"
#include <cassert>
#include <memory>

//#include "memcheck/mmgr.h"

namespace GoldCPP
{
  void GrammarProperties::setProperty(PropertyIndex index, const GPSTR_T &val)
  {
    _Properties[index] = val;
  }

  GPSTR_T GrammarProperties::getProperty(PropertyIndex index) const
  {
    return _Properties[index];
  }

  Parser::Parser() :
    TrimReductions(false)
  {
    Clear();
  }

   /* When the Parse() method returns a Reduce, this method will
   contain the current Reduction. */
  std::shared_ptr<Reduction> Parser::GetCurrentReduction()
  {
    if (HaveReduction_)
        return Stack_.top()->ReductionData;
    else
      return NULL;
  }

  void Parser::SetCurrentReduction(const std::shared_ptr<Reduction> &value)
  {
    if (HaveReduction_)
        Stack_.top()->ReductionData = value;
  }

  /* Current line and column being read from the source. */
  Position Parser::GetCurrentPosition() const
  {
    return CurrentPosition_;
  }

  /* If the Parse() function returns TokenRead,
  this method will return that last read token. */
  std::shared_ptr<Token> Parser::GetCurrentToken() const
  {
    return InputTokens_.Top();
  }

  /* Removes the next token from the input queue. */
  std::shared_ptr<Token> Parser::DiscardCurrentToken()
  {
    return InputTokens_.Dequeue();
  }

  /* Added a token onto the end of the input queue. */
  void Parser::EnqueueInput(const std::shared_ptr<Token> &token)
  {
    InputTokens_.Enqueue(token);
  }

  /* Pushes the token onto the top of the input queue.
  This token will be analyzed next. */
  void Parser::PushInput(const std::shared_ptr<Token> &token)
  {
    InputTokens_.Push(token);
  }

  GPSTR_T Parser::LookaheadBuffer(size_t count) const
  {
    /* Return Count characters from the lookahead buffer. DO NOT CONSUME
    This is used to create the text stored in a token. It is disgarded
    separately. Because of the design of the DFA algorithm, count should
    never exceed the buffer length. The If-Statement below is fault-tolerate
    programming, but not necessary.
    */

    if (count > LookaheadBuffer_.size())
      count = LookaheadBuffer_.size();

    return LookaheadBuffer_.substr(0, count);
  }

  GPCHR_T Parser::Lookahead(size_t charIndex) const
  {
    /* Return single char at the index. This function will also increase
    buffer if the specified character is not present. It is used
    by the DFA algorithm.
    */

    /* If the buffer is smaller than the index, we have reached
    the end of the text. In this case, return a null string - the DFA
    code will understand.
    */

    if (charIndex <= LookaheadBuffer_.size())
      return LookaheadBuffer_[charIndex - 1];
    else
      return 0;
  }

  /* Library name and version. */
  GPSTR_T Parser::GetAbout() const
  {
    return GPSTR_C("GOLD Parser Engine; Version ") + kVersion_;
  }

  /* Returns a list of Symbols recognized by the grammar. */
  SymbolList Parser::GetSymbolTable() const
  {
    return SymbolTable_;
  }

  /* Returns a list of Productions recognized by the grammar. */
  ProductionList Parser::GetProductionTable() const
  {
    return ProductionTable_;
  }

  /* If the Parse() method returns a SyntaxError, this method will contain a list of
  the symbols the grammar expected to see. */
  SymbolList Parser::GetExpectedSymbols() const
  {
    return ExpectedSymbols_;
  }

  /* Returns true if parse tables were loaded. */
  bool Parser::TablesLoaded() const
  {
    return TablesLoaded_;
  }

  /* Specifies the text to be parsed */
  bool Parser::Open(const GPSTR_T &source)
  {
    Restart();
    LookaheadBuffer_ = source;

    // Create stack top item. Only needs state
    std::shared_ptr<Token> Start = std::make_shared<Token>();
    Start->State = LRStates_.InitialState;
    Stack_.push(Start);
    return true;
  }

  /* Restarts the parser. Loaded tables are retained. */
  void Parser::Restart()
  {
    LookaheadBuffer_ = GPSTR_C("");
    CurrentLALR_ = LRStates_.InitialState;
    Stack_ = TokenStack();
    ExpectedSymbols_.Clear();
    HaveReduction_ = false;
    InputTokens_.Clear();

    // Lexer
    SysPosition_ = Position();
    CurrentPosition_ = Position();

    // V4
    GroupStack_ = TokenStack();
  }

  void Parser::Clear()
  {
    Restart();

    SymbolTable_.Clear();
    DFA_.Clear();
    CharSetTable_.Clear();
    ProductionTable_.Clear();
    LRStates_.Clear();
    TablesLoaded_ = false;
    GroupTable_.Clear();
    Grammar = GrammarProperties();
  }

  /* Loads parse tables from the specified BinaryReader. Only EGT (version 5.0) is supported. */
  bool Parser::LoadTables(const uint8_t* binstream, size_t len)
  {
    Clear();

    EgtReader EGT(binstream, len);

    bool egtSuccess;
    bool Success = true;
    while(!EGT.EofReached() && Success)
    {
      EGT.GetNextRecord();
      EgtReader::EgtRecord RecType = (EgtReader::EgtRecord)EGT.RetrieveByte(&egtSuccess);
      assert(egtSuccess);

      switch(RecType)
      {
      case EgtReader::Property:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        GPSTR_T name = EGT.RetrieveString(&egtSuccess); assert(egtSuccess);
        Grammar.setProperty((GrammarProperties::PropertyIndex)index, name);
        break;
        }
      case EgtReader::TableCounts:
        {
        SymbolTable_ = SymbolList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        CharSetTable_ = CharacterSetList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        ProductionTable_ = ProductionList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        DFA_ = FaStateList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        LRStates_ = LRStateList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        GroupTable_ = GroupList(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);
        break;
        }
      case EgtReader::InitialStates:
        {
        DFA_.InitialState = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        LRStates_.InitialState = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        break;
        }
      case EgtReader::Symbol:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        GPSTR_T name = EGT.RetrieveString(&egtSuccess); assert(egtSuccess);
        Symbol::SymbolType type = (Symbol::SymbolType)EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        SymbolTable_[index] = Symbol(name, type, index);
        break;
        }
      case EgtReader::Group:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);

        GroupTable_[index] = Group();
        Group *G = &(GroupTable_[index]);

        G->Name = EGT.RetrieveString(&egtSuccess); assert(egtSuccess);
        G->Container = &(SymbolTable_[EGT.RetrieveInt16(&egtSuccess)]); assert(egtSuccess);
        G->Start = &(SymbolTable_[EGT.RetrieveInt16(&egtSuccess)]); assert(egtSuccess);
        G->End = &(SymbolTable_[EGT.RetrieveInt16(&egtSuccess)]); assert(egtSuccess);

        G->Advance = (Group::AdvanceMode)EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        G->Ending = (Group::EndingMode)EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveEntry();  // Reserved

        uint16_t count = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        G->Nesting.Reserve(count);
        for (uint16_t i = 0; i < count; ++i)
          G->Nesting.Add(EGT.RetrieveInt16(&egtSuccess)); assert(egtSuccess);

        // Link back
        G->Container->GoldGroup = G;
        G->Start->GoldGroup = G;
        G->End->GoldGroup = G;

        break;
        }
      case EgtReader::CharRanges:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess); // codepage
        uint16_t total = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveEntry();  // Reserved

        CharSetTable_[index] = CharacterSet();
        CharacterSet *charSet = &(CharSetTable_[index]);
        charSet->Reserve(total);

        uint16_t rangesFound = 0;
        while(!EGT.RecordComplete())
        {
          ++rangesFound;
          uint32_t rangeStart = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          uint32_t rangeEnd = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          charSet->Add(CharacterRange(rangeStart, rangeEnd));
        }
        assert(rangesFound == total);
        break;
        }
      case EgtReader::Production:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        uint16_t headIndex = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveEntry();  // Reserved

        ProductionTable_[index] = Production(&(SymbolTable_[headIndex]), index);

        SymbolList &symList = ProductionTable_[index].Handle;
        while(!EGT.RecordComplete())
        {
          uint16_t symIndex = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          symList.Add(SymbolTable_[symIndex]);
        }
        break;
        }
      case EgtReader::DFAState:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        bool accept = EGT.RetrieveBoolean(&egtSuccess); assert(egtSuccess);
        uint16_t acceptIndex = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveEntry();  // Reserved

        if (accept)
          DFA_[index] = FaState(&(SymbolTable_[acceptIndex]));
        else
          DFA_[index] = FaState();

        FaEdgeList &edgeList = DFA_[index].Edges;
        while(!EGT.RecordComplete())
        {
          uint16_t setIndex = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          uint16_t target = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          EGT.RetrieveEntry();  // Reserved
          edgeList.Add(FaEdge(&(CharSetTable_[setIndex]), target));
        }
        break;
        }
      case EgtReader::LRState:
        {
        uint16_t index = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
        EGT.RetrieveEntry();  // Reserved

        LRStates_[index] = LRState();

        Vector<LRAction> &actionList = LRStates_[index].Actions;
        while(!EGT.RecordComplete())
        {
          uint16_t symIndex = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          uint16_t action = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          uint16_t target = EGT.RetrieveInt16(&egtSuccess); assert(egtSuccess);
          EGT.RetrieveEntry();  // Reserved
          actionList.Add(LRAction(&(SymbolTable_[symIndex]), (LRActionType)action, target));
        }
        break;
        }
      default:
        {
        Success = false;
        break;
        }
      } // switch
    } // loop

    TablesLoaded_ = Success;
    return Success;

  } // method

  ParseResult Parser::ParseLALR(const std::shared_ptr<Token> &NextToken)
  {
    /* This function analyzes a token and either:
      1. Makes a SINGLE reduction and pushes a complete Reduction object on the m_Stack
      2. Accepts the token and shifts
      3. Errors and places the expected symbol indexes in the Tokens list
    The Token is assumed to be valid and WILL be checked
    If an action is performed that requires controlt to be returned to the user, the function returns true.
    The Message parameter is then set to the type of action. */

    ParseResult Result;
    std::shared_ptr<Token> Head;
    const LRAction* ParseAction = LRStates_[CurrentLALR_].GetActionForSymbol(NextToken->Parent);

    if (ParseAction)    // Work - shift or reduce
    {
        HaveReduction_ = false;   // Will be set true if a reduction is made
        //Debug.WriteLine("Action: " & ParseAction.Text)

        switch(ParseAction->Type)
        {
        case LRActionType::Accept:
          HaveReduction_ = true;
          Result = ParseResult::Accept;
          break;
        case LRActionType::Shift:
          CurrentLALR_ = ParseAction->Value;
          NextToken->State = CurrentLALR_;
          Stack_.push(NextToken);
          Result = ParseResult::Shift;
          break;
        case LRActionType::Reduce:
          {
          // Produce a reduction - remove as many tokens as members in the rule & push a nonterminal token
          Production *Prod = &(ProductionTable_[ParseAction->Value]);

          // Create Reduction
          if (TrimReductions && Prod->ContainsOneNonTerminal())
          {
            /* The current rule only consists of a single nonterminal and can be trimmed from the
            parse tree. Usually we create a new Reduction, assign it to the Data property
            of Head and push it on the m_Stack. However, in this case, the Data property of the
            Head will be assigned the Data property of the reduced token (i.e. the only one
            on the m_Stack).
            In this case, to save code, the value popped of the m_Stack is changed into the head.
            */

            Head = Stack_.top();
            Stack_.pop();

            Head->Parent = Prod->Head;
            Result = ParseResult::ReduceEliminated;
          }
          else // Build a Reduction
          {
            HaveReduction_ = true;
            std::shared_ptr<Reduction> NewReduction = std::make_shared<Reduction>(Prod->Handle.Count());
            NewReduction->Parent = Prod;
            for (size_t i = Prod->Handle.Count()-1; i < Prod->Handle.Count(); --i)
            {
              NewReduction->Branches[i] = Stack_.top();
              Stack_.pop();
            }

            Head = std::make_shared<Token>(Prod->Head, NewReduction);
            Result = ParseResult::ReduceNormal;
          }

          // ========== Goto
          uint16_t index = Stack_.top()->State;

          // ========= If n is -1 here, then we have an Internal Table Error!!!!
          const LRAction *action = LRStates_[index].GetActionForSymbol(Prod->Head);
          if (action)
          {
            CurrentLALR_ = action->Value;
            Head->State = CurrentLALR_;
            Stack_.push(Head);
          }
          else
          {
            Result = ParseResult::InternalError;
          }
          break;
          }
        default:
          Result = ParseResult::InternalError;
          break;
        }
    }
    else
    {
      // === Syntax Error! Fill Expected Tokens
      ExpectedSymbols_.Clear();
      for (size_t i = 0; i < LRStates_[CurrentLALR_].Actions.Count(); ++i)
      {
        LRAction *Action = &(LRStates_[CurrentLALR_].Actions[i]);
        switch (Action->Sym->Type)
        {
          case Symbol::SymbolType::Content:
          case Symbol::SymbolType::End:
          case Symbol::SymbolType::GroupStart:
          case Symbol::SymbolType::GroupEnd:
            ExpectedSymbols_.Add(*(Action->Sym));
          default:
            break;
        }
      }
      Result = ParseResult::SyntaxError;
    }

    return Result; // Very important
  } //method

  std::shared_ptr<Token> Parser::LookaheadDFA()
  {
    /* This function implements the DFA for the parser's lexer.
    It generates a token which is used by the LALR state
    machine.
    */

    // ===================================================
    // Match DFA token
    // ===================================================

    bool Found = false;
    bool Done = false;
    uint16_t CurrentDFA = DFA_.InitialState;
    size_t CurrentPosition = 1;               // Next byte in the input Stream
    int LastAcceptState = -1;                 // We have not yet accepted a character string
    size_t LastAcceptPosition;                // This used to be initilaized to -1 (and be int) in .NET, but that seems totally useless
    uint16_t Target = (uint16_t)-1;
    std::shared_ptr<Token> Result = std::make_shared<Token>();


    GPCHR_T ch = Lookahead(1);
    if  ((ch !=0) )
    {
      while (!Done)
      {
        /* This code searches all the branches of the current DFA state
        for the next character in the input Stream. If found the
        target state is returned. */

        ch = Lookahead(CurrentPosition);
        if (ch == 0)    // End reached, do not match
          Found = false;
        else
        {
          size_t n = 0;
          Found = false;
          while (n < DFA_[CurrentDFA].Edges.Count() && !Found)
          {
              FaEdge &Edge = DFA_[CurrentDFA].Edges[n];

              // ==== Look for character in the Character Set Table
              if (Edge.Characters->Contains(ch))
              {
                Found = true;
                Target = Edge.Target;
              }
              ++n;
          }
        }

        /* This block-if statement checks whether an edge was found from the current state. If so, the state and current
        position advances. Otherwise it is time to exit the main loop and report the token found (if there was one).
        If the LastAcceptState is -1, then we never found a match and the Error Token is created. Otherwise, a new
        token is created using the Symbol in the Accept State and all the characters that comprise it. */

        if (Found)
        {
          /* This code checks whether the target state accepts a token.
          If so, it sets the appropiate variables so when the
          algorithm in done, it can return the proper token and
          number of characters. */

          if (DFA_[Target].Accept)      // This check is very important!
          {
            LastAcceptState = Target;
            LastAcceptPosition = CurrentPosition;
          }

          CurrentDFA = Target;
          ++CurrentPosition;
        }
        else // No edge found
        {
            Done = true;
            if (LastAcceptState == -1)     // Lexer cannot recognize symbol
            {
              Result->Parent = SymbolTable_.GetFirstOfType(Symbol::SymbolType::Error);
              Result->StringData = LookaheadBuffer(1);
            }
            else                           // Create Token, read characters
            {
              assert(LastAcceptState >= 0);
              Result->Parent = DFA_[(size_t)LastAcceptState].Accept;
              Result->StringData = LookaheadBuffer(LastAcceptPosition);   // Data contains the total number of accept characters
            }
        }
      } // while
    }
    else
    {
        // End of file reached, create End Token
        Result->StringData = GPSTR_C("");
        Result->Parent = SymbolTable_.GetFirstOfType(Symbol::SymbolType::End);
    }

    // ===================================================
    // Set the new token's position information
    // ===================================================
    // Notice, this is a copy, not a linking of an instance. We don't want the user
    // to be able to alter the main value indirectly.
    Result->Pos = SysPosition_;

    return Result;
  } //method

  void Parser::ConsumeBuffer(size_t charCount)
  {
    // Consume/Remove the characters from the front of the buffer.

    if (charCount <= LookaheadBuffer_.size())
    {
      /* Count Carriage Returns and increment the internal column and line
      numbers. This is done for the Developer and is not necessary for the
      DFA algorithm. */
      for (size_t i = 0; i < charCount; ++i)
      {
        switch (LookaheadBuffer_[i])
        {
        case 10: // LF
          SysPosition_.Line += 1;
          SysPosition_.Column = 0;
          break;
        case 13: // CR
          // Ignore, LF is used to increment line to be UNIX friendly
          break;
        default:
          SysPosition_.Column += 1;
          break;
        }
      }

      LookaheadBuffer_ = LookaheadBuffer_.erase(0, charCount);
    } // if
  } // method

  std::shared_ptr<Token> Parser::ProduceToken()
  {
    /* ** VERSION 5.0 **
    This function creates a token and also takes into account the current
    lexing mode of the parser. In particular, it contains the group logic.

    A stack is used to track the current "group". This replaces the comment
    level counter. Also, text is appended to the token on the top of the
    stack. This allows the group text to returned in one chunk. */

    std::shared_ptr<Token> Result;
    bool Done = false;
    bool NestGroup = false;

    while (!Done)
    {
      std::shared_ptr<Token> Read = LookaheadDFA();

      /* The logic - to determine if a group should be nested - requires that the top of the stack
      and the symbol's linked group need to be looked at. Both of these can be unset. So, this section
      sets a Boolean and avoids errors. We will use this boolean in the logic chain below. */
      if (Read->GetType() == Symbol::SymbolType::GroupStart)
      {
        if (GroupStack_.empty())
          NestGroup = true;
        else
          NestGroup = GroupStack_.top()->GetGroup()->Nesting.Contains(Read->GetGroup()->TableIndex);
      }
      else
      {
          NestGroup = false;
      }

      // =================================
      // Logic chain
      // =================================

      if (NestGroup)
      {
        ConsumeBuffer(Read->StringData.size());
        GroupStack_.push(Read);
      }
      else if (GroupStack_.empty())
      {
        // The token is ready to be analyzed.
        ConsumeBuffer(Read->StringData.size());
        Result = Read;
        Done = true;
      }
      else if (GroupStack_.top()->GetGroup()->End == Read->Parent)
      {
        //End the current group
        std::shared_ptr<Token> Pop = GroupStack_.top();
        GroupStack_.pop();

        // === Ending logic
        if (Pop->GetGroup()->Ending == Group::EndingMode::Closed)
        {
          Pop->StringData += Read->StringData;        // Append text
          ConsumeBuffer(Read->StringData.size());  // Consume token
        }

        if (GroupStack_.empty())            // We are out of the group. Return pop'd token (which contains all the group text)
        {
          Pop->Parent = Pop->GetGroup()->Container;  // Change symbol to parent
          Result = Pop;
          Done = true;
        }
        else
        {
          GroupStack_.top()->StringData += Pop->StringData;   // Append group text to parent
        }
      }
      else if (Read->GetType() == Symbol::SymbolType::End)
      {
        // EOF always stops the loop. The caller function (Parse) can flag a runaway group error.
        Result = Read;
        Done = true;
      }
      else
      {
        // We are in a group, Append to the Token on the top of the stack.
        // Take into account the Token group mode
        std::shared_ptr<Token> Top = GroupStack_.top();

        if (Top->GetGroup()->Advance == Group::AdvanceMode::Token)
        {
          Top->StringData += Read->StringData;        // Append all text
          ConsumeBuffer(Read->StringData.size());
        }
        else
        {
          Top->StringData += Read->StringData[0];     // Append one character
          ConsumeBuffer(1);
        }
      } // if
    } // while

    return Result;
  }

  ParseMessage Parser::Parse()
  {
    ParseMessage Message;
    bool Done;
    std::shared_ptr<Token> Read;
    ParseResult Action;

    if (!TablesLoaded_)
      return ParseMessage::NotLoadedError;

    // ===================================
    // Loop until breakable event
    // ===================================
    Done = false;
    while (!Done)
    {
      if (InputTokens_.Count() == 0)
      {
        Read = ProduceToken();
        InputTokens_.Push(Read);

        Message = ParseMessage::TokenRead;
        Done = true;
      }
      else
      {
        Read = InputTokens_.Top();
        CurrentPosition_ = Read->Pos;   // Update current position

        if (GroupStack_.empty() == false)    // Runaway group
        {
          Message = ParseMessage::GroupError;
          Done = true;
        }
        else if (Read->GetType() == Symbol::SymbolType::Noise)
        {
          // Just discard. These were already reported to the user.
          InputTokens_.Pop();
        }
        else if (Read->GetType() == Symbol::SymbolType::Error)
        {
          Message = ParseMessage::LexicalError;
          Done = true;
        }
        else    // Finally, we can parse the token.
        {
          Action = ParseLALR(Read);   // SAME PROCEDURE AS v1
          switch (Action)
          {
            case ParseResult::Accept:
              Message = ParseMessage::Accept;
              Done = true;
              break;
            case ParseResult::InternalError:
              Message = ParseMessage::InternalError;
              Done = true;
              break;
            case ParseResult::ReduceNormal:
              Message = ParseMessage::Reduction;
              Done = true;
              break;
            case ParseResult::Shift:
              // ParseToken() shifted the token on the front of the Token-Queue.
              // It now exists on the Token-Stack and must be eliminated from the queue.
              InputTokens_.Dequeue();
              break;
            case ParseResult::SyntaxError:
              Message = ParseMessage::SyntaxError;
              Done = true;
              break;
            default:
              break;
          } // switch
        } // if
      } // if
    } // while

    return Message;
  }
}
