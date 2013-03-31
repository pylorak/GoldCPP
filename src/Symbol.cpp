#include "Symbol.h"
#include "Group.h"

namespace GoldCPP
{

  GPSTR_T Symbol::getLiteralFormat(const GPSTR_T source, bool ForceDelimit) const
  {
    if (source == GPSTR_C("'"))
    {
      return GPSTR_C("''");
    }
    else
    {
      for (size_t n = 0; (n < source.size()) && !ForceDelimit; ++n)
      {
        GPCHR_T ch = source[n];
        ForceDelimit = !(isalpha(ch) || (ch == '.') || (ch == '_') || (ch == '-'));
      }

      if (ForceDelimit)
        return GPSTR_C("'") + source + GPSTR_C("'");
      else
        return source;
    }
  }

  GPSTR_T Symbol::GetText(bool AlwaysDelimitTerminals) const
  {
    switch (Type)
    {
    case SymbolType::Nonterminal:
      return GPSTR_C("<") + Name + GPSTR_C(">");
    case SymbolType::Content:
      return getLiteralFormat(Name, AlwaysDelimitTerminals);
    default:
      return GPSTR_C("(") + Name + GPSTR_C(")");
    }
  }

  GPSTR_T Symbol::GetText() const
  {
    return GetText(false);
  }


  /* ----------------------------------------
                 SymbolList
     ----------------------------------------
  */


  Symbol* SymbolList::GetFirstOfType(Symbol::SymbolType type)
  {
    size_t numItems = Count();
    for (size_t i = 0; i < numItems; ++i)
    {
      if (GetItemAt(i).Type == type)
        return &(GetItemAt(i));
    }

    return NULL;
  }

  GPSTR_T SymbolList::GetText(const GPSTR_T &separator, bool AlwaysDelimitTerminals) const
  {
    GPSTR_T result;
    size_t numItems = Count();

    if (numItems == 0)
      return result;

    result = GetItemAt(0).GetText(AlwaysDelimitTerminals);

    for (size_t i = 1; i < numItems; ++i)
      result += separator + GetItemAt(i).GetText(AlwaysDelimitTerminals);

    return result;
  }

  GPSTR_T SymbolList::GetText() const
  {
    return GetText(GPSTR_C(", "), false);
  }

}
