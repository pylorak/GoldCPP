#ifndef GOLDCPP_PARSER_H
#define GOLDCPP_PARSER_H

#include "String.h"
#include "Symbol.h"
#include "FaState.h"
#include "CharacterSet.h"
#include "Production.h"
#include "LrState.h"
#include "Token.h"
#include "Group.h"

// Not used, but included for consumers
#include "Reduction.h"
#include "Production.h"

namespace GoldCPP
{
  enum class ParseMessage
  {
    TokenRead = 0,         // A new token is read
    Reduction = 1,         // A production is reduced
    Accept = 2,            // Grammar complete
    NotLoadedError = 3,    // The tables are not loaded
    LexicalError = 4,      // Token not recognized
    SyntaxError = 5,       // Token is not expected
    GroupError = 6,        // Reached the end of the file inside a block
    InternalError = 7      // Something is wrong, very wrong
  };

  // The ParseLALR() function returns this value
  enum class ParseResult
  {
    Accept = 1,
    Shift = 2,
    ReduceNormal = 3,
    ReduceEliminated = 4 ,           // Trim
    SyntaxError = 5,
    InternalError = 6
  };

  class GrammarProperties
  {

  private:
    #define GOLD_CPP_GRAMMAR_PROPERTY_COUNT 8
    GPSTR_T _Properties[GOLD_CPP_GRAMMAR_PROPERTY_COUNT];

  public:
    enum PropertyIndex
    {
      PropName = 0,
      PropVersion = 1,
      PropAuthor = 2,
      PropAbout = 3,
      PropCharacterSet = 4,
      PropCharacterMapping = 5,
      PropGeneratedBy = 6,
      PropGeneratedDate = 7
    };

    GPSTR_T getProperty(PropertyIndex index) const;
    void setProperty(PropertyIndex index, const GPSTR_T &val);
  };

  class Parser
  {
  private:

    static const GPSTR_T kVersion_;

    // ===== Symbols recognized by the system
    SymbolList SymbolTable_;

    // ===== DFA
    FaStateList DFA_;
    CharacterSetList CharSetTable_;
    GPSTR_T LookaheadBuffer_;

    // ===== Productions
    ProductionList ProductionTable_;

    // ===== LALR
    LRStateList LRStates_;
    uint16_t CurrentLALR_;
    TokenStack Stack_;

    // ===== Used for Reductions & Errors
    SymbolList ExpectedSymbols_;       // This ENTIRE list will available to the user
    bool HaveReduction_;

    // ===== Private control variables
    bool TablesLoaded_;
    TokenQueueStack InputTokens_;  // Tokens to be analyzed - Hybred object!

    // === Line and column information.
    Position SysPosition_;        // Internal - so user cannot mess with values
    Position CurrentPosition_;    // Last read terminal

    // ===== Lexical Groups
    TokenStack GroupStack_;
    GroupList GroupTable_;

    ParseResult ParseLALR(const std::shared_ptr<Token> &NextToken);
    std::shared_ptr<Token> LookaheadDFA();
    void ConsumeBuffer(size_t charCount);
    std::shared_ptr<Token> ProduceToken();

#ifndef __GNUC__
    Parser(const Parser& that){};
#else
    Parser(const Parser& that) = delete;
#endif

  public:

    /* Determines if reductions will be trimmed in cases where a production
    contains a single element. */
    bool TrimReductions;

    /* Returns information about the current grammar. */
    GrammarProperties Grammar;

    Parser();

    /* Specifies the text to be parsed */
    bool Open(const GPSTR_T &source);

    /* Restarts the parser. Loaded tables are retained.
    Open() calls this internally,
    so there is rarely a need to call Restart() manually. */
    void Restart();

    /* When the Parse() method returns a Reduce, this method will
    contain the current Reduction. */
    std::shared_ptr<Reduction> GetCurrentReduction();

    void SetCurrentReduction(const std::shared_ptr<Reduction> &value);

    /* Current line and column being read from the source. */
    Position GetCurrentPosition() const;

    /* If the Parse() function returns TokenRead,
    this method will return that last read token. */
    std::shared_ptr<Token> GetCurrentToken() const;

    /* Removes the next token from the input queue. */
    std::shared_ptr<Token> DiscardCurrentToken();

    /* Added a token onto the end of the input queue. */
    void EnqueueInput(const std::shared_ptr<Token> &token);

    /* Pushes the token onto the top of the input queue.
    This token will be analyzed next. */
    void PushInput(const std::shared_ptr<Token> &token);

    GPSTR_T LookaheadBuffer(size_t count) const;

    GPCHR_T Lookahead(size_t charIndex) const;

    /* Library name and version. */
    GPSTR_T GetAbout() const;

    /* Resets all state and frees internal objects.
    LoadTables() calls this internally,
    so there is rarely a need to call Clear() manually. */
    void Clear();

    /* Loads parse tables from the specified BinaryReader. Only EGT (version 5.0) is supported. */
    bool LoadTables(const uint8_t* binstream, size_t len);

    /* Returns a list of Symbols recognized by the grammar. */
    SymbolList GetSymbolTable() const;

    /* Returns a list of Productions recognized by the grammar. */
    ProductionList GetProductionTable() const;

    /* If the Parse() method returns a SyntaxError, this method will contain a list of
    the symbols the grammar expected to see. */
    SymbolList GetExpectedSymbols() const;

    /* Returns true if parse tables were loaded. */
    bool TablesLoaded() const;

    /* Performs a parse action on the input. This method is typically used in a loop
    until either grammar is accepted or an error occurs. */
    ParseMessage Parse();

  };
}

#endif // GOLDCPP_PARSER_H



