#include "LrState.h"

namespace GoldCPP
{
  size_t LRState::FindActionForSymbol(const Symbol *sym) const
  {
    size_t numItems = Actions.Count();
    for (size_t i = 0; i < numItems; ++i)
    {
      if (Actions[i].Sym == sym)
        return i;
    }

    return INVALID_IDX;
  }

  /* This function replaces the subscript overload of the .NET implementation
  which is indexed by the Symbol */
  const LRAction* LRState::GetActionForSymbol(const Symbol *sym) const
  {
    size_t index = FindActionForSymbol(sym);
    if (index != INVALID_IDX)
      return &Actions[index];
    else
      return NULL;
  }
}
