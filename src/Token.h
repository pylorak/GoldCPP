#ifndef GOLDCPP_TOKEN_H
#define GOLDCPP_TOKEN_H

#include "Symbol.h"
#include "Position.h"
#include "Vector.h"
#include "String.h"
#include <cstdint>
#include <stack>
#include <list>
#include <memory>

namespace GoldCPP
{
  class Symbol;
  class Reduction;

  /* Note: The ReductionData and StringData members used to be
   a single, unified "Data" member (of type "Object")
   in the reference implementation. I chose to separate
   them into separate and type-safe variables. */

  struct Token
  {
    Symbol* Parent;
    std::shared_ptr<Reduction> ReductionData;
    GPSTR_T StringData;
    uint16_t State;
    Position Pos;

    Token() :
      Parent(NULL), ReductionData(NULL), State(0)
    {}

    Token(Symbol *parent, const std::shared_ptr<Reduction> &data) :
      Parent(parent), ReductionData(data), State(0)
    {}

    Symbol::SymbolType GetType() const
    {
      return Parent->Type;
    }

    Group* GetGroup() const
    {
      return Parent->GoldGroup;
    }
  };

  typedef Vector<std::shared_ptr<Token>> TokenList;
  typedef std::stack<std::shared_ptr<Token>> TokenStack;

  class TokenQueueStack   // Hybrid stack and queue
  {
  private:
    std::list<std::shared_ptr<Token>>  list_;

  public:

    size_t Count() const
    {
      return list_.size();
    }

    void Clear()
    {
      list_.clear();
    }

    void Enqueue(const std::shared_ptr<Token> &token)
    {
      list_.push_back(token);
    }

    void Push(const std::shared_ptr<Token> &token)
    {
      list_.push_front(token);
    }

    std::shared_ptr<Token> Dequeue()
    {
      std::shared_ptr<Token> ret = list_.front();
      list_.pop_front();
      return ret;
    }

    std::shared_ptr<Token> Pop()
    {
      return Dequeue();
    }

    std::shared_ptr<Token> Top() const
    {
      if (Count() > 0)
        return list_.front();
      else
        return NULL;
    }

  };

}

#endif // GOLDCPP_TOKEN_H





