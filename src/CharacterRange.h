#ifndef GOLDCPP_CHARACTERRANGE_H
#define GOLDCPP_CHARACTERRANGE_H

#include <cstdint>

namespace GoldCPP
{
  struct CharacterRange
  {
    uint32_t Start;
    uint32_t End;

    CharacterRange()
    {}

    CharacterRange(uint32_t start, uint32_t end)
      : Start(start), End(end)
    {}

  };
}

#endif // GOLDCPP_CHARACTERRANGE_H
