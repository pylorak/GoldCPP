#include "SimpleParser.h"
#include "Parser.h"
#include <stdexcept>

#ifdef __GNUC__
#define UNUSED __attribute__ ((unused))
#else
#define UNUSED
#endif

namespace GoldCPP
{
    SimpleParser::SimpleParser(const uint8_t* egt_data, size_t len) :
      parser_(NULL), User0(NULL), User1(NULL), Root(NULL)
    {
      parser_ = new Parser();

      if (!parser_->LoadTables(egt_data, len))
      {
        throw std::runtime_error("Could not load EGT parser tables.");
        delete parser_;
      }
    }

    SimpleParser::~SimpleParser()
    {
      delete parser_;
    }

    GPSTR_T SimpleParser::LexicalError(SimpleParser UNUSED *parser, Parser* parserCore)
    {
      //Cannot recognize token
      return GPSTR_T(GPSTR_C("Lexical Error:\n")) +
             GPSTR_C("Position: ") + toString(parserCore->GetCurrentPosition().Line) + GPSTR_C(", ") + toString(parserCore->GetCurrentPosition().Column) + GPSTR_T(GPSTR_C("\n")) +
             GPSTR_C("Read: ") + parserCore->GetCurrentToken()->StringData;
    }
    GPSTR_T SimpleParser::SyntaxError(SimpleParser UNUSED *parser, Parser* parserCore)
    {
      //Expecting a different token
      return GPSTR_T(GPSTR_C("Syntax Error:\n")) +
             GPSTR_C("Position: ") + toString(parserCore->GetCurrentPosition().Line) + GPSTR_C(", ") +  toString(parserCore->GetCurrentPosition().Column) + GPSTR_C("\n") +
             GPSTR_T(GPSTR_C("Read: ")) + parserCore->GetCurrentToken()->StringData + GPSTR_C("\n") +
             GPSTR_T(GPSTR_C("Expecting: ")) + parserCore->GetExpectedSymbols().GetText();
    }
    std::shared_ptr<Reduction> SimpleParser::Reduce(SimpleParser UNUSED *parser, const std::shared_ptr<Reduction> &reduction)
    {
      //For this project, we will let the parser build a tree of Reduction objects
      //parser.CurrentReduction = CreateNewObject(parser.CurrentReduction);
      return reduction;
    }
    GPSTR_T SimpleParser::InternalError(SimpleParser UNUSED *parser)
    {
      //INTERNAL ERROR! Something is horribly wrong.
      return GPSTR_C("Ka-BOOOOM!");
    }
    GPSTR_T SimpleParser::TablesNotLoaded(SimpleParser UNUSED *parser)
    {
      //This error occurs if the EGT was not loaded.
      return GPSTR_C("No parser tables loaded! (Since we load the EGT table with RAII, this case is pretty impossible...)");
    }
    GPSTR_T SimpleParser::Runaway(SimpleParser UNUSED *parser)
    {
      //GROUP ERROR! Unexpected end of file
      return GPSTR_C("Unexpecetd end of input. Probably unclosed structure in input.");
    }

    bool SimpleParser::Parse(const GPSTR_T &source, GPSTR_T &msgOut, bool trimReductions)
    {
      /* This procedure starts the GOLD Parser Engine and handles each of the
      messages it returns. Each time a reduction is made, you can create new
      custom object and reassign the .CurrentReduction property. Otherwise,
      the system will use the Reduction object that was returned.

      The resulting tree will be a pure representation of the language
      and will be ready to implement. */

      ParseMessage response;
      bool done;                      //Controls when we leave the loop
      bool accepted = false;          //Was the parse successful?

      parser_->Open(source);
      parser_->TrimReductions = trimReductions;  //Please read about this feature before enabling

      done = false;
      while (!done)
      {
          response = parser_->Parse();

          switch (response)
          {
              case ParseMessage::LexicalError:
                  //Cannot recognize token
                  msgOut = LexicalError(this, parser_);
                  done = true;
                  break;

              case ParseMessage::SyntaxError:
                  //Expecting a different token
                  msgOut = SyntaxError(this, parser_);
                  done = true;
                  break;

              case ParseMessage::Reduction:
                  parser_->SetCurrentReduction(Reduce(this, parser_->GetCurrentReduction()));
                  break;

              case ParseMessage::Accept:
                  //Accepted!
                  Root = parser_->GetCurrentReduction();    //The root node!
                  done = true;
                  accepted = true;
                  break;

              case ParseMessage::TokenRead:
                  //You don't have to do anything here.
                  break;

              case ParseMessage::InternalError:
                  //INTERNAL ERROR! Something is horribly wrong.
                  msgOut = InternalError(this);
                  done = true;
                  break;

              case ParseMessage::NotLoadedError:
                  //This error occurs if the EGT was not loaded.
                  msgOut = TablesNotLoaded(this);
                  done = true;
                  break;

              case ParseMessage::GroupError:
                  //GROUP ERROR! Unexpected end of file
                  msgOut = Runaway(this);
                  done = true;
                  break;
          }
      } //while

      return accepted;
    }
}
