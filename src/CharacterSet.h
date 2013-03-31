#ifndef GOLDCPP_CHARACTERSET_H
#define GOLDCPP_CHARACTERSET_H

#include "CharacterRange.h"
#include "Vector.h"

namespace GoldCPP
{
  class CharacterSet : public Vector<CharacterRange>
  {
  public:

    CharacterSet(size_t initSize = 0);
    bool Contains(uint32_t c) const;
  };

  typedef Vector<CharacterSet> CharacterSetList;
}

#endif // GOLDCPP_CHARACTERSET_H

