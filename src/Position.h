#ifndef GOLDCPP_POSITION_H
#define GOLDCPP_POSITION_H

#include <cstdint>

namespace GoldCPP
{
  struct Position
  {
    uint32_t Line;
    uint32_t Column;

    Position()
      : Line(0), Column(0)
    {}
  };
}

#endif // GOLDCPP_POSITION_H



