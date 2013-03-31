#ifndef GOLDCPP_SYMBOL_H
#define GOLDCPP_SYMBOL_H

#include "String.h"
#include "Vector.h"
#include <cstdint>

namespace GoldCPP
{
  struct Group;

  struct Symbol
  {
  public:
    enum SymbolType : uint16_t
    {
      Nonterminal = 0,
      Content = 1,
      Noise = 2,
      End = 3,
      GroupStart = 4,
      GroupEnd = 5,
      Error = 7
    };

    Group* GoldGroup;
    GPSTR_T Name;
    SymbolType Type;
    uint32_t TableIndex;

    GPSTR_T GetText(bool AlwaysDelimitTerminals) const;
    GPSTR_T GetText() const;

    Symbol()
      : GoldGroup(NULL), Name(), Type(SymbolType::Nonterminal), TableIndex(0)
    {}

    Symbol(const GPSTR_T &name, SymbolType type, uint32_t tindex)
      : GoldGroup(NULL), Name(name), Type(type), TableIndex(tindex)
    {}

    GPSTR_T getLiteralFormat(const GPSTR_T source, bool ForceDelimit) const;
  };


  class SymbolList : public Vector<Symbol>
  {
  public:

    Symbol* GetFirstOfType(Symbol::SymbolType type);
    GPSTR_T GetText(const GPSTR_T &separator, bool AlwaysDelimitTerminals) const;
    GPSTR_T GetText() const;

    SymbolList(size_t initSize = 0) :
      Vector<Symbol>(initSize)
    {}

  };

}

#endif // GOLDCPP_SYMBOL_H


