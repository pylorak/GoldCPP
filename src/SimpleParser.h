#ifndef GoldCPP_SIMPLEPARSER_H
#define GoldCPP_SIMPLEPARSER_H

#include "String.h"
#include <memory>
#include <sstream>

// Not used, but included for consumers
#include "Reduction.h"
#include "Production.h"

namespace GoldCPP
{
  /* Only for native numeric types! */
  template <typename T>
  GPSTR_T toString(T number)
  {
    // Convert to standard string
    std::ostringstream ss;
    ss << number;
    std::string tmp(ss.str());

    // Because character codes 0-127 are identical in ASCII and UTF,
    // this conversion actually works (for latin digits).
    GPSTR_T ret(tmp.size(), ' ');
    for (size_t i = 0; i < tmp.size(); ++i)
      ret[i] = (GPCHR_T)tmp[i];

    return ret;
  }

  class Parser;

  class SimpleParser
  {
  private:
    Parser *parser_;
    SimpleParser(const SimpleParser& that) = delete;

  public:
    void* User0;
    void* User1;
    std::shared_ptr<Reduction> Root;

    SimpleParser(const uint8_t* egt_data, size_t len);
    virtual ~SimpleParser();

    // Override these functions to get custom behavior
    virtual GPSTR_T LexicalError(SimpleParser *parser, Parser* parserCore);
    virtual GPSTR_T SyntaxError(SimpleParser *parser, Parser* parserCore);
    virtual std::shared_ptr<Reduction> Reduce(SimpleParser *parser, const std::shared_ptr<Reduction> &reduction);
    virtual GPSTR_T InternalError(SimpleParser *parser);
    virtual GPSTR_T TablesNotLoaded(SimpleParser *parser);
    virtual GPSTR_T Runaway(SimpleParser *parser);

    bool Parse(const GPSTR_T &source, GPSTR_T &msgOut, bool trimReductions = false);
    Parser* GetParserCore() const { return parser_; }
  };
}

#endif // GoldCPP_SIMPLEPARSER_H

