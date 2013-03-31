#ifndef GOLDCPP_FASTATE_H
#define GOLDCPP_FASTATE_H

#include "Vector.h"
#include "FaEdge.h"
#include <cstdint>

namespace GoldCPP
{
  struct FaEdge;
  struct Symbol;

  struct FaState
  {
    FaEdgeList Edges;
    Symbol *Accept;

    FaState()
      : Edges(), Accept(NULL)
    {}

    FaState(Symbol *accept)
      : Edges(), Accept(accept)
    {}
  };

  class FaStateList : public Vector<FaState>
  {
  public:

    uint16_t InitialState;
    Symbol* ErrorSymbol;

    FaStateList(size_t initSize = 0) :
      Vector<FaState>(initSize, NULL),
      InitialState(0),
      ErrorSymbol(NULL)
    {}

    virtual void Clear()
    {
      InitialState = 0;
      ErrorSymbol = NULL;
      Vector<FaState>::Clear();
    }
  };
}

#endif // GOLDCPP_FASTATE_H


