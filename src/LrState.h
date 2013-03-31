#ifndef GOLDCPP_LRSTATE_H
#define GOLDCPP_LRSTATE_H

#include "Vector.h"
#include <cstdint>

namespace GoldCPP
{
  struct Symbol;

  enum class LRConflict
  {
    ShiftShift = 1,      // Never happens
    ShiftReduce = 2,
    ReduceReduce = 3,
    AcceptReduce = 4,    // Never happens with this implementation
    None = 5
  };

  enum class LRActionType
  {
    Shift = 1, // Shift a symbol and goto a state
    Reduce = 2, // Reduce by a specified rule
    Goto = 3, // Goto to a state on reduction
    Accept = 4, // Input successfully parsed
    Error = 5, // Programmars see this often!
  };

  struct LRAction
  {
    Symbol* Sym;
    LRActionType Type;
    uint16_t Value;

    LRAction() :
      Sym(NULL), Type(LRActionType::Error), Value((uint16_t)-1)
    {}

    LRAction(Symbol *sym, LRActionType type, uint16_t value) :
      Sym(sym), Type(type), Value(value)
    {}
  };

  class LRState
  {
  public:

    Vector<LRAction> Actions;

    /* This function replaces the subscript overload of the .NET implementation
    which is indexed by the Symbol */
    const LRAction* GetActionForSymbol(const Symbol *sym) const;

  private:

    size_t FindActionForSymbol(const Symbol *sym) const;

  };

  class LRStateList : public Vector<LRState>
  {
  public:

    uint16_t InitialState;

    LRStateList(size_t initSize = 0) :
      Vector<LRState>(initSize),
      InitialState(0)
    {}

    virtual void Clear()
    {
      InitialState = 0;
      Vector<LRState>::Clear();
    }
  };
}

#endif // GOLDCPP_LRSTATE_H




