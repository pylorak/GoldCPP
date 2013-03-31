#include "CharacterSet.h"

namespace GoldCPP
{
  CharacterSet::CharacterSet(size_t initSize) :
    Vector<CharacterRange>(initSize)
  {}

  bool CharacterSet::Contains(uint32_t c) const
  {
    /* Sets don't usually have a lot of ranges, so we just search linearly */

    size_t numItems = Count();
    for (size_t i = 0; i < numItems; ++i)
    {
      const CharacterRange &range = GetItemAt(i);
      if ((c >= range.Start) && (c <= range.End))
        return true;
    }

    return false;
  }
}
