#ifndef GOLDCPP_GROUP_H
#define GOLDCPP_GROUP_H

#include "Vector.h"
#include "String.h"
#include <cstdint>

namespace GoldCPP
{
  class Symbol;

  struct Group
  {
    enum AdvanceMode : uint16_t
    {
      Token = 0,
      Character = 1
    };

    enum EndingMode : uint16_t
    {
      Open = 0,
      Closed = 1
    };

    Symbol* Container;
    Symbol* Start;
    Symbol* End;
    uint16_t TableIndex;
    GPSTR_T Name;
    AdvanceMode Advance;
    EndingMode Ending;
    Vector<uint16_t> Nesting;

    Group() :
      Container(NULL), Start(NULL), End(NULL),
      TableIndex(0), Name(),
      Advance(AdvanceMode::Character), Ending(EndingMode::Closed),
      Nesting()
    {}

  };

  typedef Vector<Group> GroupList;
}

#endif // GOLDCPP_GROUP_H



