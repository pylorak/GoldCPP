#ifndef GOLDCPP_FAEDGE_H
#define GOLDCPP_FAEDGE_H

#include "Vector.h"
#include <cstdint>

namespace GoldCPP
{
  class CharacterSet;

  struct FaEdge
  {
    CharacterSet *Characters;
    uint16_t Target;

    FaEdge()
      : Characters(NULL), Target((uint16_t)-1)
    {}

    FaEdge(CharacterSet *chars, uint16_t target)
      : Characters(chars), Target(target)
    {}
  };

  typedef Vector<FaEdge> FaEdgeList;
}

#endif // GOLDCPP_FAEDGE_H

