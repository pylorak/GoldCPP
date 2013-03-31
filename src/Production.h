#ifndef GOLDCPP_PRODUCTION_H
#define GOLDCPP_PRODUCTION_H

#include "String.h"
#include "Symbol.h"
#include "Vector.h"

#include <cstdint>

namespace GoldCPP
{
  struct Production
  {
    Symbol* Head;
    SymbolList Handle;
    uint16_t TableIndex;

    Production() :
      Head(NULL), Handle(0), TableIndex((uint16_t)-1)
    {}

    Production(Symbol *head, uint16_t tableIndex) :
      Head(head), Handle(0), TableIndex(tableIndex)
    {}

    GPSTR_T GetText(bool AlwaysDelimitTerminals = false) const
    {
      return Head->GetText() + GPSTR_C(" ::= ") + Handle.GetText(GPSTR_C(" "), AlwaysDelimitTerminals);
    }

    bool ContainsOneNonTerminal() const
    {
      if (Handle.Count() == 1)
      {
        if (Handle[0].Type == Symbol::SymbolType::Nonterminal)
          return true;
      }

      return false;
    }
  };

  typedef Vector<Production> ProductionList;

}

#endif // GOLDCPP_PRODUCTION_H




