#ifndef GOLDCPP_REDUCTION_H
#define GOLDCPP_REDUCTION_H

#include "Token.h"

namespace GoldCPP
{
  struct Production;

  struct Reduction
  {
    TokenList Branches;
    Production *Parent;
    void *User;

    Reduction(size_t n) :
      Branches(n, NULL),
      Parent(NULL),
      User(NULL)
    {}
  };
}

#endif // GOLDCPP_REDUCTION_H





